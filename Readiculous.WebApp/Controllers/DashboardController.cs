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
        private readonly IUserService _userService;
        private readonly IDashboardService _dashboardService;
        private readonly SignInManager _signInManager;

        public DashboardController(
            IBookService bookService,
            IGenreService genreService,
            IUserService userService,
            IDashboardService dashboardService,
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
        public IActionResult ViewNewBooks(string keyword = "", int page = 1)
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
                filteredBooks = allBooks
                    .Where(b => !string.IsNullOrWhiteSpace(b.Title)
                        && b.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            else
            {
                filteredBooks = allBooks
                    .Where(b => b.CreatedTime >= DateTime.UtcNow.AddDays(-14))
                    .ToList();
            }

            int pageSize = 10;
            int totalBooks = filteredBooks.Count;
            int totalPages = (int)Math.Ceiling((double)totalBooks / pageSize);

            var pagedBooks = filteredBooks
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Keyword = keyword;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View("ViewNewBooks", pagedBooks);
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

        public IActionResult GenreScreen(List<string> selectedGenres)
        {
            selectedGenres ??= new List<string>();

            // Lookup all genres from your genre service
            var allGenreItems = _genreService.GetAllGenreSelectListItems(null);

            // Convert names to IDs
            var selectedGenreIds = allGenreItems
                .Where(g => selectedGenres.Contains(g.Text))
                .Select(g => g.Value)
                .ToList();

            // Create GenreViewModels with both ID and Name
            var genreViewModels = selectedGenreIds
                .Select(id => new GenreViewModel { GenreId = id })
                .ToList();

            var books = _bookService.GetBookList(
                searchString: null,
                genres: genreViewModels,
                userID: null
            );

            // Get all genre names
            var allGenres = allGenreItems
                .Select(g => g.Text)
                .Distinct()
                .OrderBy(g => g)
                .ToList();

            ViewBag.AllGenres = allGenres;
            ViewBag.SelectedGenres = selectedGenres;

            return View(books);
        }


        //[HttpPost]
        //public IActionResult LoadBooksByGenres([FromBody] List<string> genres)
        //{
        //    genres ??= new List<string>();

        //    // Lookup all genres
        //    var allGenreItems = _genreService.GetAllGenreSelectListItems(null);

        //    var selectedGenreIds = allGenreItems
        //        .Where(g => genres.Contains(g.Text))
        //        .Select(g => g.Value)
        //        .ToList();

        //    var genreViewModels = selectedGenreIds
        //        .Select(id => new GenreViewModel { GenreId = id })
        //        .ToList();

        //    var books = _bookService.GetBookList(
        //        searchString: null,
        //        genres: genreViewModels,
        //        userID: null
        //    );

        //    return PartialView("_BookGridPartial", books);
        //}



        [HttpGet("Dashboard/BookDetailScreen/{id}")]
        [AllowAnonymous]
        public IActionResult BookDetailScreen(string id)
        {
            // Validate ID
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Invalid book ID.");

            // Get book details
            var book = _bookService.GetBookDetailsById(id);
            if (book == null)
                return NotFound("Book not found.");

            // Check if user is authenticated and is a Reviewer
            bool isReviewer = User.Identity?.IsAuthenticated == true && User.IsInRole("Reviewer");
            ViewBag.IsReviewer = isReviewer;

            if (isReviewer)
            {
                ViewBag.UserId = this.UserId;
                ViewBag.UserName = this.UserName;
                ViewBag.UserEmail = _userService.GetEmailByUserId(this.UserId);
            }

            return View(book);
        }

        [HttpPost]
        [Authorize(Roles = "Reviewer")]
        public IActionResult AddToFavoritesBook(string bookId)
        {
            _bookService.AddBookToFavorites(bookId, this.UserId);
            return RedirectToAction("BookDetailScreen", new { id = bookId });
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
