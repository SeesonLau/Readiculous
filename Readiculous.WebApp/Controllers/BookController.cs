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
        public BookController(IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory, IConfiguration configuration, IBookService bookService, IGenreService genreService, IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _bookService = bookService;
            _genreService = genreService;
        }

        public IActionResult Index(string searchString, List<GenreViewModel> genres, BookSearchType searchType, BookSortType sortOrder)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSortOrder"] = sortOrder;

            ViewBag.AllGenres = _genreService.GetGenreList(genreName: string.Empty);
            ViewBag.SelectedGenreIds = _genreService.GetSelectedGenreIds(genres);
            ViewBag.BookSearchTypes = Enum.GetValues(typeof(BookSearchType))
                .Cast<BookSearchType>()
                .Select(t => new SelectListItem
                {
                    Value = ((int)t).ToString(),
                    Text = t.ToString(),
                    Selected = t == searchType
                }).ToList();
            ViewBag.BookSortTypes = Enum.GetValues(typeof(BookSortType))
                .Cast<BookSortType>()
                .Select(v => new SelectListItem
                {
                    Text = v.ToString(), 
                    Value = ((int)v).ToString(),
                    Selected = v == sortOrder
                }).ToList();

            if(string.IsNullOrEmpty(searchString) && (genres == null || !genres.Any()) && searchType == BookSearchType.AllBooks && sortOrder == BookSortType.CreatedTimeDescending)
            {
                return View(_bookService.ListAllActiveBooks());
            }
            else if(string.IsNullOrEmpty(searchString))
            {
                return View(_bookService.ListBooksByGenreList(genres, searchType, sortOrder));
            }
            else if (genres == null || !genres.Any())
            {
                return View(_bookService.ListBooksByTitle(searchString, searchType, sortOrder));
            }
            else
            {
                return View(_bookService.ListBooksByTitleAndGenres(searchString, genres, searchType, sortOrder));
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new BookViewModel();

            var allGenres = _genreService.GetGenreList(genreName: string.Empty); 

            model.AllAvailableGenres = allGenres.Select(g => new GenreViewModel
            {
                GenreId = g.GenreId,
                Name = g.Name
            }).ToList();

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
            model.AllAvailableGenres = allGenres.Select(g => new GenreViewModel
            {
                GenreId = g.GenreId,
                Name = g.Name
            }).ToList();
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            try
            {
                var model = _bookService.GetBookEditById(id);
                model.AllAvailableGenres = _genreService.GetGenreList(genreName: string.Empty)
                    .Select(g => new GenreViewModel
                    {
                        GenreId = g.GenreId,
                            Name = g.Name
                        })
                    .ToList();

                model.CoverImageUrl = model.CoverImageUrl ?? string.Empty;

                return View(model);
            }
            catch (Exception ex) {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Index", _bookService.ListAllActiveBooks());
            }
        }
        [HttpPost]
        public async Task<IActionResult> Edit(BookViewModel model)
        {
            if(ModelState.IsValid)
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
            model.AllAvailableGenres = allGenres.Select(g => new GenreViewModel
            {
                GenreId = g.GenreId,
                Name = g.Name
            }).ToList();
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
                return View("Index", _bookService.ListAllActiveBooks());
            }
        }
    }
}
