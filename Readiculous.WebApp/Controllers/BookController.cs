using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using Readiculous.WebApp.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.WebApp.Controllers
{
    public class BookController : ControllerBase<BookController>
    {
        private readonly IUserService _userService;
        private readonly IBookService _bookService;
        private readonly IGenreService _genreService;
        private readonly IReviewService _reviewService;
        public BookController(IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory, IConfiguration configuration, IUserService userService, IBookService bookService, IGenreService genreService, IReviewService reviewService, IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _userService = userService;
            _bookService = bookService;
            _genreService = genreService;
            _reviewService = reviewService;
        }

        public IActionResult Index(string searchString, List<GenreViewModel> genres, BookSearchType searchType, BookSortType sortOrder)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSortOrder"] = sortOrder;

            ViewBag.AllGenres = _genreService.GetGenreList(genreName: string.Empty);
            ViewBag.SelectedGenreIds = _genreService.GetSelectedGenreIds(genres);
            ViewBag.BookSortTypes = _bookService.GetBookSortTypes(sortOrder);

            var model = _bookService.GetBookList(searchString: searchString, genres: genres, userID: this.UserId, sortType: sortOrder);

            return View(model);
        }
        [HttpGet]
        public IActionResult Create()
        {
            var model = new BookViewModel();
            var allGenres = _genreService.GetGenreList(genreName: string.Empty);
            model.AllAvailableGenres = _genreService.ConvertGenreListItemViewModelToGenreViewModel(allGenres);

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Create(BookViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _bookService.AddBook(model, this.UserId);
                return RedirectToAction("Index");
            }

            var allGenres = _genreService.GetGenreList(genreName: string.Empty);
            model.AllAvailableGenres = _genreService.ConvertGenreListItemViewModelToGenreViewModel(allGenres);

            return View(model);
        }
        [HttpGet]
        public IActionResult Edit(string id)
        {
            try
            {
                var model = _bookService.GetBookEditById(id);
                var allGenres = _genreService.GetGenreList(genreName: string.Empty);
                model.AllAvailableGenres = _genreService.ConvertGenreListItemViewModelToGenreViewModel(allGenres);
                model.CoverImageUrl = model.CoverImageUrl ?? string.Empty;

                return View(model);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Index", _bookService.GetBookList(searchString: string.Empty, genres: null, userID: this.UserId));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = Resources.Messages.Errors.ServerError;
                return View("Index", _bookService.GetBookList(searchString: string.Empty, genres: null, userID: this.UserId));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(BookViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _bookService.UpdateBook(model, this.UserId);
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = Resources.Messages.Errors.ServerError;
                }
            }

            var allGenres = _genreService.GetGenreList(genreName: string.Empty);
            model.AllAvailableGenres = _genreService.ConvertGenreListItemViewModelToGenreViewModel(allGenres);

            return View(model);
        }
        public IActionResult Details(string id)
        {
            try
            {
                var model = _bookService.GetBookDetailsById(id);
                return View(model);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Index", _bookService.GetBookList(searchString: string.Empty, genres: null, userID: this.UserId));
            }
            catch
            {
                TempData["ErrorMessage"] = Resources.Messages.Errors.ServerError;
                return View("Index", _bookService.GetBookList(searchString: string.Empty, genres: null, userID: this.UserId));
            }
            
        }
        public IActionResult Delete(string id)
        {
            try
            {
                _bookService.DeleteBook(id, this.UserId);
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Index", _bookService.GetBookList(searchString: string.Empty, genres: null, userID: this.UserId));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = Resources.Messages.Errors.ServerError;
                return View("Index", _bookService.GetBookList(searchString: string.Empty, genres: null, userID: this.UserId));
            }
        }

        // Favorite Book Methods
        public IActionResult AddToFavorites(string id)
        {
            try
            {
                _bookService.AddBookToFavorites(id, this.UserId);
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = Resources.Messages.Errors.ServerError;
                return RedirectToAction("Index");
            }
        }
        public IActionResult RemoveFromFavorites(string id)
        {
            try
            {
                _bookService.RemoveBookFromFavorites(id, this.UserId);
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = Resources.Messages.Errors.ServerError;
                return RedirectToAction("Index");
            }
        }

        // Review Methods
        [HttpGet]
        public IActionResult CreateReview(string id)
        {
            try
            {
                var title = _bookService.GetTitleByBookId(id);
                var email = _userService.GetEmailByUserId(this.UserId);


                var model = new ReviewViewModel { BookId = id, UserId = this.UserId, UserName = this.UserName, BookTitle = title, Email = email };
                return PartialView("_AddReviewModal", model);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return StatusCode(400);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = Resources.Messages.Errors.ServerError;
                return StatusCode(500);
            }
        }
        [HttpPost]
        public IActionResult CreateReview(ReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_AddReviewModal", model); // Return modal with validation errors
            }

            try
            {
                _reviewService.AddReview(model);
                return RedirectToAction("Details", new { id = model.BookId }); // redirect if success
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = Resources.Messages.Errors.ServerError;
                return PartialView("_AddReviewModal", model); // Return modal with error
            }
        }

        [HttpGet]
        public IActionResult EditReviewModal(string id)
        {
            try
            {
                var model = _reviewService.GetReviewByBookIdAndUserId(id, this.UserId);
                if (model == null)
                {
                    return NotFound();
                }
                return PartialView("_EditReviewModal", model);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = Resources.Messages.Errors.ServerError;
                return RedirectToAction("Index", _bookService.GetBookList(searchString: string.Empty, genres: null, userID: this.UserId));
            }
        }

        [HttpPost]
        public IActionResult EditReview(ReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_EditReviewModal", model);
            }
            try
            {
                _reviewService.UpdateReview(model, this.UserId);
                return RedirectToAction("Details", new { id = model.BookId });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = Resources.Messages.Errors.ServerError;
            }

            return PartialView("_EditReviewModal", model);
        }
    }
}
