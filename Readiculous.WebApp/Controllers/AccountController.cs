using Readiculous.Data.Models;
using Readiculous.Services.Interfaces;
using Readiculous.Services.Manager;
using Readiculous.Services.ServiceModels;
using Readiculous.WebApp.Authentication;
using Readiculous.WebApp.Models;
using Readiculous.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using static Readiculous.Resources.Constants.Enums;
using Readiculous.Resources.Messages;

namespace Readiculous.WebApp.Controllers
{
    [AllowAnonymous]
    public class AccountController : ControllerBase<AccountController>
    {
        private readonly SessionManager _sessionManager;
        private readonly SignInManager _signInManager;
        private readonly TokenValidationParametersFactory _tokenValidationParametersFactory;
        private readonly TokenProviderOptionsFactory _tokenProviderOptionsFactory;
        private readonly IConfiguration _appConfiguration;
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="signInManager">The sign in manager.</param>
        /// <param name="localizer">The localizer.</param>
        /// <param name="userService">The user service.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="tokenValidationParametersFactory">The token validation parameters factory.</param>
        /// <param name="tokenProviderOptionsFactory">The token provider options factory.</param>
        public AccountController(
                            SignInManager signInManager,
                            IHttpContextAccessor httpContextAccessor,
                            ILoggerFactory loggerFactory,
                            IConfiguration configuration,
                            IMapper mapper,
                            IUserService userService,
                            TokenValidationParametersFactory tokenValidationParametersFactory,
                            TokenProviderOptionsFactory tokenProviderOptionsFactory) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._sessionManager = new SessionManager(this._session);
            this._signInManager = signInManager;
            this._tokenProviderOptionsFactory = tokenProviderOptionsFactory;
            this._tokenValidationParametersFactory = tokenValidationParametersFactory;
            this._appConfiguration = configuration;
            this._userService = userService;
        }

        /// <summary>
        /// Login Method
        /// </summary>
        /// <returns>Created response view</returns>
        [HttpGet]
        public ActionResult Login()
        {
            TempData["returnUrl"] = System.Net.WebUtility.UrlDecode(HttpContext.Request.Query["ReturnUrl"]);
            this._sessionManager.Clear();
            this._session.SetString("SessionId", System.Guid.NewGuid().ToString());
            return this.View();
        }

        /// <summary>
        /// Register Method - Shows registration page
        /// </summary>
        /// <returns>Registration view</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new EmailRequestModel());
        }

        /// <summary>
        /// Request OTP for registration - Handles email submission
        /// </summary>
        /// <param name="model">Email request model</param>
        /// <returns>Redirects to OTP verification page</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RequestOtp(EmailRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Register", model);
            }

            var success = await _userService.SendOtpForRegistrationAsync(model.Email.Trim());
            if (success)
            {
                TempData["SuccessMessage"] = "OTP has been sent to your email address.";
                TempData["EmailForOtp"] = model.Email;
                return RedirectToAction("VerifyOtp");
            }
            else
            {
                TempData["ErrorMessage"] = "Email already exists or failed to send OTP.";
                return View("Register", model);
            }
        }

        /// <summary>
        /// Verify OTP page - Shows OTP entry form
        /// </summary>
        /// <returns>OTP verification view</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyOtp()
        {
            var email = TempData["EmailForOtp"]?.ToString();
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Register");
            }

            var model = new OtpVerificationModel { Email = email };
            return View(model);
        }

        /// <summary>
        /// Verify OTP - Handles OTP verification and user creation
        /// </summary>
        /// <param name="model">OTP verification model</param>
        /// <returns>Redirects to registration success page</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp(OtpVerificationModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var isValid = _userService.ValidateOtpForRegistration(model.Email.Trim(), model.Otp.Trim());
            if (isValid)
            {
                try
                {
                    var tempPassword = Readiculous.Services.Manager.OtpManager.GenerateTempPassword();
                    var userModel = new UserViewModel
                    {
                        UserId = Guid.NewGuid().ToString(),
                        Email = model.Email,
                        Username = model.Email,
                        Password = tempPassword,
                        Role = RoleType.Reviewer
                    };

                    await _userService.AddUserAsync(userModel, userModel.UserId);
                    await _userService.SendTempPasswordEmailAsync(model.Email, tempPassword);

                    // Store success message for landing page
                    TempData["SuccessMessage"] = "Registration successful! Check your email for your temporary password.";

                    return RedirectToAction("LandingScreen", "Home");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Failed to create user or send email. " + ex.Message;
                    return View(model);
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid OTP. Please try again.";
                return View(model);
            }
        }

        /// <summary>
        /// Registration Success page
        /// </summary>
        /// <returns>Registration success view</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterSuccess()
        {
            var userId = TempData["RegistrationSuccessUserId"]?.ToString();
            var username = TempData["RegistrationSuccessUsername"]?.ToString();
            var email = TempData["RegistrationSuccessEmail"]?.ToString();
            var password = TempData["RegistrationSuccessPassword"]?.ToString();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return RedirectToAction("Register");
            }

            var successModel = new RegisterSuccessfulViewModel
            {
                UserId = userId,
                Username = username,
                Email = email,
                Password = password
            };

            return View(successModel);
        }

        /// <summary>
        /// Authenticate user and signs the user in when successful.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns> Created response view </returns>
        /// 


        //[HttpPost]
        //public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        //{
        //    this._session.SetString("HasSession", "Exist");

        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    try
        //    {
        //        User user = null;
        //        var loginResult = _userService.AuthenticateUserByEmail(model.Email, model.Password, ref user);
        //        if (loginResult == LoginResult.Success)
        //        {
        //            if (user.AccessStatus != AccessStatus.FirstTime && user.AccessStatus != AccessStatus.Verified)
        //            {
        //                TempData["ErrorMessage"] = "Your account is not allowed to login. Please contact support.";
        //                return View();
        //            }

        //            await this._signInManager.SignInAsync(user);
        //            this._session.SetString("UserName", user.Username);
        //            // Pass AccessStatus to the view for modal logic
        //            TempData["AccessStatus"] = user.AccessStatus.ToString();
        //            return RedirectToAction("Index", "Home");
        //        }
        //        else
        //        {
        //            TempData["ErrorMessage"] = Resources.Messages.Errors.IncorrectLoginCredentials;
        //            return View(model);
        //        }
        //    }
        //    catch (InvalidDataException ex)
        //    {
        //        TempData["ErrorMessage"] = ex.Message;
        //        return View(model);
        //    }
        //    catch (Exception)
        //    {
        //        TempData["ErrorMessage"] = Resources.Messages.Errors.ServerError;
        //        // Optionally log ex.Message
        //        return View(model);
        //    }
        //}

        [HttpPost]

        public async Task<IActionResult> LoginAjax([FromForm] LoginViewModel model)
        {
            _session.SetString("HasSession", "Exist");

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid input." });
            }

            try
            {
                User user = null;
                var loginResult = _userService.AuthenticateUserByEmail(model.Email, model.Password, ref user);
                if (loginResult == LoginResult.Success)
                {
                    if (user.AccessStatus != AccessStatus.FirstTime && user.AccessStatus != AccessStatus.Verified)
                    {
                        return Json(new { success = false, message = "Your account is not allowed to login. Please contact support." });
                    }

                    await _signInManager.SignInAsync(user);
                    _session.SetString("UserRole", user.Role.ToString());

                    return Json(new { success = true, redirectUrl = Url.Action("DashboardScreen", "Dashboard") });
                }
                else
                {
                    return Json(new { success = false, message = "Incorrect email or password." });
                }
            }
            catch (InvalidDataException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "A server error occurred. Please try again later." });
            }
        }



        [HttpPost]

        public async Task<IActionResult> RegisterAjax([FromForm] EmailRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Valid email required." });
            }

            var success = await _userService.SendOtpForRegistrationAsync(model.Email.Trim());
            return Json(new
            {
                success = success,
                message = success ? "OTP sent!" : "Email exists or failed to send OTP."
            });
        }

        [HttpPost]

        public async Task<IActionResult> VerifyOtpAjax([FromForm] OtpVerificationModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Otp))
            {
                Console.WriteLine("Invalid input: Email or OTP is empty.");
                return Json(new { success = false, message = "Invalid input." });
            }

            var isValid = _userService.ValidateOtpForRegistration(model.Email.Trim(), model.Otp.Trim());
            Console.WriteLine($"OTP validation for {model.Email}: {isValid}");

            if (isValid)
            {
                try
                {
                    var tempPassword = Readiculous.Services.Manager.OtpManager.GenerateTempPassword();
                    var userModel = new UserViewModel
                    {
                        UserId = Guid.NewGuid().ToString(),
                        Email = model.Email,
                        Username = model.Email,
                        Password = tempPassword,
                        Role = RoleType.Reviewer
                    };
                    Console.WriteLine($"Attempting to create user: {model.Email}");
                    await _userService.AddUserAsync(userModel, userModel.UserId);
                    Console.WriteLine($"User created: {model.Email}. Sending temp password...");
                    await _userService.SendTempPasswordEmailAsync(model.Email, tempPassword);
                    Console.WriteLine($"Temp password sent to: {model.Email}");

                    return Json(new { success = true, message = "OTP verified! Check your email for your temporary password." });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception during user creation or email sending: {ex.Message}");
                    return Json(new { success = false, message = "Failed to create user or send email. " + ex.Message });
                }
            }
            else
            {
                Console.WriteLine("Invalid OTP entered.");
                return Json(new { success = false, message = "Invalid OTP. Please try again." });
            }
        }


        [HttpPost]

        public async Task<IActionResult> ResendOtpAjax([FromBody] EmailRequestModel model)
        {
            var success = await _userService.SendOtpForRegistrationAsync(model.Email.Trim());
            return Json(new
            {
                success = success,
                message = success ? "OTP resent!" : "Failed to resend OTP."
            });
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResendOtp(string email)
        {
            var success = await _userService.ResendOtpForRegistrationAsync(email);
            if (success)
            {
                TempData["SuccessMessage"] = "New OTP has been sent to your email address.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to resend OTP. Please try again.";
            }

            var model = new OtpVerificationModel { Email = email };
            return View("VerifyOtp", model);
        }

        /// <summary>
        /// Sign Out current account and return landing page view.
        /// </summary>
        /// <returns>Created response view</returns>
        public async Task<IActionResult> SignOutUser()
        {
            await this._signInManager.SignOutAsync();
            HttpContext.Session.Clear(); // Clear session data on logout
            return RedirectToAction("LandingScreen", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> CompleteFirstTimeProfile([FromForm] string Username, [FromForm] string Password, [FromForm] string ConfirmPassword, [FromForm] IFormFile ProfilePicture)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password) || Password != ConfirmPassword)
            {
                return BadRequest();
            }
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var user = _userService.GetUserEditById(userId);
            user.Username = Username;
            user.Password = Password;
            if (ProfilePicture != null && ProfilePicture.Length > 0)
                user.ProfilePicture = ProfilePicture;
            user.AccessStatus = AccessStatus.Verified;
            await _userService.UpdateUserAsync(user, userId);
            return Ok();
        }

        // Forgot Password Methods
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new EmailRequestModel());
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RequestForgotPasswordOtp(EmailRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("ForgotPassword", model);
            }

            var success = await _userService.SendOtpForForgotPasswordAsync(model.Email);
            if (success)
            {
                TempData["SuccessMessage"] = "OTP has been sent to your email address.";
                TempData["EmailForForgotPasswordOtp"] = model.Email;
                return RedirectToAction("ForgotPasswordOtp");
            }
            else
            {
                TempData["ErrorMessage"] = Errors.ForgotPasswordEmailFailed;
                return View("ForgotPassword", model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordOtp()
        {
            var email = TempData["EmailForForgotPasswordOtp"]?.ToString();
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }

            var model = new OtpVerificationModel { Email = email };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyForgotPasswordOtp(OtpVerificationModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("ForgotPasswordOtp", model);
            }

            var isValid = _userService.ValidateOtpForForgotPassword(model.Email, model.Otp);
            if (isValid)
            {
                TempData["EmailForPasswordReset"] = model.Email;
                return RedirectToAction("ResetPassword");
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid OTP. Please try again.";
                return View("ForgotPasswordOtp", model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResendForgotPasswordOtp([FromBody] EmailRequestModel model)
        {
            var success = await _userService.ResendOtpForForgotPasswordAsync(model.Email);
            return Json(new { success = success });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword()
        {
            var email = TempData["EmailForPasswordReset"]?.ToString();
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }

            var model = new ForgotPasswordModel { Email = email };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await _userService.UpdatePasswordAsync(model.Email, model.NewPassword);
            if (success)
            {
                // Store success message for landing page
                TempData["SuccessMessage"] = "Password reset successful! You can now sign in with your new password.";

                return RedirectToAction("LandingScreen", "Home");
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update password. Please try again.";
                return View(model);
            }
        }

        /// <summary>
        /// Forgot Password Success page
        /// </summary>
        /// <returns>Forgot password success view</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordSuccess()
        {
            return View();
        }
    }
}
