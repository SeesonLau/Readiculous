using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using Readiculous.WebApp.Mvc;
using Readiculous.WebApp.Models;               // ✅ ADD THIS
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Readiculous.Resources.Constants.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Readiculous.WebApp.Controllers
{
    public class UserController : ControllerBase<UserController>
    {
        private readonly IUserService _userService;

        public UserController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IUserService userService)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _userService = userService;
        }

        public IActionResult Index(string searchString, RoleType? roleType, UserSortType searchType = UserSortType.Latest)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentRoleType"] = roleType.HasValue ? roleType.Value : string.Empty;
            ViewData["CurrentUserSearchType"] = searchType.ToString();

            ViewBag.RoleTypes = _userService.GetUserRoles();
            ViewBag.UserSearchTypes = _userService.GetUserSortTypes();

            List<UserListItemViewModel> users = _userService.GetUserList(role: roleType, username: searchString, sortType: searchType);

            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _userService.AddUserAsync(model, this.UserId);
                    return RedirectToAction("Index");
                }
                catch (InvalidDataException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(string userId)
        {
            try
            {
                var user = _userService.SearchUserEditById(userId);
                return View(user);
            }
            catch (InvalidDataException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditAsync(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _userService.UpdateUserAsync(model, this.UserId);
                    return RedirectToAction("Index");
                }
                catch (InvalidDataException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(model);
        }

        public IActionResult Details(string userId)
        {
            try
            {
                var user = _userService.SearchUserDetailsById(userId);
                return View(user);
            }
            catch (InvalidDataException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return RedirectToAction("Index");
            }
        }

        public IActionResult Delete(string userId)
        {
            try
            {
                _userService.DeleteUserAsync(userId, this.UserId);
                return RedirectToAction("Index");
            }
            catch (InvalidDataException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return RedirectToAction("Index");
            }
        }


        [AllowAnonymous]
        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ModalToShow = "forgot";
                return View("GuestView");
            }

            // TODO: Add OTP send logic here

            return RedirectToAction("LandingPage", "Home", new { modal = "otp" });
        }

       


    }
}
