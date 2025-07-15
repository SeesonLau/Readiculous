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
        /// Authenticate user and signs the user in when successful.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns> Created response view </returns>
        /// 


        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            this._session.SetString("HasSession", "Exist");

            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Invalid input." });
                return View(model);
            }

            try
            {
                User user = null;
                var loginResult = _userService.AuthenticateUserByEmail(model.Email, model.Password, ref user);
                if (loginResult == LoginResult.Success)
                {
                    if (user.AccessStatus == AccessStatus.FirstTime)
                    {
                        await this._signInManager.SignInAsync(user, isPersistent: true);
                        this._session.SetString("UserName", user.Username);
                        TempData["AccessStatus"] = user.AccessStatus.ToString();
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = true, firstTime = true });
            }
            else
            {
                            return RedirectToAction("FirstTimeProfile");
                        }
                    }
                    if (user.AccessStatus != AccessStatus.Verified)
                    {
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                            return Json(new { success = false, message = "Your account is not allowed to login. Please contact support." });
                        TempData["ErrorMessage"] = "Your account is not allowed to login. Please contact support.";
                        return View();
                    }

                    await this._signInManager.SignInAsync(user, isPersistent: true);
                    this._session.SetString("UserName", user.Username);
                    TempData["AccessStatus"] = user.AccessStatus.ToString();

                    // Redirect based on role
                    var isAdmin = user.Role == RoleType.Admin;
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        var redirectUrl = isAdmin
                            ? Url.Action("AdminScreen", "DashboardAdmin")
                            : Url.Action("DashboardScreen", "Dashboard");
                        return Json(new { success = true, redirectUrl });
                    }
                    else
                    {
                        if (isAdmin)
                            return RedirectToAction("AdminScreen", "DashboardAdmin");
                        else
                            return RedirectToAction("DashboardScreen", "Dashboard");
                    }
                }
                else
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return Json(new { success = false, message = "Wrong Email or Password" });
                    TempData["ErrorMessage"] = "Wrong Email or Password";
                    return View(model);
                }
            }
            catch (InvalidDataException ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = ex.Message });
                TempData["ErrorMessage"] = ex.Message;
                return View(model);
            }
            catch (Exception)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "A server error occurred. Please try again later." });
                TempData["ErrorMessage"] = "A server error occurred. Please try again later.";
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult FirstTimeProfile()
        {
            // Return a partial/modal for first time profile completion
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var user = _userService.GetUserEditById(userId);
            return PartialView("~/Views/Shared/_FirstTimeProfilePartial.cshtml", user);
        }

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

                    await _signInManager.SignInAsync(user, isPersistent: true);
                    _session.SetString("UserRole", user.Role.ToString());

                    // Redirect based on role
                    var isAdmin = user.Role == RoleType.Admin;
                    var redirectUrl = isAdmin
                        ? Url.Action("DashboardScreen", "DashboardAdmin")
                        : Url.Action("DashboardScreen", "Dashboard");
                    return Json(new { success = true, redirectUrl });
                }
                else
                {
                    return Json(new { success = false, message = "Wrong Email or Password" });
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
        public async Task<IActionResult> RequestSignupOtp(EmailRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Valid email required." });
                return View("Register", model);
            }

            // Signup: Email must NOT exist
            if (_userService.EmailExists(model.Email.Trim()))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Email already exists." });
                TempData["ErrorMessage"] = "Email already exists.";
                return View("Register", model);
            }

            var success = await _userService.SendOtpForRegistrationAsync(model.Email.Trim());
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = success, message = success ? "OTP sent!" : "Failed to send OTP." });
            if (success)
            {
                TempData["SuccessMessage"] = "OTP has been sent to your email address.";
                TempData["EmailForOtp"] = model.Email;
                return RedirectToAction("VerifyOtp");
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to send OTP.";
                return View("Register", model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RequestForgotPasswordOtp(EmailRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Valid email required." });
                return View("ForgotPassword", model);
            }

            // Forgot password: Email must exist
            if (!_userService.EmailExists(model.Email.Trim()))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Email does not exist." });
                TempData["ErrorMessage"] = "Email does not exist.";
                return View("ForgotPassword", model);
            }

            var success = await _userService.SendOtpForForgotPasswordAsync(model.Email.Trim());
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = success, message = success ? "OTP sent!" : "Failed to send OTP." });
            if (success)
            {
                TempData["SuccessMessage"] = "OTP has been sent to your email address.";
                TempData["EmailForForgotPasswordOtp"] = model.Email;
                return RedirectToAction("ForgotPasswordOtp");
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to send OTP.";
                return View("ForgotPassword", model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> VerifySignupOtp(OtpVerificationModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Invalid input." });
                return View(model);
            }

            // Signup: Email must NOT exist (should not happen, but double check)
            if (_userService.EmailExists(model.Email.Trim()))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Email already exists." });
                TempData["ErrorMessage"] = "Email already exists.";
                return View(model);
            }

            var isValid = _userService.ValidateOtpForRegistration(model.Email.Trim(), model.Otp.Trim());
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (isValid)
                {
                    // Create user and send temp password
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
                        TempData["SuccessMessage"] = "Registration complete! Check your email for your temporary password.";
                        return Json(new { success = true });
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, message = "Failed to create user or send email. " + ex.Message });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Invalid OTP. Please try again." });
                }
            }
            if (isValid)
            {
                // ... (non-AJAX, not used in SPA)
                TempData["SuccessMessage"] = "Registration complete! Check your email for your temporary password.";
                return RedirectToAction("LandingScreen", "Home");
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid OTP. Please try again.";
            return View(model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyForgotPasswordOtp(OtpVerificationModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Invalid input." });
                return View("ForgotPasswordOtp", model);
            }

            // Forgot password: Email must exist
            if (!_userService.EmailExists(model.Email.Trim()))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Email does not exist." });
                TempData["ErrorMessage"] = "Email does not exist.";
                return View("ForgotPasswordOtp", model);
            }

            var isValid = _userService.ValidateOtpForForgotPassword(model.Email, model.Otp);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (isValid)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Invalid OTP. Please try again." });
                }
            }
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
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Invalid input." });
                return View(model);
            }

            var success = await _userService.UpdatePasswordAsync(model.Email, model.NewPassword);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (success)
                {
                    TempData["SuccessMessage"] = "Your password has been successfully reset!";
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update password. Please try again." });
                }
            }
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

        // Unified AJAX auth partial loader
        [HttpGet]
        public IActionResult AuthPartial(string view, string email = null, string flow = null)
        {
            switch (view)
            {
                case "login":
                    return PartialView("~/Views/Shared/_LoginPartial.cshtml", new LoginViewModel());
                case "register":
                    return PartialView("~/Views/Shared/_RegisterPartial.cshtml", new EmailRequestModel());
                case "forgot":
                    return PartialView("~/Views/Shared/_ForgotPasswordPartial.cshtml", new EmailRequestModel());
                case "otp":
                    ViewBag.OtpFlow = (flow == "forgot") ? "forgot" : "signup";
                    return PartialView("~/Views/Shared/_OtpPartial.cshtml", new OtpVerificationModel { Email = email });
                case "reset":
                    return PartialView("~/Views/Shared/_ResetPasswordPartial.cshtml", new ForgotPasswordModel { Email = email });
                case "success":
                    if (flow == "signup") {
                        ViewBag.Title = "All done";
                        ViewBag.Message = "Check your email for your temporary password.";
                    } else if (flow == "forgot") {
                        ViewBag.Title = "All done";
                        ViewBag.Message = "Your password has been reset.";
                    } else if (flow == "firsttime") {
                        ViewBag.Title = "All done";
                        ViewBag.Message = "Welcome to Readiculous";
                    } else {
                        ViewBag.Title = "All done";
                        ViewBag.Message = TempData["SuccessMessage"] ?? "Success!";
                    }
                    return PartialView("~/Views/Shared/_SuccessPartial.cshtml");
                default:
                    return PartialView("~/Views/Shared/_LoginPartial.cshtml", new LoginViewModel());
            }
        }
    }
}
