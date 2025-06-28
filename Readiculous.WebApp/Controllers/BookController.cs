using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private readonly IBookService _bookService;
        private readonly IGenreService _genreService;
        private readonly IReviewService _reviewService;
        public BookController(IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory, IConfiguration configuration, IBookService bookService, IGenreService genreService, IReviewService reviewService, IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
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
            ViewBag.BookSearchTypes = _bookService.GetBookSearchTypes(searchType);
            ViewBag.BookSortTypes = _bookService.GetBookSortTypes(sortOrder);

            var model = _bookService.GetBookList(searchString: searchString, genres: genres, userID: this.UserId, searchType: searchType, sortType: sortOrder); 

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
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Index", _bookService.GetBookList(searchString: string.Empty, genres: null, userID: this.UserId));
            }
        }
        [HttpPost]
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
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            var allGenres = _genreService.GetGenreList(genreName: string.Empty);
            model.AllAvailableGenres = _genreService.ConvertGenreListItemViewModelToGenreViewModel(allGenres);

            return View(model);
        }
        public IActionResult Details(string id)
        {
            var model = _bookService.GetBookDetailsById(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
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
                ModelState.AddModelError(string.Empty, ex.Message);
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
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
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
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return RedirectToAction("Index");
            }
        }

        // Review Methods
        [HttpGet]
        public IActionResult CreateReview(string id)
        {
            var model = new ReviewViewModel { BookId = id, UserId = this.UserId };
            return View(model);
        }
        [HttpPost]
        public IActionResult CreateReview(ReviewViewModel model)
        {
            if(!@ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                _reviewService.AddReview(model);
                return RedirectToAction("Details", new { id = model.BookId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }
    }
}
