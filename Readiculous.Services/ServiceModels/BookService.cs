using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Readiculous.Resources.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Services
{
    public class BookService : IBookService
    {
        private static readonly List<BookViewModel> _books = new List<BookViewModel>
        {
            new BookViewModel
            {
                Id = "1",
                Title = "Dune",
                Author = "Frank Herbert",
                Rating = 4.7,
                Genre = "Sci-Fi",
                CoverImageUrl = "/img/dune.jpg",
                Description = "A sci-fi classic set on the desert planet Arrakis.",
                CreatedDate = DateTime.UtcNow.AddDays(-5)
            },
            new BookViewModel
            {
                Id = "2",
                Title = "Circe",
                Author = "Madeline Miller",
                Rating = 4.9,
                Genre = "Fantasy",
                CoverImageUrl = "/img/circe.jpg",
                Description = "A mythological retelling of the witch Circe.",
                CreatedDate = DateTime.UtcNow.AddDays(-2)
            }
        };

        private static readonly List<(string userId, string bookId)> _favorites = new();

        public Task AddBook(BookViewModel model, string creatorId)
        {
            model.Id = Guid.NewGuid().ToString();
            model.CreatedDate = DateTime.UtcNow;
            _books.Add(model);
            Console.WriteLine($"[Mock] Added book '{model.Title}' by {creatorId}");
            return Task.CompletedTask;
        }

        public Task UpdateBook(BookViewModel model, string updaterId)
        {
            var index = _books.FindIndex(b => b.Id == model.Id);
            if (index >= 0)
            {
                _books[index] = model;
                Console.WriteLine($"[Mock] Updated book '{model.Title}' by {updaterId}");
            }
            return Task.CompletedTask;
        }

        public void DeleteBook(string bookId, string deleterId)
        {
            var book = _books.FirstOrDefault(b => b.Id == bookId);
            if (book != null)
            {
                _books.Remove(book);
                Console.WriteLine($"[Mock] Deleted book '{book.Title}' by {deleterId}");
            }
        }

        public List<BookListItemViewModel> GetBookList(string searchString, List<GenreViewModel> genres, string userID, BookSearchType searchType = BookSearchType.AllBooks, BookSortType sortType = BookSortType.CreatedTimeDescending)
        {
            var query = _books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
                query = query.Where(b => b.Title.Contains(searchString, StringComparison.OrdinalIgnoreCase));

            switch (sortType)
            {
                case BookSortType.RatingDescending:
                    query = query.OrderByDescending(b => b.Rating);
                    break;
                case BookSortType.CreatedTimeDescending:
                    query = query.OrderByDescending(b => b.CreatedDate);
                    break;
            }

            return query.Select(b => new BookListItemViewModel
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                Rating = b.Rating,
                Genre = b.Genre
            }).ToList();
        }

        public BookDetailsViewModel GetBookDetailsById(string id)
        {
            var book = _books.FirstOrDefault(b => b.Id == id);
            if (book == null) return null;

            return new BookDetailsViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Rating = book.Rating,
                Genre = book.Genre,
                CoverImageUrl = book.CoverImageUrl,
                Description = book.Description
            };
        }

        public BookViewModel GetBookEditById(string id)
        {
            return _books.FirstOrDefault(b => b.Id == id);
        }

        public List<SelectListItem> GetBookSearchTypes(BookSearchType selected)
        {
            return Enum.GetValues(typeof(BookSearchType))
                .Cast<BookSearchType>()
                .Select(e => new SelectListItem
                {
                    Text = e.ToString(),
                    Value = e.ToString(),
                    Selected = e == selected
                }).ToList();
        }

        public List<SelectListItem> GetBookSortTypes(BookSortType selected)
        {
            return Enum.GetValues(typeof(BookSortType))
                .Cast<BookSortType>()
                .Select(e => new SelectListItem
                {
                    Text = e.ToString(),
                    Value = e.ToString(),
                    Selected = e == selected
                }).ToList();
        }

        public List<BookViewModel> GetNewBooks()
        {
            return _books.OrderByDescending(b => b.CreatedDate).ToList();
        }

        public List<BookViewModel> GetTopBooks()
        {
            return _books.OrderByDescending(b => b.Rating).ToList();
        }

        public void AddBookToFavorites(string bookId, string userId)
        {
            if (!_favorites.Any(f => f.userId == userId && f.bookId == bookId))
            {
                _favorites.Add((userId, bookId));
                Console.WriteLine($"[Mock] Added to favorites: BookID={bookId}, UserID={userId}");
            }
        }

        public void RemoveBookFromFavorites(string bookId, string userId)
        {
            var fav = _favorites.FirstOrDefault(f => f.userId == userId && f.bookId == bookId);
            if (fav != default)
            {
                _favorites.Remove(fav);
                Console.WriteLine($"[Mock] Removed from favorites: BookID={bookId}, UserID={userId}");
            }
        }
    }
}
