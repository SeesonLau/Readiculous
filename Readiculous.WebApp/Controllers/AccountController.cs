using Readiculous.Data.Models;
using Readiculous.Services.Interfaces;
using Readiculous.Services.Manager;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
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
using System.Linq;
using Readiculous.Data;
using System.Collections.Generic;

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
        private readonly ReadiculousDbContext _context;

        public AccountController(
            SignInManager signInManager,
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            ReadiculousDbContext context,
            IUserService userService,
            TokenValidationParametersFactory tokenValidationParametersFactory,
            TokenProviderOptionsFactory tokenProviderOptionsFactory
        ) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._sessionManager = new SessionManager(this._session);
            this._signInManager = signInManager;
            this._tokenProviderOptionsFactory = tokenProviderOptionsFactory;
            this._tokenValidationParametersFactory = tokenValidationParametersFactory;
            this._appConfiguration = configuration;
            this._userService = userService;
            this._context = context;
        }

  
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
         
            return View(new RegisterViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RegisterAjax(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();

                return Json(new
                {
                    success = false,
                    message = errors.Any() ? string.Join(" ", errors) : "Please complete all fields correctly."
                });
            }

            if (_context.Users.Any(u => u.Email == model.Email))
            {
                return Json(new { success = false, message = "Email is already registered." });
            }

            var encryptedPassword = PasswordManager.EncryptPassword(model.Password);

            var newUser = new User
            {
                UserId = Guid.NewGuid().ToString(),
                Username = model.Email.Split('@')[0],
                Email = model.Email,
                Password = encryptedPassword,
                Role = RoleType.Reviewer,
                CreatedTime = DateTime.Now
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> LoginAjax(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();

                return Json(new
                {
                    success = false,
                    message = errors.Any() ? string.Join(" ", errors) : "Please complete all fields correctly."
                });
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

            if (user == null)
            {
                return Json(new { success = false, message = "Account not found. Please check your email or sign up." });
            }

            string decryptedPassword;
            try
            {
                decryptedPassword = PasswordManager.DecryptPassword(user.Password);
            }
            catch
            {
                return Json(new { success = false, message = "Could not verify your password. Please contact support." });
            }

            if (decryptedPassword != model.Password)
            {
                return Json(new { success = false, message = "Invalid password." });
            }

            HttpContext.Session.SetString("UserId", user.UserId);
            HttpContext.Session.SetString("UserRole", user.Role.ToString());

           
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Email),
        new Claim(ClaimTypes.Role, user.Role.ToString()) 
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return Json(new
            {
                success = true,
                role = user.Role.ToString()
            });
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult SignOutUser()
        {
            HttpContext.Session.Clear();
            return Ok();
        }


    }
}
