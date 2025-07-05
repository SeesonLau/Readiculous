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

namespace Readiculous.WebApp.Controllers
{
    [AllowAnonymous]
    public class LandingPageController : ControllerBase<LandingPageController>
    {

        private readonly SessionManager _sessionManager;
        private readonly SignInManager _signInManager;
        private readonly TokenValidationParametersFactory _tokenValidationParametersFactory;
        private readonly TokenProviderOptionsFactory _tokenProviderOptionsFactory;
        private readonly IConfiguration _appConfiguration;
        private readonly IUserService _userService;
        public LandingPageController(
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
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            TempData["returnUrl"] = System.Net.WebUtility.UrlDecode(HttpContext.Request.Query["ReturnUrl"]);
            this._sessionManager.Clear();
            this._session.SetString("SessionId", System.Guid.NewGuid().ToString());
            return PartialView("_LoginModal", new LoginViewModel { Email = string.Empty, Password = string.Empty});

        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            this._session.SetString("HasSession", "Exist");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User user = null;

            var loginResult = _userService.AuthenticateUserByEmail(model.Email, model.Password, ref user);
            if (loginResult == LoginResult.Success)
            {
                // 認証OK
                await this._signInManager.SignInAsync(user);
                this._session.SetString("UserName", user.Username);

                //if(user.Role == RoleType.Reviewer)
                return RedirectToAction("Index", "Home");
                //else if(user.Role == RoleType.Admin)
                //return Admin Page
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
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAsync(UserViewModel model)
        {
            try
            {
                model.UserId = Guid.NewGuid().ToString();
                model.Role = RoleType.Reviewer;
                await _userService.AddUserAsync(model, model.UserId);
                return RedirectToAction("Login", "Account");
            }
            catch (InvalidDataException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = Resources.Messages.Errors.ServerError;
            }
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> SignOutUser()
        {
            await this._signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
        public IActionResult LandingPage()
        {

            return View();
        }

    }
}
