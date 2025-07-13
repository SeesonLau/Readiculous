using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Readiculous.Data.Models;
using Readiculous.Resources.Constants;
using Readiculous.Services.Interfaces;
using Readiculous.Services.Manager;
using Readiculous.Services.ServiceModels;
using Readiculous.Services.Services;
using Readiculous.WebApp.Authentication;
using Readiculous.WebApp.Models;
using Readiculous.WebApp.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserMasterController : ControllerBase<UserController>
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly SignInManager _signInManager;

        public UserMasterController(IHttpContextAccessor httpContextAccessor,
                                  ILoggerFactory loggerFactory,
                                  IConfiguration configuration,
                                  IMapper mapper,
                                  IUserService userService,
                                  IEmailService emailService,
                                  SignInManager signInManager)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _userService = userService;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        public IActionResult UserMasterScreen(string searchString, RoleType? roleType, UserSortType sortOrder = UserSortType.Latest, int page = 1, int pageSize = 10)
        {
            ViewData["CurrentFilter"] = searchString ?? "";
            ViewData["CurrentRoleType"] = roleType?.ToString() ?? "";
            ViewData["CurrentSortOrder"] = sortOrder.ToString();

            ViewBag.RoleTypes = _userService.GetUserRoles();
            ViewBag.UserSortTypes = _userService.GetUserSortTypes(sortOrder);
 
            var allUsers = _userService.GetUserList(
                username: searchString,
                role: roleType,
                sortType: sortOrder);

            var totalItems = allUsers.Count;
            var paginatedUsers = allUsers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.PaginationModel = new PaginationModel(totalItems, page, pageSize);
            ViewBag.PageSize = pageSize;

            var model = new UserMasterViewModel
            {
                Users = paginatedUsers,
                Pagination = (PaginationModel)ViewBag.PaginationModel
            };

            return View(model);
        }


        [HttpGet]
        public IActionResult UserAddModal()
        {
            UserViewModel userViewModel = new();
            return PartialView(userViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            //model.IsAdminCreation = true;
           if (ModelState.IsValid)
            {
                try
                {
                    string tempPassword = OtpManager.GenerateTempPassword();
                    model.Password = PasswordManager.EncryptPassword(tempPassword);
                    await _userService.AddUserAsync(model, this.UserId);
                    await _emailService.SendTempPasswordEmailAsync(model.Email, tempPassword);
                    return Json(new { success = true });
                }
                catch (DuplicateNameException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
                catch (InvalidDataException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = Resources.Messages.Errors.ServerError;
                }
            }
            return PartialView("UserAddModal", model);
            
        }

        [HttpGet]
        public IActionResult UserEditModal(string userId)
        {
            var user = _userService.GetUserEditById(userId);
            return PartialView(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "Password is required");
            }
                
            if (ModelState.IsValid) 
            {
                try
                {
                    await _userService.UpdateUserAsync(model, this.UserId);

                    if (model.UserId == User.FindFirst("UserId")?.Value)
                    {
                        User updatedUser = _userService.GetUserById(model.UserId);
                        await _signInManager.SignInAsync(updatedUser, isPersistent: true);
                    }

                    return Json(new { success = true });
                }
                catch (KeyNotFoundException)
                {
                    TempData["ErrorMessage"] = Resources.Messages.Errors.UserNotFound;
                }
                catch (InvalidDataException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = Resources.Messages.Errors.ServerError;
                }
            }
            return PartialView("UserEditModal", model);
        }

        [HttpGet]
        public IActionResult UserViewModal(string userId)
        {
            var user = _userService.GetUserDetailsById(userId);
            return PartialView(user);
        }

        [HttpPost]
        public IActionResult Delete(string userId)
        {
            _userService.DeleteUser(userId, this.UserId);
            return Json(new { success = true });
        }
    }
}
