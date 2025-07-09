using Readiculous.Services.Interfaces;
using Readiculous.Services.Manager;
using Readiculous.WebApp.Authentication;
using Readiculous.WebApp.Mvc;
using Readiculous.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Threading.Tasks;
using static Readiculous.Resources.Constants.Enums;
using Readiculous.Data.Models;

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
            TokenProviderOptionsFactory tokenProviderOptionsFactory
        ) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _sessionManager = new SessionManager(this._session);
            _signInManager = signInManager;
            _tokenProviderOptionsFactory = tokenProviderOptionsFactory;
            _tokenValidationParametersFactory = tokenValidationParametersFactory;
            _appConfiguration = configuration;
            _userService = userService;
        }

        //LANDING PAGE
        [HttpGet]
        public IActionResult LandingPage()
        {
            return View("~/Views/LandingPage/LandingPage.cshtml");
        }

        // LOGIN via AJAX
        [HttpPost]
        [AllowAnonymous]
        public JsonResult LoginAjax([FromBody] LoginViewModel model)
        {
            User user = null;
            var result = _userService.AuthenticateUserByEmail(model.Email, model.Password, ref user);

            if (result == LoginResult.Success)
            {
                HttpContext.Session.SetString("UserId", user.UserId);
                HttpContext.Session.SetString("UserName", user.Username);
                HttpContext.Session.SetString("UserRole", user.Role.ToString());
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Invalid email or password." });
        }

        [AllowAnonymous]
        public IActionResult SignOutUser()
        {
            // Remove user info from session
            HttpContext.Session.Remove("UserId");
            HttpContext.Session.Remove("UserRole");

            // Optionally clear all session data
            HttpContext.Session.Clear();

            // Redirect to GuestView
            return RedirectToAction("GuestView", "UserView");
        }


    }
}
