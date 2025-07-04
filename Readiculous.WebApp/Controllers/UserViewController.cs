using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;

namespace ASI.Readiculous.WebApp.Controllers
{
    [AllowAnonymous]
    public class UserViewController : Controller
    {
        private readonly IBookService _bookService;

        // ✅ Inject IBookService
        public UserViewController(IBookService bookService)
        {
            _bookService = bookService;
        }

        /// <summary>
        /// Landing page for guest users.
        /// </summary>
        [HttpGet]
        public IActionResult GuestView()
        {
            // If you want to show any featured books, fetch them here.
            var books = new List<BookViewModel>();
            return View("GuestView", books);
        }

        /// <summary>
        /// Displays books of selected genres, filtered by type and option.
        /// Example URL:
        /// /UserView/GenreBooks?genres=Romance,Thriller&filterType=Top&filterOption=High%20Rating
        /// </summary>
        [HttpGet]
        public IActionResult GenreBooks(
            string genres,
            string filterType = "Top",
            string filterOption = "High Rating")
        {
            var selectedGenres = genres?.Split(',').ToList() ?? new List<string>();

            ViewBag.SelectedGenres = selectedGenres;
            ViewBag.FilterType = filterType;
            ViewBag.FilterOption = filterOption;

            var books = new List<BookViewModel>();
            return View("GenreBooks", books);
        }

        /// <summary>
        /// Displays books from selected genres, but specifically the NEW ones.
        /// This could be routed separately if needed.
        /// </summary>
        [HttpGet]
        public IActionResult NewGenreBooks(
            string genres,
            string filterType = "New",
            string filterOption = "High Rating")
        {
            var selectedGenres = genres?.Split(',').ToList() ?? new List<string>();

            ViewBag.SelectedGenres = selectedGenres;
            ViewBag.FilterType = filterType;
            ViewBag.FilterOption = filterOption;

            var books = new List<BookViewModel>();
            return View("GenreBooks", books);
        }

        [HttpGet]
        public IActionResult ViewNewBooks(int page = 1, int pageSize = 1)
        {
            var allBooks = new List<BookViewModel>();

            // Only show Dune on first page
            if (page == 1)
            {
                allBooks.Add(new BookViewModel
                {
                    Title = "Dune",
                    Author = "Frank Herbert",
                    Rating = 4.7,
                    CoverImageUrl = "/img/dune.jpg"
                });
            }

            int totalPages = 1;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View("ViewNewBooks", allBooks);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ViewTopBooks(int page = 1)
        {
            var topBooks = new List<BookViewModel>
        {
            new BookViewModel
            {
                Title = "Circe",
                Author = "Madeline Miller",
                CoverImageUrl = "/img/circe.jpg",
                Rating = 4.9
            },
        };

            return View(topBooks);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult UserBookDetails(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                title = "Dune";  // Default
            }

            // Example static data - replace with your actual DB or service call
            if (title == "Circe")
            {
                ViewData["Title"] = "Circe";
                ViewData["Author"] = "Madeline Miller";
                ViewData["Genre"] = "Fantasy";
                ViewData["Series"] = "None";
                ViewData["Rating"] = 4.9;
                ViewData["AddedDate"] = "June 15, 2025";
                ViewData["Description"] = "A bold and subversive retelling of the goddess Circe's story...";
                ViewData["ImagePath"] = "~/img/circe.jpg";
                ViewData["Reviews"] = new List<dynamic>
        {
            new { Reviewer = "John", Date = "July 2, 2025", Stars = 5, Comment = "Loved the mythology and character depth!" },
            new { Reviewer = "Sophie", Date = "July 1, 2025", Stars = 4, Comment = "Beautifully written, but slow in parts." }
        };
            }
            else // Default to Dune
            {
                ViewData["Title"] = "Dune";
                ViewData["Author"] = "Frank Herbert";
                ViewData["Genre"] = "Science Fiction";
                ViewData["Series"] = "Dune Series";
                ViewData["Rating"] = 4.7;
                ViewData["AddedDate"] = "July 1, 2025";
                ViewData["Description"] = "Set on the desert planet Arrakis, Dune is the story of the boy Paul Atreides...";
                ViewData["ImagePath"] = "~/img/dune.jpg";
                ViewData["Reviews"] = new List<dynamic>
        {
            new { Reviewer = "Alice", Date = "June 30, 2025", Stars = 5, Comment = "Amazing book with rich world building!" },
            new { Reviewer = "Bob", Date = "June 28, 2025", Stars = 4, Comment = "Classic sci-fi that everyone should read." }
        };
            }


            return View();
        }



    }
}
