using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.WebApp.Controllers
{
    [AllowAnonymous]
    public class UserController : Controller
    {
        // Dummy data with IDs
        private static List<Book> AllNewBooks => new List<Book>
        {
            new Book { Id = 1, Title = "Atmosphere: A Love Story", Author = "Taylor Jenkins Reid", Year = 2025, Rating = 4.5, ImagePath = "atmosphere.png" },
            new Book { Id = 2, Title = "This Dog Will Change Your Life", Author = "Elias Weiss Friedman", Year = 2025, Rating = 4.4, ImagePath = "thisdog.png" },
            new Book { Id = 3, Title = "My Friends", Author = "Fredrik Backman", Year = 2025, Rating = 4.3, ImagePath = "friends.png" },
            new Book { Id = 4, Title = "Never Flinch", Author = "Stephen King", Year = 2025, Rating = 4.2, ImagePath = "flinch.png" },
            new Book { Id = 5, Title = "Hidden Nature", Author = "Nora Roberts", Year = 2025, Rating = 4.1, ImagePath = "hidden.png" },
            new Book { Id = 6, Title = "Book 6", Author = "Author 6", Year = 2025, Rating = 4.0, ImagePath = "book6.png" },
            new Book { Id = 7, Title = "Book 7", Author = "Author 7", Year = 2025, Rating = 4.0, ImagePath = "book7.png" },
            new Book { Id = 8, Title = "Book 8", Author = "Author 8", Year = 2025, Rating = 4.0, ImagePath = "book8.png" },
            new Book { Id = 9, Title = "Book 9", Author = "Author 9", Year = 2025, Rating = 4.0, ImagePath = "book9.png" },
            new Book { Id = 10, Title = "Book 10", Author = "Author 10", Year = 2025, Rating = 4.0, ImagePath = "book10.png" },
            new Book { Id = 11, Title = "Book 11", Author = "Author 11", Year = 2025, Rating = 4.0, ImagePath = "book11.png" },
            new Book { Id = 011, Title =  "The Book Thief", Author = "Markus Zusak", Year = 2005, Rating = 4.5,  ImagePath = "bookthief.jpg" },
            new Book { Id = 012, Title =  "Room", Author = "Emma Donoghue", Year = 2010, Rating = 4.3, ImagePath = "room.jpg" },
            new Book { Id = 013, Title =  "Cosmos", Author = "Carl Sagan", Year = 1980, Rating = 4.6, ImagePath = "cosmos.jpg" },
            new Book { Id = 014, Title =  "Dune", Author = "Frank Herbert", Year = 1965, Rating = 4.7, ImagePath = "dune.jpg" },
            new Book { Id = 015, Title =  "The Fault in Our Stars", Author = "John Green", Year = 2012, Rating = 4.4, ImagePath = "faultstars.jpg" },
            new Book { Id = 016, Title =  "Gone Girl", Author = "Gillian Flynn", Year = 2014, Rating = 4.2, ImagePath = "gonegirl.jpg" },
            new Book { Id = 025, Title = "Fourth Wing", Author = "Author K", Year = 2021, Rating = 4.6, ImagePath = "psych.png" },
            new Book { Id = 026, Title = "Clockwork Princess", Author = "Author L", Year = 2021, Rating = 4.6, ImagePath = "psych.png" },
            new Book { Id = 027, Title = "James", Author = "Author M", Year = 2021, Rating = 4.6, ImagePath = "james.png" },
            new Book { Id = 028, Title = "Hidden Nature", Author = "Author N", Year = 2021, Rating = 4.6, ImagePath = "hidden.png" },
            new Book { Id = 029, Title = "My Friends", Author = "Author O",  Year = 2021, Rating = 4.6, ImagePath = "friends.png" },
            new Book { Id = 017, Title =  "Normal People", Author = "Sally Rooney", Year = 2018, Rating = 4.1, ImagePath = "normalpeople.jpg" }

        };

        private static List<Book> AllTopBooks => new List<Book>
        {
            new Book { Id = 101, Title = "The Silent Patient", Author = "Alex Michaelides", Year = 2019, Rating = 4.5, ImagePath = "silentpatient.png" },
            new Book { Id = 102, Title = "James", Author = "Percival Everett", Year = 2024, Rating = 4.5, ImagePath = "james.png" },
            new Book { Id = 103, Title = "Nightingale", Author = "Kristin Hannah", Year = 2015, Rating = 4.5, ImagePath = "nightingale.png" },
            new Book { Id = 104, Title = "Clockwork Princess", Author = "Cassandra Clare", Year = 2013, Rating = 4.5, ImagePath = "clockprincess.png" },
            new Book { Id = 105, Title = "Fourth Wing", Author = "Rebecca Yarros", Year = 2023, Rating = 4.5, ImagePath = "fourthwing.png" },
            new Book { Id = 106, Title = "Top Book 6", Author = "Author 6", Year = 2025, Rating = 4.2, ImagePath = "topbook6.png" },
            new Book { Id = 107, Title = "Top Book 7", Author = "Author 7", Year = 2025, Rating = 4.1, ImagePath = "topbook7.png" },
            new Book { Id = 108, Title = "Top Book 8", Author = "Author 8", Year = 2025, Rating = 4.0, ImagePath = "topbook8.png" },
            new Book { Id = 109, Title = "Top Book 9", Author = "Author 9", Year = 2025, Rating = 4.0, ImagePath = "topbook9.png" },
            new Book { Id = 018, Title =  "Beloved", Author = "Toni Morrison", Year = 1987, Rating = 4.5, ImagePath = "beloved.jpg" },
            new Book { Id = 019, Title =  "The life of Elizabeth I", Author = "Alison Weir", Year = 1998, Rating = 4.5, ImagePath = "historical1.png" },
            new Book { Id = 020, Title =  "Circe", Author = "Madeline Miller", Year = 2018, Rating = 4.5,  ImagePath = "circe.png" },
            new Book { Id = 021, Title =  "The Night Circus", Author = "Erin Morgenstern", Year = 2011, Rating = 4.3, ImagePath = "fantasy2.png" },
            new Book { Id = 022, Title =  "In The Books", Author = "Tana French", Year = 2007, Rating = 4.5, ImagePath = "mystery1.png" },
            new Book { Id = 023, Title = "Philosophy Deep Dive", Author = "Author I", Year = 2019, Rating = 4.0, ImagePath = "philosophy.png" },
            new Book { Id = 110, Title = "Top Book 10", Author = "Author 10", Year = 2025, Rating = 4.0, ImagePath = "topbook10.png" },
            new Book { Id = 111, Title = "Top Book 11", Author = "Author 11", Year = 2025, Rating = 4.0, ImagePath = "topbook11.png" },
            new Book { Id = 024, Title =   "Psych Minds", Author = "Author J", Year = 2021, Rating = 4.6, ImagePath = "psych.png" }
        };

        public IActionResult UserView()
        {
            ViewBag.NewBooks = AllNewBooks.Take(5).ToList();
            ViewBag.TopBooks = AllTopBooks.Take(5).ToList();
            return View();
        }

        public IActionResult ViewNewBooks(int page = 1)
        {
            int pageSize = 10;
            int totalBooks = AllNewBooks.Count;
            int totalPages = (int)System.Math.Ceiling((double)totalBooks / pageSize);

            var books = AllNewBooks
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View("ViewNewBooks", books);
        }

        public IActionResult ViewTopBooks(int page = 1)
        {
            int pageSize = 10;
            int totalBooks = AllTopBooks.Count;
            int totalPages = (int)System.Math.Ceiling((double)totalBooks / pageSize);

            var books = AllTopBooks
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View("ViewTopBooks", books);
        }

  

        public IActionResult BookDetails(int id)
        {
            var book = AllNewBooks
                .Concat(AllTopBooks)
                .FirstOrDefault(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            // Genre, Series, Rating, AddedDate will be left blank in the view
            return View(book);
        }
    }
}


