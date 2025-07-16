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
using Readiculous.Data.Interfaces;

namespace Readiculous.WebApp.Controllers
{
    public class DashboardController : ControllerBase<DashboardController>
    {
        private readonly IBookService _bookService;
        private readonly IGenreService _genreService;
        private readonly IUserService _userService;
        private readonly IDashboardService _dashboardService;
        private readonly IReviewService _reviewService;
        private readonly IFavoriteBookRepository _favoriteBookRepository;
        private readonly SignInManager _signInManager;

        public DashboardController(
            IBookService bookService,
            IGenreService genreService,
            IUserService userService,
            IDashboardService dashboardService,
            IFavoriteBookRepository favoriteBookRepository,
            IReviewService reviewService,
            IMapper mapper,
            SignInManager signInManager,
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration
        ) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _bookService = bookService;
            _genreService = genreService;
            _userService = userService;
            _dashboardService = dashboardService;
            _favoriteBookRepository = favoriteBookRepository;
            _reviewService = reviewService;
            _signInManager = signInManager;
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
            return PartialView("~/Views/Shared/_EditProfileModal.cshtml", model);
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
            catch (Exception ex)
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
            var dashboardViewModel = _dashboardService.GetUserDashboardViewModel(this.UserId);
            
            return View(dashboardViewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ViewNewBooks(string keyword = "")
        {
            var allBooks = _bookService.GetBookList(
                "", // leave search string blank — we’ll filter it ourselves
                new List<GenreViewModel>(),
                "",
                BookSortType.Latest
            );

            List<BookListItemViewModel> filteredBooks;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                // ONLY search by keyword — DO NOT filter by CreatedTime
                filteredBooks = allBooks
                    .Where(b => !string.IsNullOrWhiteSpace(b.Title)
                        && b.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            else
            {
                // If no keyword, show books from the last 2 weeks only
                filteredBooks = allBooks
                    .Where(b => b.CreatedTime >= DateTime.UtcNow.AddDays(-14))
                    .ToList();
            }

            ViewBag.Keyword = keyword;
            return View("ViewNewBooks", filteredBooks);
        }




        [HttpGet]
        [AllowAnonymous]
        public IActionResult ViewTopBooks()
        {
            var topBooks = _bookService.GetBookList(
                searchString: "",
                genres: new List<GenreViewModel>(),
                userID: null,
                sortType: BookSortType.RatingDescending
            ).OrderByDescending(b => b.AverageRating)
             .ToList();

            return View(topBooks);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GenreScreen(string selectedGenres, GenreSortType sortType = GenreSortType.Latest)
        {
            var selected = string.IsNullOrEmpty(selectedGenres)
                ? new List<string>()
                : selectedGenres.Split(',').ToList();

            var allBooks = _bookService.GetBookList("", new List<GenreViewModel>(), null, BookSortType.Latest);

            var filteredBooks = selected.Any()
                ? allBooks.Where(book => book.Genres != null && book.Genres.Any(g => selected.Contains(g))).ToList()
                : allBooks;

            var allGenres = allBooks
                .SelectMany(b => b.Genres ?? new List<string>())
                .Distinct()
                .OrderBy(g => g)
                .ToList();

            // Prepare SelectListItem for sort dropdown
            var sortOptions = Enum.GetValues(typeof(GenreSortType))
                .Cast<GenreSortType>()
                .Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = e.ToString(),
                    Text = e.ToString(),
                    Selected = (e == sortType)
                })
                .ToList();

            ViewBag.AllGenres = allGenres;
            ViewBag.SelectedGenres = selected;
            ViewBag.SortOptions = sortOptions;

            return View(filteredBooks);
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult GenreBooksPartial(string selectedGenres, GenreSortType sortType = GenreSortType.Latest)
        {
            var selected = string.IsNullOrEmpty(selectedGenres)
                ? new List<string>()
                : selectedGenres.Split(',').ToList();

            var allBooks = _bookService.GetBookList("", new List<GenreViewModel>(), null, BookSortType.Latest);

            var filteredBooks = selected.Any()
                ? allBooks.Where(book => book.Genres != null && book.Genres.Any(g => selected.Contains(g))).ToList()
                : allBooks;

            return PartialView("_BookGridPartial", filteredBooks);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult BookDetailScreen(string id)
        {
            var book = _bookService.GetBookDetailsById(id);
            if (book == null)
                return NotFound();

            var favBooks = _favoriteBookRepository.GetFavoriteBookByBookIdAndUserId(book.BookId, this.UserId);

            if (User.Identity.IsAuthenticated && User.IsInRole("Reviewer"))
            {
                ViewBag.IsReviewer = true;
                ViewBag.UserId = this.UserId;
                ViewBag.UserName = this.UserName;
                ViewBag.UserEmail = _userService.GetEmailByUserId(this.UserId);
                ViewBag.IsReviewed = book.Reviews.Any(r => r.Reviewer == this.UserName);
                ViewBag.IsFavorited = favBooks != null;
            }
            else
            {
                ViewBag.IsReviewer = false;
                ViewBag.IsReviewed = false;
                ViewBag.IsFavorited = false;
            }

            return View(book);
        }

        [HttpPost]
        [Authorize(Roles = "Reviewer")]
        [ValidateAntiForgeryToken]
        public IActionResult AddToFavoritesBook([FromForm] string bookId)
        {
            if (string.IsNullOrEmpty(bookId))
                return BadRequest(new { success = false, message = "Book ID is missing." });

            _bookService.AddBookToFavorites(bookId, this.UserId);
            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize(Roles = "Reviewer")]
        public IActionResult RemoveFromFavoritesBook([FromForm] string bookId)
        {
            if (string.IsNullOrEmpty(bookId))
                return BadRequest(new { success = false, message = "Book ID is missing." });

            _bookService.RemoveBookFromFavorites(bookId, this.UserId);
            return Json(new { success = true });
        }

        [HttpGet]
        [Authorize(Roles = "Reviewer")]
        public IActionResult CreateReviewModal(string id)
        {
            try
            {
                var title = _bookService.GetTitleByBookId(id);
                var email = _userService.GetEmailByUserId(this.UserId);

                var model = new ReviewViewModel
                {
                    BookId = id,
                    UserId = this.UserId,
                    UserName = this.UserName,
                    BookTitle = title,
                    Email = email
                };

                return PartialView("_AddReviewModal", model);
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Reviewer")]
        [ValidateAntiForgeryToken]
        public IActionResult CreateReview(ReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_AddReviewModal", model);
            }

            try
            {
                _reviewService.AddReview(model);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return PartialView("_AddReviewModal", model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Reviewer")]
        public IActionResult EditReviewModal(string id)
        {
            try
            {
                var model = _reviewService.GetReviewByBookIdAndUserId(id, this.UserId);
                if (model == null)
                    return NotFound();

                return PartialView("_EditReviewModal", model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Reviewer")]
        [ValidateAntiForgeryToken]
        public IActionResult EditReview(ReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_EditReviewModal", model);
            }

            try
            {
                _reviewService.UpdateReview(model, this.UserId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return PartialView("_EditReviewModal", model);
            }
        }


        [HttpGet]
        public IActionResult SearchBooks(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new List<object>());
            }

            var books = _bookService.GetBookList(
                query,                        
                new List<GenreViewModel>(),    
                "",                            
                BookSortType.Latest,           
                null                          
            );

            var filtered = books
                .Take(10)
                .Select(b => new
                {
                    id = b.BookId,
                    title = b.Title,
                    cover = string.IsNullOrEmpty(b.CoverImageUrl) ? "/img/placeholder.png" : b.CoverImageUrl
                });

            return Json(filtered);
        }
    }
}
