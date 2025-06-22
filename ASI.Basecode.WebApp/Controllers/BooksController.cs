using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using ASI.Basecode.WebApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace ASI.Basecode.WebApp.Controllers
{
    [AllowAnonymous]
    public class BooksController : Controller
    {
        public IActionResult GenreBooks(
            string genres,
            string filterType = "Top",
            string filterOption = "High Rating",
            int page = 1,
            int pageSize = 10)
        {
            // Parse genres
            var genreList = string.IsNullOrWhiteSpace(genres)
                ? new List<string>()
                : genres.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(g => g.Trim()).ToList();

            ViewBag.SelectedGenres = genreList;
            ViewBag.FilterType = filterType;
            ViewBag.FilterOption = filterOption;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            List<BookViewModel> filteredBooks;

            if (genreList.Any())
            {
                // Filter books by selected genres
                filteredBooks = GetAllBooks()
                    .Where(book => genreList.Contains(book.Genre, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                // Apply sorting
                filteredBooks = filterOption switch
                {
                    "High Rating" => filteredBooks.OrderByDescending(b => b.Rating).ToList(),
                    "Latest Book" => filteredBooks.OrderByDescending(b => b.Year).ToList(),
                    _ => filteredBooks
                };

                // Total and page calculations
                int totalBooks = filteredBooks.Count;
                int totalPages = (int)Math.Ceiling((double)totalBooks / pageSize);

                ViewBag.TotalBooks = totalBooks;
                ViewBag.TotalPages = totalPages;

                // Paginate
                var paginated = filteredBooks
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return View("GenreBooks", paginated);
            }
            else
            {
                // No genres selected = return empty model
                return View("GenreBooks", new List<BookViewModel>());
            }
        }

        private List<BookViewModel> GetAllBooks()
        {
            return new List<BookViewModel>
            {
                new BookViewModel { Id = 011, Title = "The Book Thief", Author = "Markus Zusak", Genre = "Fiction", Rating = 4.5, Year = 2005, ImagePath = "bookthief.jpg" },
                new BookViewModel { Id = 012, Title = "Room", Author = "Emma Donoghue", Genre = "Fiction", Rating = 4.3, Year = 2010, ImagePath = "room.jpg" },
                new BookViewModel { Id = 013, Title = "Cosmos", Author = "Carl Sagan", Genre = "Sci-Fi", Rating = 4.6, Year = 1980, ImagePath = "cosmos.jpg" },
                new BookViewModel { Id = 014, Title = "Dune", Author = "Frank Herbert", Genre = "Sci-Fi", Rating = 4.7, Year = 1965, ImagePath = "dune.jpg" },
                new BookViewModel { Id = 015, Title = "The Fault in Our Stars", Author = "John Green", Genre = "Romance", Rating = 4.4, Year = 2012, ImagePath = "faultstars.jpg" },
                new BookViewModel { Id = 016, Title = "Gone Girl", Author = "Gillian Flynn", Genre = "Psychological", Rating = 4.2, Year = 2014, ImagePath = "gonegirl.jpg" },
                new BookViewModel { Id = 017, Title = "Normal People", Author = "Sally Rooney", Genre = "Drama", Rating = 4.1, Year = 2018, ImagePath = "normalpeople.jpg" },
                new BookViewModel { Id = 018, Title = "Beloved", Author = "Toni Morrison", Genre = "Drama", Rating = 4.5, Year = 1987, ImagePath = "beloved.jpg" },
                new BookViewModel { Id = 019, Title = "The life of Elizabeth I", Author = "Alison Weir", Genre = "Historical", Rating = 4.5, Year = 1998, ImagePath = "historical1.png" },
                new BookViewModel { Id = 020, Title = "Circe", Author = "Madeline Miller", Genre = "Psychological", Rating = 4.5, Year = 2018, ImagePath = "circe.png" },
                new BookViewModel { Id = 021, Title = "The Night Circus", Author = "Erin Morgenstern", Genre = "Psychological", Rating = 4.3, Year = 2011, ImagePath = "fantasy2.png" },
                new BookViewModel { Id = 022, Title = "In The Books", Author = "Tana French", Genre = "Psychological", Rating = 4.5, Year = 2007, ImagePath = "mystery1.png" },
                new BookViewModel { Id = 023, Title = "Philosophy Deep Dive", Author = "Author I", Genre = "Psychological", Rating = 4.0, Year = 2019, ImagePath = "philosophy.png" },
                new BookViewModel { Id = 024, Title = "Psych Minds", Author = "Author J", Genre = "Psychological", Rating = 4.6, Year = 2021, ImagePath = "psych.png" },
                new BookViewModel { Id = 025, Title = "Fourth Wing", Author = "Author K", Genre = "Psychological", Rating = 4.6, Year = 2021, ImagePath = "psych.png" },
                new BookViewModel { Id = 026, Title = "Clockwork Princess", Author = "Author L", Genre = "Psychological", Rating = 4.6, Year = 2021, ImagePath = "psych.png" },
                new BookViewModel { Id = 027, Title = "James", Author = "Author M", Genre = "Psychological", Rating = 4.6, Year = 2021, ImagePath = "james.png" },
                new BookViewModel { Id = 028, Title = "Hidden Nature", Author = "Author N", Genre = "Psychological", Rating = 4.6, Year = 2021, ImagePath = "hidden.png" },
                new BookViewModel { Id = 029, Title = "My Friends", Author = "Author O", Genre = "Psychological", Rating = 4.6, Year = 2021, ImagePath = "friends.png" }
            };
        }
    }
}
