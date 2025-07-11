using AspNetCoreGeneratedDocument;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using Readiculous.WebApp.Models;
using Readiculous.WebApp.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.WebApp.Controllers
{
    public class UserController : ControllerBase<UserController>
    {
        private readonly IUserService _userService;
        public UserController(IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory, IConfiguration configuration, IMapper mapper, IUserService userService) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _userService = userService;
        }

        public IActionResult Index(string searchString, RoleType? roleType, UserSortType searchType = UserSortType.Latest)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentRoleType"] = roleType.HasValue ? roleType.Value : string.Empty;
            ViewData["CurrentUserSearchType"] = searchType.ToString();

            ViewBag.RoleTypes = _userService.GetUserRoles();
            ViewBag.UserSearchTypes = _userService.GetUserSortTypes(searchType);

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
        public IActionResult EditUserModal(string userId)
        {
            try
            {
                var user = _userService.GetUserEditById(userId);
                return PartialView("_EditUserModal", user);
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsync(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_EditUserModal", model);
            }

            await _userService.UpdateUserAsync(model, this.UserId);
            return Json(new { success = true });
        }

        public IActionResult Details(string userId)
        {
            try
            {
                var user = _userService.GetUserEditById(userId);
                var registrationComplete = new RegisterSuccessfulViewModel
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    Password = user.Password
                };
                return PartialView("_RegistrationSuccessfulModal", registrationComplete);
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
    }
}
