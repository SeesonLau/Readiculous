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
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
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

        public BookService(IBookRepository bookRepository, IGenreRepository genreRepository, IFavoriteBookRepository favoriteBookRepository, IReviewRepository reviewRepository, IMapper mapper, Client client)
        {
            _bookRepository = bookRepository;
            _genreRepository = genreRepository;
            _favoriteBookRepository = favoriteBookRepository;
            _reviewRepository = reviewRepository;
            _mapper = mapper;
            _client = client;
        }

        // CRUD Operations for Books
        public async Task AddBook(BookViewModel model, string creatorId)
        {
            // Books can only be added if the title and author combination does not already exist.
            if (_bookRepository.BookTitleAndAuthorExists(model.Title.Trim(), model.Author.Trim()))
            {
                throw new InvalidOperationException(Resources.Messages.Errors.BookTitleAndAuthorExists);
            }
            if (_bookRepository.ISBNExists(model.BookId, model.ISBN))
            {
                throw new InvalidOperationException(Resources.Messages.Errors.ISBNExists);
            }
            if (string.IsNullOrEmpty(model.CoverImageUrl))
            {
                model.CoverImageUrl = string.Empty;
            }

            var book = new Book();
            model.BookId = Guid.NewGuid().ToString();

            _mapper.Map(model, book);
            book.Title = book.Title.Trim();
            book.Description = book.Description.Trim();
            book.Author = book.Author.Trim();
            book.ISBN = book.ISBN.Trim();
            book.CreatedBy = creatorId;
            book.CreatedTime = DateTime.UtcNow;
            book.UpdatedBy = creatorId;
            book.UpdatedTime = DateTime.UtcNow;

            if (model.CoverImage != null && model.CoverImage.Length > 0)
            {
                var extension = Path.GetExtension(model.CoverImage.FileName);
                var fileName = Path.Combine(Const.BookDirectory, $"{book.BookId}{extension}");

                using (var memoryStream = new MemoryStream())
                {
                    await model.CoverImage.CopyToAsync(memoryStream);
                    var fileBytes = memoryStream.ToArray();

                    var uploadResult = await _client.Storage
                        .From(Const.BucketName)
                        .Upload(fileBytes, fileName, new Supabase.Storage.FileOptions
                        {
                            ContentType = model.CoverImage.ContentType,
                            Upsert = true
                        });

                    if (!string.IsNullOrEmpty(uploadResult))
                    {
                        book.CoverImageUrl = _client.Storage
                            .From(Const.BucketName)
                            .GetPublicUrl(fileName);
                    }
                    else
                    {
                        throw new Exception(Resources.Messages.Errors.ImageFailedToUpload);
                    }
                }
            }

            foreach (var genreId in model.SelectedGenres)
            {
                var genre = _genreRepository.GetGenreById(genreId);
                if (genre == null)
                {
                    throw new InvalidOperationException(Resources.Messages.Errors.GenreNotExist);
                }

                book.GenreAssociations.Add(new BookGenreAssignment
                {
                    BookId = book.BookId,
                    GenreId = genre.GenreId,
                });
            }

            _bookRepository.AddBook(book);
        }
        public async Task UpdateBook(BookViewModel model, string updaterId)
        {
            if (_bookRepository.BookTitleAndAuthorExists(model.Title, model.Author.Trim()))
            {
                throw new InvalidOperationException(Resources.Messages.Errors.BookTitleAndAuthorExists);
            }
            if(_bookRepository.ISBNExists(model.BookId, model.ISBN.Trim()))
            {
                throw new InvalidOperationException(Resources.Messages.Errors.ISBNExists);
            }

            var book = _bookRepository.GetBookById(model.BookId);
            if (book == null)
            {
                throw new InvalidOperationException(Resources.Messages.Errors.BookNotExists);
            }
            if (string.IsNullOrEmpty(model.CoverImageUrl))
            {
                model.CoverImageUrl = book.CoverImageUrl;
            }

            _mapper.Map(model, book);
            book.UpdatedBy = updaterId;
            book.UpdatedTime = DateTime.UtcNow;
            var selectedGenreIds = model.SelectedGenres ?? new List<string>();

            if (model.CoverImage != null && model.CoverImage.Length > 0)
            {
                var uri = new Uri(book.CoverImageUrl);
                var relativePath = uri.AbsolutePath.Replace(Const.StoragePath, string.Empty);

                var result = await _client.Storage
                    .From(Const.BucketName)
                    .Remove(new List<string> { relativePath });

                if (result == null)
                {
                    throw new InvalidOperationException(Resources.Messages.Errors.ImageFailedToDelete);
                }

                var extension = Path.GetExtension(model.CoverImage.FileName);
                var fileName = Path.Combine(Const.BookDirectory, $"{book.BookId}-{Guid.NewGuid():N}{extension}");

                using (var memoryStream = new MemoryStream())
                {
                    await model.CoverImage.CopyToAsync(memoryStream);
                    var fileBytes = memoryStream.ToArray();

                    var uploadResult = await _client.Storage
                        .From(Const.BucketName)
                        .Upload(fileBytes, fileName, new Supabase.Storage.FileOptions
                        {
                            ContentType = model.CoverImage.ContentType,
                            Upsert = true
                        });

                    if (!string.IsNullOrEmpty(uploadResult))
                    {
                        book.CoverImageUrl = _client.Storage
                            .From(Const.BucketName)
                            .GetPublicUrl(fileName);
                    }
                    else
                    {
                        throw new Exception(Resources.Messages.Errors.ImageFailedToUpload);
                    }
                }
            }

            var assignmentsToRemove = book.GenreAssociations
                .Where(ga => !selectedGenreIds.Contains(ga.GenreId))
                .ToList();

            foreach (var assignment in assignmentsToRemove)
            {
                book.GenreAssociations.Remove(assignment);
            }

            var existingGenreIds = book.GenreAssociations.Select(ga => ga.GenreId).ToList();
            var genreIdsToAdd = selectedGenreIds
                .Where(id => !existingGenreIds.Contains(id))
                .ToList();

            foreach (var genreId in genreIdsToAdd)
            {
                book.GenreAssociations.Add(new BookGenreAssignment
                {
                    GenreId = genreId,
                    BookId = book.BookId
                });
            }

            _bookRepository.UpdateBook(book);
        }
        public Task DeleteBook(string bookId, string deleterId)
        {
            if (!_bookRepository.BookIdExists(bookId))
            {
                throw new InvalidOperationException(Resources.Messages.Errors.ServerError);
            }

            var book = _bookRepository.GetBookById(bookId);
            book.FavoritedbyUsers = _favoriteBookRepository.GetFavoriteBooksByBookId(bookId).ToList();
            foreach (var favoriteBook in book.FavoritedbyUsers)
            {
                _favoriteBookRepository.RemoveFavoriteBook(favoriteBook);
            }

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

            return Task.CompletedTask;
        }

        // Multiple Book Listing Methods, ADD USEREID FOR FAVORITES
        public List<BookListItemViewModel> GetBookList(string searchString, List<GenreViewModel> genres, string userID, BookSearchType searchType = BookSearchType.AllBooks, BookSortType sortType = BookSortType.Latest, string? genreFilter = null)
        {
            if (string.IsNullOrEmpty(searchString) && (genres == null || !genres.Any()) && searchType == BookSearchType.AllBooks && sortType == BookSortType.Latest)
            {
                return ListAllActiveBooks(userID);
            }
            else if (string.IsNullOrEmpty(searchString))
            {
                return ListBooksByGenreList(genres: genres, userID: userID, searchType: searchType, sortType: sortType, genreFilter: genreFilter);
            }
            else if (genres == null || !genres.Any())
            {
                return ListBooksByTitle(bookTitle: searchString, userID: userID, searchType: searchType, sortType: sortType, genreFilter: genreFilter);
            }
            else
            {
                return ListBooksByTitleAndGenres(bookTitle: searchString, genres: genres, userID: userID, searchType: searchType, sortType: sortType, genreFilter: genreFilter);
            }
        }

        // Single Book Retrival Methods
        public BookDetailsViewModel GetBookDetailsById(string id)
        {
            var book = _bookRepository.GetBookById(id);
            if (book == null)
            {
                throw new InvalidOperationException(Resources.Messages.Errors.BookNotExists);
            }

            var model = new BookDetailsViewModel();

            _mapper.Map(book, model);
            model.Genres = _genreRepository.GetGenreNamesByBookId(book.BookId)
                .ToList();
            model.Reviews = _reviewRepository.GetReviewsByBookId(book.BookId)
                .ToList()
                .Select(r =>
                {
                    ReviewListItemViewModel reviewViewModel = new();

                    _mapper.Map(r, reviewViewModel);
                    reviewViewModel.Reviewer = r.User.Username;
                    reviewViewModel.BookName = r.Book.Title;
                    reviewViewModel.Author = r.Book.Author;
                    reviewViewModel.PublicationYear = r.Book.PublicationYear;
                    reviewViewModel.ReviewBookCrImageUrl = r.Book.CoverImageUrl;

                    return reviewViewModel;
                })
                .ToList();

            model.AverageRating = model.Reviews.Count != 0
                        ? (decimal)book.BookReviews.Average(r => r.Rating)
                        : 0;
            model.CreatedByUserName = book.CreatedByUser.Username;
            model.UpdatedByUserName = book.UpdatedByUser.Username;
            return model;
        }
        public BookViewModel GetBookEditById(string id)
        {
            var book = _bookRepository.GetBookById(id);
            if (book == null)
            {
                throw new InvalidOperationException(Resources.Messages.Errors.BookNotExists);
            }

            var model = new BookViewModel();
            _mapper.Map(book, model);
            model.SelectedGenres = book.GenreAssociations.Select(ga => ga.GenreId).ToList();
            return model;
        }

        // Book Dropdown Fillup Methods
        public List<SelectListItem> GetBookSearchTypes(BookSearchType searchType)
        {
            return Enum.GetValues(typeof(BookSearchType))
                .Cast<BookSearchType>()
                .Select(t => {
                    var displayName = t.GetType()
                                     .GetMember(t.ToString())
                                     .First()
                                     .GetCustomAttribute<DisplayAttribute>()?
                                     .Name ?? t.ToString();

                    return new SelectListItem
                    {
                        Value = ((int)t).ToString(),
                        Text = displayName,
                        Selected = t == searchType
                    };
                }).ToList();
        }
        public List<SelectListItem> GetBookSortTypes(BookSortType sortType)
        {
            return Enum.GetValues(typeof(BookSortType))
                .Cast<BookSortType>()
                .Select(t =>
                {
                    var displayName = t.GetType()
                                     .GetMember(t.ToString())
                                     .First()
                                     .GetCustomAttribute<DisplayAttribute>()?
                                     .Name ?? t.ToString();

                    return new SelectListItem
                    {
                        Value = ((int)t).ToString(),
                        Text = displayName,
                        Selected = t == sortType
                    };
                }).ToList();
        }

        // String Helper Functions
        public string GetTitleByBookId(string bookId)
        {
            var book = _bookRepository.GetBookById(bookId);
            if (book == null)
            {
                throw new InvalidOperationException(Resources.Messages.Errors.BookNotExists);
            }

            return book.Title;
        }

        // Private Helper Methods for Book Listing
        private List<BookListItemViewModel> ListAllActiveBooks(string userID)
        {
            var books = _bookRepository.GetAllActiveBooks()
                .ToList()
                .Select(book =>
                {
                    var model = new BookListItemViewModel();

                    _mapper.Map(book, model);
                    model.Genres = _genreRepository.GetGenreNamesByBookId(book.BookId)
                        .ToList();
                    model.IsFavorite = _favoriteBookRepository.FavoriteBookExists(book.BookId, userID);
                    model.IsReviewed = _reviewRepository.ReviewExists(book.BookId, userID);
                    model.AverageRating = (decimal)(_reviewRepository.GetReviewsByBookId(book.BookId).ToList().Count != 0
                        ? book.BookReviews.Average(r => r.Rating)
                        : 0);
                    model.CreatedByUserName = book.CreatedByUser.Username;
                    model.UpdatedByUserName = book.UpdatedByUser.Username;

                    return model;
                })
                .OrderByDescending(b => b.CreatedTime)
                .ToList();
            return books;
        }
        private List<BookListItemViewModel> ListBooksByTitle(string bookTitle, string userID, BookSearchType searchType = BookSearchType.AllBooks, BookSortType sortType = BookSortType.Latest, string? genreFilter = null)
        {
            var books = _bookRepository.GetBooksByTitle(bookTitle.Trim())
                .Where(b => b.DeletedTime == null)
                .ToList()
                .Select(book =>
                {
                    var model = new BookListItemViewModel();

                    _mapper.Map(book, model);
                    model.Genres = _genreRepository.GetGenreNamesByBookId(book.BookId)
                        .ToList();
                    model.IsFavorite = _favoriteBookRepository.FavoriteBookExists(book.BookId, userID);
                    model.IsReviewed = _reviewRepository.ReviewExists(book.BookId, userID);
                    model.CreatedByUserName = book.CreatedByUser.Username;
                    model.UpdatedByUserName = book.UpdatedByUser.Username;
                    model.AverageRating = (decimal)(_reviewRepository.GetReviewsByBookId(book.BookId).ToList().Count != 0
                        ? book.BookReviews.Average(r => r.Rating)
                        : 0);

                    return model;
                });

            return SearchAndSortBook(books, searchType, sortType, genreFilter)
                .ToList();
        }
        private List<BookListItemViewModel> ListBooksByGenreList(List<GenreViewModel> genres, string userID, BookSearchType searchType = BookSearchType.AllBooks, BookSortType sortType = BookSortType.Latest, string? genreFilter = null)
        {
            var bookGenres = genres.Select(g =>
            {
                Genre genre = new();
                _mapper.Map(g, genre);

                return genre;
            })
                .ToList();

            var books = _bookRepository.GetBooksByGenreList(bookGenres)
                .ToList()
                .Select(book =>
                {
                    var model = new BookListItemViewModel();

                    _mapper.Map(book, model);
                    model.Genres = _genreRepository.GetGenreNamesByBookId(book.BookId)
                        .ToList();
                    model.IsFavorite = _favoriteBookRepository.FavoriteBookExists(book.BookId, userID);
                    model.IsReviewed = _reviewRepository.ReviewExists(book.BookId, userID);
                    model.CreatedByUserName = book.CreatedByUser.Username;
                    model.UpdatedByUserName = book.UpdatedByUser.Username; 
                    model.AverageRating = (decimal)(_reviewRepository.GetReviewsByBookId(book.BookId).ToList().Count != 0
                        ? book.BookReviews.Average(r => r.Rating)
                        : 0);

                    return model;
                });

            return SearchAndSortBook(books, searchType, sortType, genreFilter)
                .ToList();
        }
        private List<BookListItemViewModel> ListBooksByTitleAndGenres(string bookTitle, List<GenreViewModel> genres, string userID, BookSearchType searchType = BookSearchType.AllBooks, BookSortType sortType = BookSortType.Latest, string? genreFilter = null)
        {
            var bookGenres = genres.Select(g =>
            {
                var genre = _genreRepository.GetGenreById(g.GenreId);
                return genre;
            })
                .ToList();

            var books = _bookRepository.GetBooksByTitleAndGenres(bookTitle, bookGenres)
                .ToList()
                .Select(book =>
                {
                    var model = new BookListItemViewModel();

                    _mapper.Map(book, model);
                    model.Genres = _genreRepository.GetGenreNamesByBookId(book.BookId)
                        .ToList();
                    model.IsFavorite = _favoriteBookRepository.FavoriteBookExists(book.BookId, userID);
                    model.IsReviewed = _reviewRepository.ReviewExists(book.BookId, userID);
                    model.CreatedByUserName = book.CreatedByUser.Username;
                    model.UpdatedByUserName = book.UpdatedByUser.Username;
                    model.AverageRating = (decimal)(_reviewRepository.GetReviewsByBookId(book.BookId).ToList().Count != 0
                        ? book.BookReviews.Average(r => r.Rating)
                        : 0);

                    return model;
                });

            return SearchAndSortBook(books, searchType, sortType, genreFilter)
                .ToList();
        }
        
        // Search And Sort Book Helper Function
        private IEnumerable<BookListItemViewModel> SearchAndSortBook(IEnumerable<BookListItemViewModel> books, BookSearchType searchType, BookSortType sortType, string? genreFilter)
        {
            switch (searchType)
            {
                case BookSearchType.AllBooks:
                    break;

                case BookSearchType.TopBooks:
                    books = books.OrderByDescending(b => b.AverageRating);

                    switch (sortType)
                    {
                        case BookSortType.RatingAscending:
                            return books.Reverse();
                        case BookSortType.RatingDescending:
                            return books;
                        default:
                            break;
                    }
                    break;

                case BookSearchType.NewBooks:
                    DateTime twoWeeksAgo = DateTime.UtcNow.AddDays(-14);
                    books = books.Where(b => b.CreatedTime >= twoWeeksAgo);
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrWhiteSpace(genreFilter))
            {
                books = books.Where(b => b.Genres.Any(g =>
                    string.Equals(g, genreFilter, StringComparison.OrdinalIgnoreCase)));
            }

            return sortType switch
            {
                /*BookSortType.GenreAscending => books.OrderBy(b => b.Genres
                    .Select(name => name)
                    .OrderBy(name => name)
                    .FirstOrDefault()),
                BookSortType.GenreDescending => books.OrderBy(b => b.Genres
                    .Select(name => name)
                    .OrderByDescending(name => name)
                    .FirstOrDefault()),*/
                BookSortType.TitleAscending => books.OrderBy(b => b.Title),
                BookSortType.TitleDescending => books.OrderByDescending(b => b.Title),
                BookSortType.AuthorAscending => books.OrderBy(b => b.Author),
                BookSortType.AuthorDescending => books.OrderByDescending(b => b.Author),
                BookSortType.RatingAscending => books.OrderByDescending(b => b.AverageRating),
                BookSortType.RatingDescending => books.OrderBy(b => b.AverageRating),
               // BookSortType.SeriesAscending => books.OrderBy(b => b.SeriesNumber),
               // BookSortType.SeriesDescending => books.OrderByDescending(b => b.SeriesNumber),
                BookSortType.Oldest => books.OrderBy(b => b.CreatedTime),
                BookSortType.Latest => books.OrderByDescending(b => b.CreatedTime),
                _ => books, // Default case
            };
        }

        // Favorite Book Methods
        public void AddBookToFavorites(string bookId, string userId)
        {
            if (!_bookRepository.BookIdExists(bookId))
            {
                throw new InvalidOperationException(Resources.Messages.Errors.BookNotExists);
            }
            if (_favoriteBookRepository.FavoriteBookExists(bookId, userId))
            {
                throw new InvalidOperationException(Resources.Messages.Errors.FavoriteBookExists);
            }

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
            {
                throw new InvalidOperationException(Resources.Messages.Errors.BookNotExists);
            }
            if (!_favoriteBookRepository.FavoriteBookExists(bookId, userId))
            {
                throw new InvalidOperationException(Resources.Messages.Errors.FavoritedBookNotExists);
            }

            var favoriteBook = _favoriteBookRepository.GetFavoriteBookByBookIdAndUserId(bookId, userId);

            _favoriteBookRepository.RemoveFavoriteBook(favoriteBook);
        }
    }
}
