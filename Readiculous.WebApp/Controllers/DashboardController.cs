using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Readiculous.Data.Models;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using Readiculous.WebApp.Models;
using Readiculous.WebApp.Mvc;
using static Readiculous.Resources.Constants.Enums;
using System.Threading.Tasks;
using Readiculous.WebApp.Authentication;

namespace Readiculous.WebApp.Controllers
{
    public class DashboardController : ControllerBase<DashboardController>
    {
        private readonly IBookService _bookService;
        private readonly IGenreService _genreService;
        private readonly SignInManager _signInManager;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public DashboardController(
            IBookService bookService,
            IGenreService genreService,
            IUserService userService,
            SignInManager signInManager,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration
        ) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _bookService = bookService;
            _genreService = genreService;
            _userService = userService;
            _mapper = mapper;
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


                if (editProfileViewModel.UserId == User.FindFirst("UserId")?.Value)
                {
                    User updatedUser = _userService.GetUserById(editProfileViewModel.UserId);
                    await _signInManager.SignInAsync(updatedUser, isPersistent: true);
                }
                return Json(new { success = true });
            }
            catch
            {
                return PartialView("_EditProfileModal", editProfileViewModel);
            }

        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult LandingScreen()
        {
            ViewBag.ShowProfile = false;
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult DashboardScreen()
        {
            var dashboardViewModel = new DashboardViewModel();
            dashboardViewModel.NewBooks = _bookService.GetBookList(
                searchString: string.Empty,
                genres: new List<GenreViewModel>(),
                userID: null,
                searchType: BookSearchType.NewBooks,
                sortType: BookSortType.Latest
                )
                .Take(5)
                .ToList();
            dashboardViewModel.TopBooks = _bookService.GetBookList(
                searchString: string.Empty,
                genres: new List<GenreViewModel>(),
                userID: null,
                searchType: BookSearchType.NewBooks,
                sortType: BookSortType.Latest
                )
                .Take(5)
                .ToList();

            return View(dashboardViewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ViewNewBooks()
        {
            var now = DateTime.UtcNow;

            var newBooks = _bookService.GetBookList(
                searchString: "",
                genres: new List<GenreViewModel>(),
                userID: null,
                searchType: BookSearchType.AllBooks,
                sortType: BookSortType.Latest
            ).Where(b => b.CreatedTime >= now.AddDays(-14))
             .OrderByDescending(b => b.CreatedTime)
             .ToList();

            return View(newBooks);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ViewTopBooks()
        {
            var topBooks = _bookService.GetBookList(
                searchString: "",
                genres: new List<GenreViewModel>(),
                userID: null,
                searchType: BookSearchType.AllBooks,
                sortType: BookSortType.RatingDescending
            ).OrderByDescending(b => b.AverageRating)
             .ToList();

            return View(topBooks);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GenreScreen()
        {
            var allBooks = _bookService.GetBookList(
                searchString: "",
                genres: new List<GenreViewModel>(),
                userID: null,
                searchType: BookSearchType.AllBooks,
                sortType: BookSortType.Latest
            );

            var allGenres = allBooks
                .SelectMany(b => b.Genres ?? new List<string>())
                .Distinct()
                .OrderBy(g => g)
                .ToList();

            ViewBag.AllGenres = allGenres;

            return View(allBooks);
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult BookDetailScreen(string id)
        {
            var book = _bookService.GetBookDetailsById(id);
            if (book == null)
                return NotFound();

            // We set a clear role string for clarity
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Reviewer"))
                {
                    ViewBag.UserRole = "Reviewer";
                }
                else
                {
                    ViewBag.UserRole = "User";
                }

                var userId = this.UserId;
                ViewBag.UserId = userId;
                ViewBag.UserName = this.UserName;
                ViewBag.UserEmail = _userService.GetEmailByUserId(userId);
            }
            else
            {
                ViewBag.UserRole = "Anonymous";
            }

            return View(book);
        }
        public IActionResult AddToFavoritesBook(string bookId)
        {
            _bookService.AddBookToFavorites(bookId, this.UserId);
            return RedirectToAction("BookDetailsScreen", "Dashboard");
        }


    }
}
