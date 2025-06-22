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

        public IActionResult Index(string searchString, GenreSortType searchType = GenreSortType.CreatedTimeAscending)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentGenreSearchType"] = searchType.ToString();

            ViewBag.GenreSearchTypes = Enum.GetValues(typeof(GenreSortType))
                .Cast<GenreSortType>()
                .Select(t => new SelectListItem
                {
                    Value = ((int)t).ToString(),
                    Text = t.ToString()
                }).ToList();

            List<GenreViewModel> genres;

            if (!string.IsNullOrEmpty(searchString))
            {
                genres = _genreService.SearchGenresByName(searchString, searchType);
            }
            else
            {
                genres = _genreService.SearchGenresByName(string.Empty, searchType);
            }

            return View(genres);
        }


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

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var genre = _genreService.GetGenreById(id);
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

        public IActionResult Delete(string id)
        {
            try
            {
                _genreService.DeleteGenre(id, this.UserId);
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Index", _genreService.GetActiveGenres());
            }
        }

        public IActionResult Details(string id)
        {
            var genre = _genreService.GetGenreById(id);
            if (genre == null)
            {
                return NotFound();
            }
            return View(genre);
        }
    }
}
