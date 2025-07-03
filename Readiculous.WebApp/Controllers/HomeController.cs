using Readiculous.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using System.Threading.Tasks;
using System;
using System.IO;

namespace Readiculous.WebApp.Controllers
{
    /// <summary>
    /// Home Controller
    /// </summary>
    public class HomeController : ControllerBase<HomeController>
    {
        private readonly IUserService _userService;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="configuration"></param>
        /// <param name="localizer"></param>
        /// <param name="mapper"></param>
        public HomeController(IHttpContextAccessor httpContextAccessor,
                              ILoggerFactory loggerFactory,
                              IConfiguration configuration, IUserService userService,
                              IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _userService = userService;
        }

        /// <summary>
        /// Returns Home View.
        /// </summary>
        /// <returns> Home View </returns>
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            var model = _userService.GetEditProfileById(this.UserId);
            if (model == null)
            {
                return NotFound();
            }
            return PartialView("_EditProfileModal", model);
        }
        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileViewModel editProfileViewModel)
        {
            // Filling up passwords are optional when changing profile
            if (_userService.IsChangingPassword(editProfileViewModel))
            {
                var isCurrentPasswordCorrect = _userService.IsCurrentPasswordCorrect(this.UserId, editProfileViewModel.CurrentPassword);
                // Current password must match with user's current password
                if (!isCurrentPasswordCorrect)
                {
                    ModelState.AddModelError(nameof(editProfileViewModel.CurrentPassword), "Current password is incorrect.");
                }

                // If the user wants to change their password, all 3 fields must be filled out
                if (string.IsNullOrWhiteSpace(editProfileViewModel.CurrentPassword))
                    ModelState.AddModelError(nameof(editProfileViewModel.CurrentPassword), "Current password is required.");
                if (string.IsNullOrWhiteSpace(editProfileViewModel.NewPassword))
                    ModelState.AddModelError(nameof(editProfileViewModel.NewPassword), "New password is required.");
                if (string.IsNullOrWhiteSpace(editProfileViewModel.ConfirmPassword))
                    ModelState.AddModelError(nameof(editProfileViewModel.ConfirmPassword), "Confirm password is required.");
            }

            if (!ModelState.IsValid)
            {
                return PartialView("_EditProfileModal", editProfileViewModel);
            }

            try
            {
                await _userService.UpdateProfileAsync(editProfileViewModel, this.UserId);
                return Json(new { success = true });
            }
            catch
            {
                return PartialView("_EditProfileModal", editProfileViewModel);
            }
        }
    }
}
