using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
using Readiculous.Resources.Constants;

namespace Readiculous.WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BookMasterController : ControllerBase<BookController>
    {
        private readonly IBookService _bookService;
        private readonly IGenreService _genreService;
        private readonly IReviewService _reviewService;
        private readonly IUserService _userService;
        public BookMasterController(IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory, IConfiguration configuration, IBookService bookService, IGenreService genreService, IReviewService reviewService, IUserService userService, IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _bookService = bookService;
            _genreService = genreService;
            _reviewService = reviewService;
            _userService = userService;
        }

        public IActionResult BookMasterScreen(string searchString, List<GenreViewModel> genres, BookSearchType searchType, BookSortType sortOrder = BookSortType.Latest, string? genreFilter = null, int page = 1, int pageSize = 10)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSortOrder"] = sortOrder;

            ViewBag.GenreList = _genreService.GetAllGenreSelectListItems(genreFilter);
            ViewBag.SelectedGenreIds = _genreService.GetSelectedGenreIds(genres);
            ViewBag.BookSearchTypes = _bookService.GetBookSearchTypes(searchType);
            ViewBag.BookSortTypes = _bookService.GetBookSortTypes(sortOrder);

            var allBooks = _bookService.GetBookList(
                searchString: searchString,
                genres: genres,
                userID: this.UserId,
                searchType: searchType,
                sortType: sortOrder,
                genreFilter: genreFilter);

            var totalItems = allBooks.Count;
            var paginatedBooks = allBooks
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.PaginationModel = new PaginationModel(totalItems, page, pageSize);
            ViewBag.PageSize = pageSize;

            return View(paginatedBooks);
        }
        [HttpGet]
        public IActionResult BookAddModal()
        {
            var model = new BookViewModel();
            var allGenres = _genreService.GetGenreList(genreName: string.Empty);
            model.AllAvailableGenres = _genreService.ConvertGenreListItemViewModelToGenreViewModel(allGenres);
            return PartialView(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // ✅ Handle Cover Image Upload
                    if (model.CoverImage != null && model.CoverImage.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.CoverImage.FileName);
                        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/uploads");

                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        var fullPath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await model.CoverImage.CopyToAsync(stream);
                        }

                        model.CoverImageUrl = $"/img/uploads/{fileName}"; // Save path to model
                    }
                    else
                    {
                        model.CoverImageUrl = "/img/placeholder.png"; // fallback default
                    }

                    // ✅ Save book through service
                    await _bookService.AddBook(model, this.UserId);
                    return Json(new { success = true });
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, Resources.Messages.Errors.ServerError);
                }
            }

            // If validation fails or error occurs, re-populate genres
            var allGenres = _genreService.GetGenreList(genreName: string.Empty);
            model.AllAvailableGenres = _genreService.ConvertGenreListItemViewModelToGenreViewModel(allGenres);
            return PartialView("BookAddModal", model);
        }


        [HttpGet]
        public IActionResult BookEditModal(string id)
        {
            var model = _bookService.GetBookEditById(id);
            var allGenres = _genreService.GetGenreList(genreName: string.Empty);
            model.AllAvailableGenres = _genreService.ConvertGenreListItemViewModelToGenreViewModel(allGenres);
            model.CoverImageUrl = model.CoverImageUrl ?? string.Empty;
            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _bookService.UpdateBook(model, this.UserId);
                    return Json(new { success = true });
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            var allGenres = _genreService.GetGenreList(genreName: string.Empty);
            model.AllAvailableGenres = _genreService.ConvertGenreListItemViewModelToGenreViewModel(allGenres);
            return PartialView("BookEditModal", model);
        }

        [HttpGet]
        public IActionResult BookViewModal(string id)
        {
            var model = _bookService.GetBookDetailsById(id);
            return PartialView(model);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await _bookService.DeleteBook(id, this.UserId);
            return Json(new { success = true });
        }

        // Favorite Book Methods
        public IActionResult AddToFavorites(string id)
        {
            _bookService.AddBookToFavorites(id, this.UserId);
            return RedirectToAction("BookMasterScreen", "BookMaster");
        }
        public IActionResult RemoveFromFavorites(string id)
        {
            _bookService.RemoveBookFromFavorites(id, this.UserId);
            return RedirectToAction("BookMasterScreen", "BookMaster");
        }

        // Review Methods
        [HttpGet]
        public IActionResult CreateReview(string id)
        {
            var model = _reviewService.GenerateInitialReviewViewModel(id, this.UserId, this.UserName);
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateReview(ReviewViewModel model)
        {
            if (!@ModelState.IsValid)
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

        [HttpGet]
        public IActionResult EditReview(string id)
        {
            var model = _reviewService.GetReviewByBookIdAndUserId(id, this.UserId);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditReview(ReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                _reviewService.UpdateReview(model, this.UserId);
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
