using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Readiculous.Data.Interfaces;
using Readiculous.Data.Models;
using Readiculous.Resources.Constants;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using Supabase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Services.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IGenreRepository _genreRepository;
        private readonly IFavoriteBookRepository _favoriteBookRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IMapper _mapper;
        private readonly Client _client;

        public BookService(
            IBookRepository bookRepository,
            IGenreRepository genreRepository,
            IFavoriteBookRepository favoriteBookRepository,
            IReviewRepository reviewRepository,
            IMapper mapper,
            Client client)
        {
            _bookRepository = bookRepository;
            _genreRepository = genreRepository;
            _favoriteBookRepository = favoriteBookRepository;
            _reviewRepository = reviewRepository;
            _mapper = mapper;
            _client = client;
        }

        public List<BookViewModel> GetNewBooks()
        {
            var recentBooks = _bookRepository.GetAllActiveBooks()
                .OrderByDescending(b => b.CreatedTime)
                .Take(6)
                .ToList();

            return recentBooks.Select(book => _mapper.Map<BookViewModel>(book)).ToList();
        }

        public List<BookViewModel> GetTopBooks()
        {
            var books = _bookRepository.GetAllActiveBooks()
                .Where(b => b.BookReviews.Any())
                .OrderByDescending(b => b.BookReviews.Average(r => r.Rating))
                .Take(6)
                .ToList();

            return books.Select(book => _mapper.Map<BookViewModel>(book)).ToList();
        }

        public IEnumerable<Book> GetLatestBooks()
        {
            return _bookRepository.GetAllActiveBooks()
                .OrderByDescending(b => b.CreatedTime)
                .Take(10)
                .ToList();
        }

        public IEnumerable<Book> GetTopRatedBooks()
        {
            return _bookRepository.GetAllActiveBooks()
                .Where(b => b.BookReviews.Any())
                .OrderByDescending(b => b.BookReviews.Average(r => r.Rating))
                .Take(10)
                .ToList();
        }

        public void AddBookToFavorites(string bookId, string userId)
        {
            if (!_bookRepository.BookIdExists(bookId))
                throw new InvalidOperationException("Book does not exist.");

            if (_favoriteBookRepository.FavoriteBookExists(bookId, userId))
                throw new InvalidOperationException("Book is already in favorites.");

            var favoriteBook = new FavoriteBook
            {
                BookId = bookId,
                UserId = userId,
                CreatedTime = DateTime.UtcNow
            };

            _favoriteBookRepository.AddFavoriteBook(favoriteBook);
        }

        public void RemoveBookFromFavorites(string bookId, string userId)
        {
            if (!_bookRepository.BookIdExists(bookId))
                throw new InvalidOperationException("Book does not exist.");

            if (!_favoriteBookRepository.FavoriteBookExists(bookId, userId))
                throw new InvalidOperationException("Book is not in favorites.");

            var favoriteBook = _favoriteBookRepository.GetFavoriteBookByBookIdAndUserId(bookId, userId);
            _favoriteBookRepository.RemoveFavoriteBook(favoriteBook);
        }

        public BookDetailsViewModel GetBookDetailsById(string id)
        {
            var book = _bookRepository.GetBookById(id);
            if (book == null) return null;

            var model = new BookDetailsViewModel();
            _mapper.Map(book, model);

            model.Genres = book.GenreAssociations
                .Where(ga => ga.Genre.DeletedTime == null)
                .Select(ga => ga.Genre.Name)
                .ToList();

            model.AverageRating = (decimal)(book.BookReviews.Any()
                ? book.BookReviews.Average(r => r.Rating)
                : 0);

            model.Reviews = _reviewRepository.GetReviewsByBookId(book.BookId)
                .ToList()
                .Select(r =>
                {
                    var vm = new ReviewListItemViewModel();
                    _mapper.Map(r, vm);
                    vm.Reviewer = r.User.Username;
                    vm.BookName = r.Book.Title;
                    vm.Author = r.Book.Author;
                    vm.PublicationYear = r.Book.PublicationYear;
                    vm.ReviewBookCrImageUrl = r.Book.CoverImageUrl;
                    return vm;
                }).ToList();

            model.CreatedByUserName = book.CreatedByUser?.Username;
            model.UpdatedByUserName = book.UpdatedByUser?.Username;

            return model;
        }

        public BookViewModel GetBookEditById(string id)
        {
            var book = _bookRepository.GetBookById(id);
            if (book == null) return null;

            var model = new BookViewModel();
            _mapper.Map(book, model);
            model.SelectedGenres = book.GenreAssociations.Select(ga => ga.GenreId).ToList();

            return model;
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

        public void DeleteBook(string bookId, string deleterId)
        {
            if (!_bookRepository.BookIdExists(bookId))
                throw new InvalidOperationException("Book does not exist.");

            var book = _bookRepository.GetBookById(bookId);
            book.FavoritedbyUsers = _favoriteBookRepository.GetFavoriteBooksByBookId(bookId).ToList();

            foreach (var favoriteBook in book.FavoritedbyUsers)
                _favoriteBookRepository.RemoveFavoriteBook(favoriteBook);

            book.BookReviews = _reviewRepository.GetReviewsByBookId(bookId).ToList();

            foreach (var review in book.BookReviews)
            {
                review.DeletedBy = deleterId;
                review.DeletedTime = DateTime.UtcNow;
                _reviewRepository.UpdateReview(review);
            }

            book.DeletedBy = deleterId;
            book.DeletedTime = DateTime.UtcNow;
            _bookRepository.UpdateBook(book);
        }

        public List<BookListItemViewModel> GetBookList(string searchString, List<GenreViewModel> genres, string userID, BookSearchType searchType = BookSearchType.AllBooks, BookSortType sortType = BookSortType.CreatedTimeDescending)
        {
            // Placeholder logic (you can enhance this)
            return new List<BookListItemViewModel>();
        }

        public async Task AddBook(BookViewModel model, string creatorId)
        {
            // Implement this as needed
        }

        public async Task UpdateBook(BookViewModel model, string updaterId)
        {
            // Implement this as needed
        }

        public List<BookViewModel> GetBooksForGuest(string section)
        {
            var query = _bookRepository.GetAllActiveBooks();

            if (section == "top-books")
            {
                query = query
                    .Where(b => b.BookReviews.Any())
                    .OrderByDescending(b => b.BookReviews.Average(r => r.Rating));
            }
            else
            {
                query = query.OrderByDescending(b => b.CreatedTime);
            }

            return query
                .Take(5)
                .Select(book => _mapper.Map<BookViewModel>(book))
                .ToList();
        }
    }
}
