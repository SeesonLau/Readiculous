using System.Collections.Generic;
using ASI.Basecode.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace ASI.Basecode.WebApp.Controllers
{
    [AllowAnonymous]
    public class AdminController : Controller
    {
        private static List<BookViewModel> books = new List<BookViewModel>
        {
            new BookViewModel { Id = 1, Title = "The Silent Patient", Author = "Alex Michaelides", Genre = "Fiction", Series = "N/A", Rating = 5 },
            new BookViewModel { Id = 2, Title = "James", Author = "Percival Everett", Genre = "Novel", Series = "N/A", Rating = 4 },
            new BookViewModel { Id = 3, Title = "Nightingale", Author = "Kristin Hannah", Genre = "Romance", Series = "N/A", Rating = 4 },
            new BookViewModel { Id = 4, Title = "Clockwork Princess", Author = "Cassandra Clare", Genre = "Fantasy", Series = "3", Rating = 5 },
            new BookViewModel { Id = 5, Title = "Fourth Wing", Author = "Rebecca Yarros", Genre = "Romance", Series = "3", Rating = 4 },
            new BookViewModel { Id = 6, Title = "Crooked Kingdom", Author = "Leigh Bardugo", Genre = "Fantasy", Series = "2", Rating = 3 },
        };

        public ActionResult BookMaster()
        {
            return View(books);
        }

        [HttpGet]
        public ActionResult AddBook()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddBook(BookViewModel model, IFormFile CoverImage)
        {
            if (ModelState.IsValid)
            {
                model.Id = books.Count + 1;
                model.Rating = 0;
                books.Add(model);
                return RedirectToAction("BookMaster");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteBook(int id)
        {
            var bookToRemove = books.Find(b => b.Id == id);
            if (bookToRemove != null)
            {
                books.Remove(bookToRemove);
            }
            return RedirectToAction("BookMaster");
        }
    }
}
