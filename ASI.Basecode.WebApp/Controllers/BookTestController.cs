using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using ASI.Basecode.WebApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace ASI.Basecode.WebApp.Controllers
{
    [AllowAnonymous]
    public class BookTestController : Controller
    {
        public IActionResult GenreBooks(
            string genres,
            string filterType = "Top",
            string filterOption = "High Rating",
            int page = 1,
            int pageSize = 10)
        {
            // Parse genres (comma-separated string to list)
            var genreList = string.IsNullOrWhiteSpace(genres)
                ? new List<string>()
                : genres.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(g => g.Trim()).ToList();

            // Fetch all books
            var allBooks = GetAllBooks();

            // Filter books by genre (if any selected)
            List<BookViewModel> filteredBooks = genreList.Any()
                ? allBooks.Where(book => genreList.Contains(book.Genre, StringComparer.OrdinalIgnoreCase)).ToList()
                : allBooks;

            // Sort by rating or year
            if (filterOption == "High Rating")
            {
                filteredBooks = filteredBooks.OrderByDescending(b => b.Rating).ToList();
            }
            else if (filterOption == "Latest Book")
            {
                filteredBooks = filteredBooks.OrderByDescending(b => b.Year).ToList();
            }

            // Additional sort if needed based on filterType (optional)
            if (filterType == "Top")
            {
                filteredBooks = filteredBooks.OrderByDescending(b => b.Rating).ToList();
            }
            else if (filterType == "New")
            {
                filteredBooks = filteredBooks.OrderByDescending(b => b.Year).ToList();
            }

            // Pagination
            int totalBooks = filteredBooks.Count;
            int totalPages = (int)Math.Ceiling((double)totalBooks / pageSize);

            var paginated = filteredBooks
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pass data to view
            ViewBag.SelectedGenres = genreList;
            ViewBag.FilterType = filterType;
            ViewBag.FilterOption = filterOption;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalBooks = totalBooks;
            ViewBag.TotalPages = totalPages;

            return View("GenreBooks", paginated);
        }

        private List<BookViewModel> GetAllBooks()
        {
            return new List<BookViewModel>
            {
                new BookViewModel { Id = 11, Title = "The Book Thief", Author = "Markus Zusak", Genre = "Fiction", Rating = 4.5, Year = 2005, ImagePath = "bookthief.jpg" },
                new BookViewModel { Id = 12, Title = "Room", Author = "Emma Donoghue", Genre = "Fiction", Rating = 4.3, Year = 2010, ImagePath = "room.jpg" },
                new BookViewModel { Id = 13, Title = "Cosmos", Author = "Carl Sagan", Genre = "Sci-Fi", Rating = 4.6, Year = 1980, ImagePath = "cosmos.jpg" },
                new BookViewModel { Id = 14, Title = "Dune", Author = "Frank Herbert", Genre = "Sci-Fi", Rating = 4.7, Year = 1965, ImagePath = "dune.jpg" },
                new BookViewModel { Id = 15, Title = "The Fault in Our Stars", Author = "John Green", Genre = "Romance", Rating = 4.4, Year = 2012, ImagePath = "faultstars.jpg" },
                new BookViewModel { Id = 16, Title = "Gone Girl", Author = "Gillian Flynn", Genre = "Psychological", Rating = 4.2, Year = 2014, ImagePath = "gonegirl.jpg" },
                new BookViewModel { Id = 17, Title = "Normal People", Author = "Sally Rooney", Genre = "Drama", Rating = 4.1, Year = 2018, ImagePath = "normalpeople.jpg" },
                new BookViewModel { Id = 18, Title = "Beloved", Author = "Toni Morrison", Genre = "Drama", Rating = 4.5, Year = 1987, ImagePath = "beloved.jpg" },
                new BookViewModel { Id = 19, Title = "The life of Elizabeth I", Author = "Alison Weir", Genre = "Historical", Rating = 4.5, Year = 1998, ImagePath = "historical1.png" },
                new BookViewModel { Id = 20, Title = "Circe", Author = "Madeline Miller", Genre = "Psychological", Rating = 4.5, Year = 2018, ImagePath = "circe.png" },
                new BookViewModel { Id = 21, Title = "The Night Circus", Author = "Erin Morgenstern", Genre = "Psychological", Rating = 4.3, Year = 2011, ImagePath = "fantasy2.png" },
                new BookViewModel { Id = 22, Title = "In The Books", Author = "Tana French", Genre = "Psychological", Rating = 4.5, Year = 2007, ImagePath = "mystery1.png" },
                new BookViewModel { Id = 23, Title = "Philosophy Deep Dive", Author = "Author I", Genre = "Psychological", Rating = 4.0, Year = 2019, ImagePath = "philosophy.png" },
                new BookViewModel { Id = 24, Title = "Psych Minds", Author = "Author J", Genre = "Psychological", Rating = 4.6, Year = 2021, ImagePath = "psych.png" },
                new BookViewModel { Id = 25, Title = "Fourth Wing", Author = "Author K", Genre = "Psychological", Rating = 4.6, Year = 2021, ImagePath = "psych.png" },
                new BookViewModel { Id = 26, Title = "Clockwork Princess", Author = "Author L", Genre = "Psychological", Rating = 4.6, Year = 2021, ImagePath = "psych.png" },
                new BookViewModel { Id = 27, Title = "James", Author = "Author M", Genre = "Psychological", Rating = 4.6, Year = 2021, ImagePath = "james.png" },
                new BookViewModel { Id = 28, Title = "Hidden Nature", Author = "Author N", Genre = "Psychological", Rating = 4.6, Year = 2021, ImagePath = "hidden.png" },
                new BookViewModel { Id = 29, Title = "My Friends", Author = "Author O", Genre = "Psychological", Rating = 4.6, Year = 2021, ImagePath = "friends.png" }
            };
        }
    }
}
