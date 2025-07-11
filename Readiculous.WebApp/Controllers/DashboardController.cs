using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Readiculous.Resources.Constants;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using Readiculous.Services.Services;
using Readiculous.WebApp.Models;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.WebApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IGenreService _genreService;
        private readonly IMapper _mapper;

        public DashboardController(IBookService bookService, IGenreService genreService, IMapper mapper)
        {
            _bookService = bookService;
            _genreService = genreService;
            _mapper = mapper;
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult DashboardScreen()
        {
            var allBooks = _bookService.GetBookList(
                searchString: string.Empty,
                genres: new List<GenreViewModel>(),
                userID: null,
                searchType: BookSearchType.AllBooks,
                sortType: BookSortType.Latest
            );

            var now = DateTime.UtcNow;

            var allNewBooks = allBooks
                .Where(b => b.CreatedTime != default && (now - b.CreatedTime).TotalDays <= 14)
                .OrderByDescending(b => b.CreatedTime)
                .ToList();

            var newBooks = allNewBooks
                .Take(5)
                .ToList();

            var topBooks = allBooks
                .Where(b => b.AverageRating > 0)
                .OrderByDescending(b => b.AverageRating)
                .ThenByDescending(b => b.CreatedTime)
                .Take(5)
                .ToList();

            ViewBag.HasEnoughNewBooks = allNewBooks.Count >= 5;

            var model = new DashboardViewModel
            {
                NewBooks = newBooks,
                TopBooks = topBooks
            };

            return View(model);
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ViewNewBooks()  
        {
            var allBooks = _bookService.GetBookList(
                searchString: "",
                genres: new List<GenreViewModel>(),
                userID: null,
                searchType: BookSearchType.AllBooks,
                sortType: BookSortType.Latest
            );

            var now = DateTime.UtcNow;

            var newBooks = allBooks
                .Where(b => b.CreatedTime >= now.AddDays(-14))
                .OrderByDescending(b => b.CreatedTime)
                .ToList();

            return View(newBooks);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ViewTopBooks()
        {
            var allBooks = _bookService.GetBookList(
                searchString: "",
                genres: new List<GenreViewModel>(),
                userID: null,
                searchType: Enums.BookSearchType.AllBooks,
                sortType: Enums.BookSortType.RatingDescending
            );

            var topBooks = allBooks
                .OrderByDescending(b => b.AverageRating)
                .ToList();

            return View(topBooks);
        }




        [HttpGet]
        [AllowAnonymous]
        public IActionResult GenreScreen()
        {
            var allBooks = _bookService.GetBookList(
                searchString: "",
                genres: new List<GenreViewModel>(),
                userID: null,
                searchType: BookSearchType.AllBooks,
                sortType: BookSortType.Latest
            );

            var allGenres = allBooks
                .SelectMany(b => b.Genres ?? new List<string>())
                .Distinct()
                .OrderBy(g => g)
                .ToList();

            ViewBag.AllGenres = allGenres;

            return View(allBooks);
        }




        [HttpGet]
        [AllowAnonymous]
        public IActionResult BookDetailScreen(int id)
        {
            return View();
        }



    }
}
