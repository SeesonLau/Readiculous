using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Readiculous.Data.Models;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using Readiculous.WebApp.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.WebApp.Controllers
{
    public class GenreController : ControllerBase<GenreController>
    {
        private readonly IGenreService _genreService;
        public GenreController(IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory, IConfiguration configuration, IGenreService genreService, IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _genreService = genreService;
        }

        //GenreListItemViewModel
        public IActionResult Index(string searchString, GenreSortType searchType = GenreSortType.CreatedTimeAscending)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentGenreSearchType"] = searchType.ToString();

            ViewBag.GenreSearchTypes = _genreService.GetGenreSortTypes();

            List<GenreListItemViewModel> genres = _genreService.GetGenreList(searchString, searchType);
            return View(genres);
        }


        // GenreViewModel
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(GenreViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _genreService.AddGenre(model, this.UserId);
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(model);
        }

        // GenreViewModel
        [HttpGet]
        public IActionResult Edit(string id)
        {
            var genre = _genreService.GetGenreEditById(id);
            if (genre == null)
            {
                return NotFound();
            }
            return View(genre);
        }
        [HttpPost]
        public IActionResult Edit(GenreViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _genreService.UpdateGenre(model, this.UserId);
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Delete(string id)
        {
            var genre = _genreService.GetGenreEditById(id);
            if (genre == null)
            {
                return NotFound();
            }
            // Map to the Genre model for the Delete view
            var genreModel = new Readiculous.Data.Models.Genre
            {
                GenreId = genre.GenreId,
                Name = genre.Name
            };
            return View(genreModel);
        }

        [HttpPost]
        public IActionResult Delete(Readiculous.Data.Models.Genre model)
        {
            try
            {
                _genreService.DeleteGenre(model.GenreId, this.UserId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        //GenreDetailsViewModel
        public IActionResult Details(string id, int page = 1, string bookSearch = null)
        {
            var genre = _genreService.GetGenreEditById(id);
            if (genre == null)
            {
                return NotFound();
            }
            var allBooks = _genreService.GetBooksByGenreId(id);
            if (!string.IsNullOrWhiteSpace(bookSearch))
            {
                allBooks = allBooks.Where(b => b.Title != null && b.Title.ToLower().Contains(bookSearch.ToLower())).ToList();
            }
            ViewData["BookSearch"] = bookSearch;
            int pageSize = 10;
            int totalBooks = allBooks.Count;
            int totalPages = (int)Math.Ceiling(totalBooks / (double)pageSize);
            page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));
            var books = allBooks.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var viewModel = new GenreBooksViewModel
            {
                Genre = genre,
                Books = books,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };
            return View(viewModel);
        }
    }
}
