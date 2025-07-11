using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Readiculous.Data.Models;
using Readiculous.Resources.Constants;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using Readiculous.Services.Services;
using Readiculous.WebApp.Models;
using Readiculous.WebApp.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GenreMasterController : ControllerBase<GenreMasterController>
    {
        private readonly IGenreService _genreService;
        private readonly IUserService _userService;
        public GenreMasterController(IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory, IConfiguration configuration, IGenreService genreService, IUserService userService, IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _genreService = genreService;
            _userService = userService;
        }

        //GenreListItemViewModel
        public IActionResult GenreMasterScreen(string searchString, GenreSortType sortType = GenreSortType.Latest, int page = 1, int pageSize = 10)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentGenreSearchType"] = sortType;

            ViewBag.GenreSortTypes = _genreService.GetGenreSortTypes(sortType);

            var allGenres = _genreService.GetGenreList(
                searchString,
                sortType : sortType);

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
            return PartialView(new GenreViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                catch (DuplicateNameException ex)
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
            try
            {
                var genre = _genreService.GetGenreEditById(id);
                return PartialView(genre);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        public IActionResult GenreViewPage(string id, int page = 1, string bookSearch = null)
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

            var paginationModel = new PaginationModel(totalBooks, page, pageSize);
            var books = allBooks
                .Skip((paginationModel.CurrentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new GenreBooksViewModel
            {
                Genre = genre,
                Books = books,
                CurrentPage = paginationModel.CurrentPage,
                TotalPages = paginationModel.TotalPages,
                PageSize = pageSize,
                TotalBooksCount = totalBooks,
                AllGenres = _genreService.GetGenreList("", GenreSortType.Latest)
            };

            return View(viewModel);
        }
    }
}
