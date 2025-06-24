using System.Collections.Generic;
using System.IO;
using System.Linq;
using ASI.Basecode.WebApp.Models;
using ASI.Basecode.WebApp.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASI.Basecode.WebApp.Controllers
{
    [AllowAnonymous]
    public class BookMasterController : Controller
    {
        // Display the book list
        public IActionResult ListBooks()
        {
            var books = BookStorage.LoadBooks();
            return View(books);
        }

        // GET: Show the Add Book form
        [HttpGet]
        public IActionResult AddBook()
        {
            return View();
        }
        // POST: Handle Add Book form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddBook(BookViewModel model, IFormFile CoverImage)
        {
            if (ModelState.IsValid)
            {
                var books = BookStorage.LoadBooks();
                model.Id = books.Any() ? books.Max(b => b.Id) + 1 : 1;
                model.Rating = 0; // Default rating

                if (CoverImage != null && CoverImage.Length > 0)
                {
                    var fileName = Path.GetFileName(CoverImage.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/Book", fileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        CoverImage.CopyTo(stream);
                    }
                    model.ImagePath = fileName;
                }

                books.Add(model);
                BookStorage.SaveBooks(books);
                return RedirectToAction("ListBooks");
            }

            return View(model);
        }

        // GET: Show Edit Book form
        [HttpGet]
        public ActionResult EditBook(int id)
        {
            var books = BookStorage.LoadBooks();
            var book = books.FirstOrDefault(b => b.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Handle Edit Book form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditBook(BookViewModel model, IFormFile CoverImage)
        {
            if (ModelState.IsValid)
            {
                var books = BookStorage.LoadBooks();
                var existingBook = books.FirstOrDefault(b => b.Id == model.Id);
                if (existingBook != null)
                {
                    existingBook.Title = model.Title;
                    existingBook.Author = model.Author;
                    existingBook.Genre = model.Genre;
                    existingBook.Series = model.Series;
                    existingBook.Description = model.Description;
                    existingBook.Rating = model.Rating;
                    existingBook.AddedDate = model.AddedDate;
                    existingBook.ISBN = model.ISBN;

                    if (CoverImage != null && CoverImage.Length > 0)
                    {
                        var fileName = Path.GetFileName(CoverImage.FileName);
                        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/Book", fileName);
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            CoverImage.CopyTo(stream);
                        }
                        existingBook.ImagePath = fileName;
                    }

                    BookStorage.SaveBooks(books);
                }

                return RedirectToAction("ListBooks");
            }

            return View(model);
        }

        // POST: Delete a book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteBook(int id)
        {
            var books = BookStorage.LoadBooks();
            var book = books.FirstOrDefault(b => b.Id == id);
            if (book != null)
            {
                books.Remove(book);
                BookStorage.SaveBooks(books);
            }

            return RedirectToAction("ListBooks");
        }

        // GET: View Book Details (no review form)
        [HttpGet]
        public IActionResult ViewBook(int id)
        {
            var books = BookStorage.LoadBooks();
            var book = books.FirstOrDefault(b => b.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            var allReviews = ReviewStorage.LoadReviews();
            var bookReviews = allReviews
                .Where(r => r.BookId == id)
                .OrderByDescending(r => r.DatePosted)
                .ToList();

            ViewBag.Reviews = bookReviews;

            return View(book);
        }
    }
}