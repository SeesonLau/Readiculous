using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Authorization;

namespace ASI.Basecode.WebApp.Controllers
{
    [AllowAnonymous]
    public class UserController : Controller
    {
        public IActionResult UserView()
        {
            var newBooks = new List<Book>
            {
                new Book { Title = "Atmosphere: A Love Story", Author = "Taylor Jenkins Reid", Year = 2025, Rating = 4.5, ImagePath = "atmosphere.png" },
                new Book { Title = "This Dog Will Change Your Life", Author = "Elias Weiss Friedman", Year = 2025, Rating = 4.4, ImagePath = "thisdog.png" },
                new Book { Title = "My Friends", Author = "Fredrik Backman", Year = 2025, Rating = 4.3, ImagePath = "friends.png" },
                new Book { Title = "Never Flinch", Author = "Stephen King", Year = 2025, Rating = 4.2, ImagePath = "flinch.png" },
                new Book { Title = "Hidden Nature", Author = "Nora Roberts", Year = 2025, Rating = 4.1, ImagePath = "hidden.png" },
            };

            var topBooks = new List<Book>
            {
                new Book { Title = "The Silent Patient", Author = "Alex Michaelides", Year = 2019, Rating = 4.5, ImagePath = "silentpatient.png" },
                new Book { Title = "James", Author = "Percival Everett", Year = 2024, Rating = 4.5, ImagePath = "james.png" },
                new Book { Title = "Nightingale", Author = "Kristin Hannah", Year = 2015, Rating = 4.5, ImagePath = "nightingale.png" },
                new Book { Title = "Clockwork Princess", Author = "Cassandra Clare", Year = 2013, Rating = 4.5, ImagePath = "clockprincess.png" },
                new Book { Title = "Fourth Wing", Author = "Rebecca Yarros", Year = 2023, Rating = 4.5, ImagePath = "fourthwing.png" },
            };

            ViewBag.NewBooks = newBooks;
            ViewBag.TopBooks = topBooks;

            return View();
        }

        public IActionResult GenreBooks(string filter, string genre)
        {
            // Dummy data
            var allBooks = new List<Book>
         {
                 new Book { Title = "Fiction Book 1", Author = "Author A", Year = 2025, Rating = 4.5, ImagePath = "fiction1.png" },
        new Book { Title = "Fiction Book 2", Author = "Author B", Year = 2025, Rating = 4.5, ImagePath = "fiction2.png" },
        new Book { Title = "Drama Book", Author = "Author C", Year = 2025, Rating = 4.4, ImagePath = "drama1.png" },
        new Book { Title = "Drama 2", Author = "Author D", Year = 2025, Rating = 4.3, ImagePath = "drama2.png" },
        new Book { Title = "Historical Book", Author = "Author E", Year = 2025, Rating = 4.2, ImagePath = "historical1.png" },
        new Book { Title = "Fantasy Book", Author = "Author F", Year = 2025, Rating = 4.5, ImagePath = "fantasy1.png" },
        new Book { Title = "Fantasy 2", Author = "Author G", Year = 2025, Rating = 4.3, ImagePath = "fantasy2.png" },
        new Book { Title = "Mystery Book", Author = "Author H", Year = 2025, Rating = 4.5, ImagePath = "mystery1.png" }
    };

            // 🛠️ Fix null genre
            genre = string.IsNullOrEmpty(genre) ? "Fiction" : genre;

            var filteredBooks = allBooks
                .Where(b => b.Title.ToLower().Contains(genre.ToLower()))
                .Take(10)
                .ToList();

            ViewBag.Filter = filter;
            ViewBag.Genre = genre;
            ViewBag.Books = filteredBooks;

            return View();
        }
    }
}
