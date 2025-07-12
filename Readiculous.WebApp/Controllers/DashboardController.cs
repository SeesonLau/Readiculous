using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Readiculous.Resources.Constants;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using Readiculous.WebApp.Models;
using Readiculous.WebApp.Mvc;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.WebApp.Controllers
{
    public class DashboardController : ControllerBase<DashboardController>
    {
        private readonly IBookService _bookService;
        private readonly IGenreService _genreService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public DashboardController(
            IBookService bookService,
            IGenreService genreService,
            IUserService userService,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration
        ) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _bookService = bookService;
            _genreService = genreService;
            _userService = userService;
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

            var newBooks = allNewBooks.Take(5).ToList();

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
            var now = DateTime.UtcNow;

            var newBooks = _bookService.GetBookList(
                searchString: "",
                genres: new List<GenreViewModel>(),
                userID: null,
                searchType: BookSearchType.AllBooks,
                sortType: BookSortType.Latest
            ).Where(b => b.CreatedTime >= now.AddDays(-14))
             .OrderByDescending(b => b.CreatedTime)
             .ToList();

            return View(newBooks);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ViewTopBooks()
        {
            var topBooks = _bookService.GetBookList(
                searchString: "",
                genres: new List<GenreViewModel>(),
                userID: null,
                searchType: BookSearchType.AllBooks,
                sortType: BookSortType.RatingDescending
            ).OrderByDescending(b => b.AverageRating)
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
        public IActionResult BookDetailScreen(string id)
        {
            var book = _bookService.GetBookDetailsById(id);
            if (book == null) return NotFound();

            ViewBag.IsReviewer = User.Identity.IsAuthenticated && User.IsInRole("Reviewer");

            if (User.Identity.IsAuthenticated)
            {
                var userId = this.UserId;
                ViewBag.UserId = userId;
                ViewBag.UserName = this.UserName;
                ViewBag.UserEmail = _userService.GetEmailByUserId(userId);
            }

            return View(book);
        }


    }
}
