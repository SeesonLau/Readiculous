using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Readiculous.Data.Models;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using Readiculous.WebApp.Models;
using Readiculous.WebApp.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.WebApp.Controllers
{
    public class GenreMasterController : ControllerBase<GenreMasterController>
    {
        private readonly IGenreService _genreService;
        public GenreMasterController(IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory, IConfiguration configuration, IGenreService genreService, IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _genreService = genreService;
        }

        //GenreListItemViewModel
        public IActionResult GenreMasterScreen(string searchString, GenreSortType searchType = GenreSortType.CreatedTimeAscending, int page = 1, int pageSize = 10)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentGenreSearchType"] = searchType.ToString();

            ViewBag.GenreSearchTypes = _genreService.GetGenreSortTypes();

            var allGenres = _genreService.GetGenreList(searchString, searchType);
            var totalItems = allGenres.Count;
            var paginatedGenres = allGenres
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.PaginationModel = new PaginationModel(totalItems, page, pageSize);
            ViewBag.PageSize = pageSize;

            return View(paginatedGenres);
        }


        // GenreViewModel
        [HttpGet]
        public IActionResult GenreAddModal()
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
                    // AJAX support
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true });
                    }
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = ex.Message });
                    }
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            // AJAX support for validation errors
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return BadRequest(new { success = false, errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).FirstOrDefault()
                ) });
            }
            return View(model);
        }

        // GenreViewModel
        [HttpGet]
        public IActionResult GenreEditModal(string id)
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
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true });
                    }
                    // No redirect
                }
                catch (InvalidOperationException ex)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = ex.Message });
                    }
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return BadRequest(new { success = false, errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).FirstOrDefault()
                ) });
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _genreService.DeleteGenre(id, this.UserId);
                return Json(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //GenreDetailsViewModel
        public IActionResult GenreViewModal(string id, int page = 1, string bookSearch = null)
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
