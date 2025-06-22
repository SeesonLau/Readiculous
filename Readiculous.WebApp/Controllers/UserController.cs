using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using Readiculous.WebApp.Mvc;
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

namespace Readiculous.WebApp.Controllers
{
    public class UserController : ControllerBase<UserController>
    {
        private readonly IUserService _userService;
        public UserController(IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory, IConfiguration configuration, IMapper mapper, IUserService userService) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _userService = userService;
        }

        public IActionResult Index(string searchString, RoleType? roleType, UserSortType searchType = UserSortType.CreatedTimeAscending)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentRoleType"] = roleType.HasValue ? roleType.Value : string.Empty;
            ViewData["CurrentUserSearchType"] = searchType.ToString();

            ViewBag.RoleTypes = Enum.GetValues(typeof(RoleType))
                .Cast<RoleType>()
                .Select(r => new SelectListItem
                {
                    Value = ((int)r).ToString(), 
                    Text = r.ToString()
                }).ToList();

            ViewBag.UserSearchTypes = Enum.GetValues(typeof(UserSortType))
                .Cast<UserSortType>()
                .Select(r => new SelectListItem
                {
                    Value = ((int)r).ToString(),
                    Text = r.ToString()
                }).ToList();

            List<UserViewModel> users;

            if (roleType.HasValue && !string.IsNullOrEmpty(searchString))
            {
                users = _userService.SearchUsersByRole(roleType.Value, searchString, searchType);
            }
            else if (roleType.HasValue)
            {
                users = _userService.SearchUsersByRole(roleType.Value, string.Empty, searchType);
            }
            else if (!string.IsNullOrEmpty(searchString))
            {
                users = _userService.SearchUsersByUsername(searchString, searchType);
            }
            else
            {
                users = _userService.SearchAllActiveUsers();
            }

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
                var user = _userService.SearchUserById(userId);
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
                var user = _userService.SearchUserById(userId);
                return View(user);
            }
            catch (InvalidDataException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Delete(string userId)
        {
            try
            {
                await _userService.DeleteUserAsync(userId, this.UserId);
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
