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
        [AllowAnonymous]
        public ActionResult Login()
        {
            TempData["returnUrl"] = System.Net.WebUtility.UrlDecode(HttpContext.Request.Query["ReturnUrl"]);
            this._sessionManager.Clear();
            this._session.SetString("SessionId", System.Guid.NewGuid().ToString());
            return this.View();
        }

        /// <summary>
        /// Authenticate user and signs the user in when successful.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns> Created response view </returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            this._session.SetString("HasSession", "Exist");

            if(!ModelState.IsValid)
            {
                return View(model);
            }

            /*
            var tempModel = _userService.GetUserByEmail(model.Email);
            User user = new() { UserId = tempModel.UserId, Username = "Name", Password = "Password" };

            await this._signInManager.SignInAsync(user);
            this._session.SetString("UserName", user.UserId);

            return RedirectToAction("Index", "Home");
            */
            
            User user = null;
            
            var loginResult = _userService.AuthenticateUserByEmail(model.Email, model.Password, ref user);
            if (loginResult == LoginResult.Success)
            {
                if (user.AccessStatus != AccessStatus.FirstTime && user.AccessStatus != AccessStatus.Verified)
                {
                    TempData["ErrorMessage"] = "Your account is not allowed to login. Please contact support.";
                    return View();
                }
                await this._signInManager.SignInAsync(user);
                this._session.SetString("UserName", user.Username);
                // Pass AccessStatus to the view for modal logic
                TempData["AccessStatus"] = user.AccessStatus.ToString();
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // 認証NG
                TempData["ErrorMessage"] = "Incorrect UserId or Password";
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new EmailRequestModel());
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RequestOtp(EmailRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Register", model);
            }

            var success = await _userService.SendOtpForRegistrationAsync(model.Email);
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

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp(OtpVerificationModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var isValid = _userService.ValidateOtpForRegistration(model.Email, model.Otp);
            if (isValid)
            {
                // Generate temp password, create user, and send temp password email
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
                // Show the registration success screen
                return View("RegisterSuccess");
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid OTP. Please try again.";
                return View(model);
            }
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
        /// Sign Out current account and return login view.
        /// </summary>
        /// <returns>Created response view</returns>
        [AllowAnonymous]
        public async Task<IActionResult> SignOutUser()
        {
            await this._signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
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
                // Use the registration success screen for "All done"
                return View("RegisterSuccess");
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update password. Please try again.";
                return View(model);
            }
        }
    }
}
