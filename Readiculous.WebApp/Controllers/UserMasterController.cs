using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using Readiculous.WebApp.Models;
using Readiculous.WebApp.Mvc;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.WebApp.Controllers
{
    public class UserMasterController : ControllerBase<UserController>
    {
        private readonly IUserService _userService;

        public UserMasterController(IHttpContextAccessor httpContextAccessor,
                                  ILoggerFactory loggerFactory,
                                  IConfiguration configuration,
                                  IMapper mapper,
                                  IUserService userService)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _userService = userService;
        }

        private bool IsReviewer()
        {
            var user = _userService.GetUserEditById(this.UserId);
            return user.Role == Readiculous.Resources.Constants.Enums.RoleType.Reviewer;
        }

        public IActionResult UserMasterScreen(string searchString, RoleType? roleType, UserSortType searchType, int page = 1, int pageSize = 10)
        {
            if (IsReviewer())
                return View("~/Views/Shared/Forbidden.cshtml");
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentRoleType"] = roleType.HasValue ? roleType.Value : string.Empty;
            ViewData["CurrentUserSearchType"] = searchType.ToString();

            ViewBag.RoleTypes = _userService.GetUserRoles();
            ViewBag.UserSearchTypes = _userService.GetUserSortTypes();

            List<UserListItemViewModel> allUsers = _userService.GetUserList(role: roleType, username: searchString, sortType: searchType);

            //Pagination
            var paginatedUsers = allUsers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var model = new UserMasterViewModel
            {
                Users = paginatedUsers,
                Pagination = new PaginationModel(allUsers.Count, page, pageSize)
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult UserAddModal()
        {
            if (IsReviewer())
                return View("~/Views/Shared/Forbidden.cshtml");
            return PartialView(new UserViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (IsReviewer())
                return View("~/Views/Shared/Forbidden.cshtml");
            if (ModelState.IsValid)
            {
                try
                {
                    await _userService.AddUserAsync(model, this.UserId);
                    return Json(new { success = true });
                }
                catch (InvalidDataException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return PartialView("UserAddModal", model);
        }

        [HttpGet]
        public IActionResult UserEditModal(string userId)
        {
            if (IsReviewer())
                return View("~/Views/Shared/Forbidden.cshtml");
            try
            {
                var user = _userService.GetUserEditById(userId);
                return PartialView(user);
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            if (IsReviewer())
                return View("~/Views/Shared/Forbidden.cshtml");
            if (ModelState.IsValid)
            {
                try
                {
                    await _userService.UpdateUserAsync(model, this.UserId);
                    return Json(new { success = true });
                }
                catch (InvalidDataException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return PartialView("UserEditModal", model);
        }

        [HttpGet]
        public IActionResult UserViewModal(string userId)
        {
            if (IsReviewer())
                return View("~/Views/Shared/Forbidden.cshtml");
            try
            {
                var user = _userService.GetUserDetailsById(userId);
                return PartialView(user);
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string userId)
        {
            if (IsReviewer())
                return View("~/Views/Shared/Forbidden.cshtml");
            try
            {
                await _userService.DeleteUserAsync(userId, this.UserId);
                return Json(new { success = true });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
