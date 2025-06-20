using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using ASI.Basecode.WebApp.Models;

namespace ASI.Basecode.WebApp.Controllers
{
    public class BooksController : Controller
    {
        public IActionResult GenreBooks(string genre = "Fiction", string filterType = "Top", string filterOption = "High Rating")
        {
            // Set ViewBag values for the view
            ViewBag.SelectedGenre = genre;
            ViewBag.FilterType = filterType;
            ViewBag.FilterOption = filterOption;

            // Get books by genre
            var books = GetBooksByGenre(genre);

            // Apply sorting logic based on filter option
            if (filterOption == "High Rating")
            {
                books = books.OrderByDescending(b => b.Rating).ToList();
            }
            else if (filterOption == "Latest Book")
            {
                books = books.OrderByDescending(b => b.Year).ToList();
            }

            return View(books);
        }

        // Mock data - simulate database fetch
        private List<BookViewModel> GetBooksByGenre(string genre)
        {
            var allBooks = new List<BookViewModel>
            {
                new BookViewModel { Title = "The Book Thief", Author = "Markus Zusak", Genre = "Fiction", Rating = 4.5, Year = 2005, ImagePath = "bookthief.jpg" },
                new BookViewModel { Title = "Room", Author = "Emma Donoghue", Genre = "Fiction", Rating = 4.3, Year = 2010, ImagePath = "room.jpg" },
                new BookViewModel { Title = "Cosmos", Author = "Carl Sagan", Genre = "Sci-Fi", Rating = 4.6, Year = 1980, ImagePath = "cosmos.jpg" },
                new BookViewModel { Title = "Dune", Author = "Frank Herbert", Genre = "Sci-Fi", Rating = 4.7, Year = 1965, ImagePath = "dune.jpg" },
                new BookViewModel { Title = "The Fault in Our Stars", Author = "John Green", Genre = "Romance", Rating = 4.4, Year = 2012, ImagePath = "faultstars.jpg" },
                new BookViewModel { Title = "Gone Girl", Author = "Gillian Flynn", Genre = "Mystery", Rating = 4.2, Year = 2014, ImagePath = "gonegirl.jpg" },
                new BookViewModel { Title = "Normal People", Author = "Sally Rooney", Genre = "Drama", Rating = 4.1, Year = 2018, ImagePath = "normalpeople.jpg" },
                new BookViewModel { Title = "Beloved", Author = "Toni Morrison", Genre = "Drama", Rating = 4.5, Year = 1987, ImagePath = "beloved.jpg" }
            };

            return allBooks.FindAll(b => b.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase));
        }
    }
}
