using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Readiculous.Data.Interfaces;
using Readiculous.Data.Models;
using Readiculous.Resources.Constants;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using Supabase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using X.PagedList;
using X.PagedList.Extensions;
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
            if (_bookRepository.BookTitleAndAuthorExists(model.Title.Trim(), model.Author.Trim(), model.BookId))
            {
                throw new DuplicateNameException(Resources.Messages.Errors.BookTitleAndAuthorExists);
            }
            if (_bookRepository.ISBNExists(model.BookId, model.ISBN))
            {
                throw new DuplicateNameException(Resources.Messages.Errors.ISBNExists);
            }
            if (string.IsNullOrEmpty(model.CoverImageUrl))
            {
                model.CoverImageUrl = string.Empty;
            }

            var book = new Book();
            model.BookId = Guid.NewGuid().ToString();

            _mapper.Map(model, book);
            book.CreatedBy = creatorId;
            book.CreatedTime = DateTime.UtcNow;
            book.UpdatedBy = creatorId;
            book.UpdatedTime = DateTime.UtcNow;

            if (model.CoverImage != null && model.CoverImage.Length > 0)
            {
                book.CoverImageUrl = await UploadCoverImage(model.CoverImage, model.BookId);
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

            await Task.Run(() => _bookRepository.AddBook(book));
        }
        public async Task UpdateBook(BookViewModel model, string updaterId)
        {
            if (_bookRepository.BookTitleAndAuthorExists(model.Title, model.Author.Trim(), model.BookId))
            {
                throw new InvalidOperationException(Resources.Messages.Errors.BookTitleAndAuthorExists);
            }
            if (_bookRepository.ISBNExists(model.BookId, model.ISBN.Trim()))
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
                if(!string.IsNullOrEmpty(book.CoverImageUrl))
                    await DeleteCoverImage(book.CoverImageUrl);

                book.CoverImageUrl = await UploadCoverImage(model.CoverImage, book.BookId);
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

            await Task.Run(() => _bookRepository.UpdateBook(book));
        }
        public async Task DeleteBook(string bookId, string deleterId)
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

            await Task.Run(() => _bookRepository.UpdateBook(book));
        }

        // Multiple Book Listing Methods, ADD USEREID FOR FAVORITES
        public List<BookListItemViewModel> GetBookList(string searchString, List<GenreViewModel> genres, string userID, BookSortType sortType = BookSortType.Latest, string? genreFilter = null)
        {
            if (string.IsNullOrEmpty(searchString) && (genres == null || !genres.Any()) && sortType == BookSortType.Latest)
            {
                return ListAllActiveBooks(userID);
            }
            else if (string.IsNullOrEmpty(searchString))
            {
                return ListBooksByGenreList(genreViewModels: genres, userID: userID, sortType: sortType, genreFilter: genreFilter);
            }
            else if (genres == null || !genres.Any())
            {
                return ListBooksByTitle(bookTitle: searchString, userID: userID, sortType: sortType, genreFilter: genreFilter);
            }
            else
            {
                return ListBooksByTitleAndGenres(bookTitle: searchString, genreViewModels: genres, userID: userID, sortType: sortType, genreFilter: genreFilter);
            }
        }

        public IPagedList<BookListItemViewModel> GetPaginatedBookList(string searchString, List<GenreViewModel> genres, string userId, int pageNumber, int pageSize = 10, BookSortType sortType = BookSortType.Latest, string? genreFilter = null)
        {
            if (string.IsNullOrEmpty(searchString) && (genres == null || !genres.Any()) && sortType == BookSortType.Latest)
            {
                return ListAllPaginatedActiveBooks(userId, pageNumber, pageSize);
            }
            else if (string.IsNullOrEmpty(searchString))
            {
                return ListPaginatedBooksByGenreList(genreViewModels: genres, userId: userId, pageNumber: pageNumber, pageSize: pageSize, sortType: sortType, genreFilter: genreFilter);
            }
            else if (genres == null || !genres.Any())
            {
                return ListPaginatedBooksByTitle(bookTitle: searchString, userId: userId, pageNumber, pageSize, sortType: sortType, genreFilter: genreFilter);
            }
            else
            {
                return ListPaginatedBooksByTitleAndGenres(bookTitle: searchString, genreViewModels: genres, userId: userId, pageNumber, pageSize, sortType: sortType, genreFilter: genreFilter);
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
            
            var reviews = _reviewRepository.GetReviewsByBookId(book.BookId);
            model.Reviews = _mapper.Map<List<ReviewListItemViewModel>>(reviews);

            model.AverageRating = model.Reviews.Count != 0
                        ? (decimal)(book.BookReviews.Average(r => r.Rating))
                        : 0;
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

        // Favorite Book Methods
        public void AddBookToFavorites(string bookId, string userId)
        {
            if (!_bookRepository.BookIdExists(bookId))
            {
                throw new KeyNotFoundException(Resources.Messages.Errors.BookNotExists);
            }
            if (_favoriteBookRepository.FavoriteBookExists(bookId, userId))
            {
                throw new DuplicateNameException(Resources.Messages.Errors.FavoriteBookExists);
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
                throw new KeyNotFoundException(Resources.Messages.Errors.BookNotExists);
            }
            if (!_favoriteBookRepository.FavoriteBookExists(bookId, userId))
            {
                throw new KeyNotFoundException(Resources.Messages.Errors.FavoritedBookNotExists);
            }

            var favoriteBook = _favoriteBookRepository.GetFavoriteBookByBookIdAndUserId(bookId, userId);

            _favoriteBookRepository.RemoveFavoriteBook(favoriteBook);
        }

        // Dashboard Book Retrieval Functions
        public UserDashboardViewModel GetDashboardViewModel()
        {
            UserDashboardViewModel dashboardViewModel = new();
            DateTime twoWeeksAgo = DateTime.UtcNow.AddDays(-14);

            var allBooks = _bookRepository.GetAllActiveBooks();
            var booksMapModels = _mapper.Map<List<BookListItemViewModel>>(allBooks);

            dashboardViewModel.TopBooks = booksMapModels
                .Where(b => b.CreatedTime >= twoWeeksAgo)
                .OrderByDescending(b => b.CreatedTime)
                .Take(5)
                .ToList();
            dashboardViewModel.NewBooks = booksMapModels
                .OrderByDescending(b => b.AverageRating)
                .Take(5)
                .ToList();

            return dashboardViewModel;
        }

        // Private Helper Methods for Book Listing
        private List<BookListItemViewModel> ListAllActiveBooks(string userID)
        {
            var allActiveBooks = _bookRepository.GetAllActiveBooks();
            var bookIds = allActiveBooks.Select(s => s.BookId).ToList();
            var genres = _genreRepository.GetAllGenreAssignmentsByBookIds(bookIds);
            var allReviews = _reviewRepository.GetAllReviews();

            var bookMapModels = _mapper.Map<List<BookListItemViewModel>>(allActiveBooks);

            foreach (var model in bookMapModels)
            {
                model.Genres = genres
                    .Where(s => s.BookId == model.BookId)
                    .Select(s=>s.Genre.Name)
                    .ToList();

                var bookReviews = allReviews
                    .Where(r =>  r.BookId == model.BookId)
                    .ToList();
                model.AverageRating = (decimal) (bookReviews.Count > 0
                    ? bookReviews.Average(r => r.Rating)
                    : 0);
            }

            var result = bookMapModels.OrderByDescending(o => o.UpdatedTime).ToList();
            return result;
        }
        private IPagedList<BookListItemViewModel> ListAllPaginatedActiveBooks(string userId, int pageNumber, int pageSize)
        {
            IQueryable<Book> queryableBookListItems;
            int bookCount;

            (queryableBookListItems, bookCount) = _bookRepository.GetAllPaginatedBooks(pageNumber, pageSize);
            var listBookListItems = queryableBookListItems.ToList();
            var bookIds = listBookListItems.Select(s => s.BookId).ToList();
            var genres = _genreRepository.GetAllGenreAssignmentsByBookIds(bookIds);
            var allReviews = _reviewRepository.GetAllReviews();

            var bookMapModels = _mapper.Map<List<BookListItemViewModel>>(listBookListItems);
            PopulateListItem(bookMapModels, genres, allReviews);

            var result = bookMapModels
                .OrderByDescending(o => o.CreatedTime);
            return new StaticPagedList<BookListItemViewModel>(
                result.ToList(),
                pageNumber,
                pageSize,
                bookCount
                );
        }
        private List<BookListItemViewModel> ListBooksByTitle(string bookTitle, string userID, BookSortType sortType = BookSortType.Latest, string? genreFilter = null)
        {
            var booksByTitle = _bookRepository.GetBooksByTitle(bookTitle);
            var bookIds = booksByTitle.Select(s => s.BookId).ToList();
            var genres = _genreRepository.GetAllGenreAssignmentsByBookIds(bookIds);
            var favoriteBooksByUser = _favoriteBookRepository.GetFavoriteBooksByUserId(userID);
            var reviewsByUser = _reviewRepository.GetReviewsByUserId(userID);
            var allReviews = _reviewRepository.GetAllReviews();
            var bookMapModels = _mapper.Map<List<BookListItemViewModel>>(booksByTitle);

            foreach(var model in bookMapModels)
            {
                model.Genres = genres
                    .Where(s => s.BookId == model.BookId)
                    .Select(s => s.Genre.Name)
                    .ToList();

                var bookReviews = allReviews
                    .Where(r => r.BookId == model.BookId)
                    .ToList();
                model.AverageRating = (decimal)(bookReviews.Any()
                    ? bookReviews.Average(r => r.Rating)
                    : 0);
            }
            return SortBook(bookMapModels, sortType, genreFilter)
                .ToList();
        }

        private IPagedList<BookListItemViewModel> ListPaginatedBooksByTitle(string bookTitle, string userId, int pageNumber, int pageSize = 10, BookSortType sortType = BookSortType.Latest, string?  genreFilter = null)
        {
            IQueryable<Book> queryableBookListItems;
            int bookCount;

            (queryableBookListItems, bookCount) = _bookRepository.GetPaginatedBooksByTitle(bookTitle, pageNumber, pageSize, sortType);
            var listBookListItems = queryableBookListItems.ToList();
            var bookIds = listBookListItems.Select(s => s.BookId).ToList();
            var genres = _genreRepository.GetAllGenreAssignmentsByBookIds(bookIds);
            var favoriteBooksByUser = _favoriteBookRepository.GetFavoriteBooksByUserId(userId);
            var reviewsByUser = _reviewRepository.GetReviewsByUserId(userId);
            var allReviews = _reviewRepository.GetAllReviews();

            var bookMapModels = _mapper.Map<List<BookListItemViewModel>>(listBookListItems);
            PopulateListItem(bookMapModels, genres, allReviews);

            return new StaticPagedList<BookListItemViewModel>(
                bookMapModels.ToList(),
                pageNumber,
                pageSize,
                bookCount
                );
        }
        
        private List<BookListItemViewModel> ListBooksByGenreList(List<GenreViewModel> genreViewModels, string userID, BookSortType sortType = BookSortType.Latest, string? genreFilter = null)
        {
            var bookGenres = _mapper.Map<List<Genre>>(genreViewModels);

            var booksByGenre = _bookRepository.GetBooksByGenreList(bookGenres);
            var bookIds = booksByGenre
                .Select(s => s.BookId).ToList();
            var genres = _genreRepository.GetAllGenreAssignmentsByBookIds(bookIds);
            var favoriteBooksByUser = _favoriteBookRepository.GetFavoriteBooksByUserId(userID);
            var reviewsByUser = _reviewRepository.GetReviewsByUserId(userID);
            var allReviews = _reviewRepository.GetAllReviews();

            var bookViewModels = _mapper.Map<List<BookListItemViewModel>>(booksByGenre);

            foreach (var model in bookViewModels)
            {
                model.Genres = genres
                    .Where(s => s.BookId == model.BookId)
                    .Select(s => s.Genre.Name)
                    .ToList();

                var bookReviews = allReviews
                    .Where(r => r.BookId == model.BookId)
                    .ToList();
                model.AverageRating = (decimal)(bookReviews.Any()
                    ? bookReviews.Average(r => r.Rating)
                    : 0);
            }

            return SortBook(bookViewModels, sortType, genreFilter)

                .ToList();
        }

        private IPagedList<BookListItemViewModel> ListPaginatedBooksByGenreList(List<GenreViewModel> genreViewModels, string userId, int pageNumber, int pageSize = 10, BookSortType sortType = BookSortType.Latest, string? genreFilter = null)
        {
            var bookGenres = _mapper.Map<List<Genre>>(genreViewModels);
            IQueryable<Book> queryableBookListItems;
            int bookCount;

            (queryableBookListItems, bookCount) = _bookRepository.GetPaginatedBooksByGenreList(bookGenres, pageNumber, pageSize, sortType);
            var listBookListItems = queryableBookListItems.ToList();
            var bookIds = listBookListItems.Select(s => s.BookId).ToList();
            var genres = _genreRepository.GetAllGenreAssignmentsByBookIds(bookIds);
            var allReviews = _reviewRepository.GetAllReviews();

            var bookMapModels = _mapper.Map<List<BookListItemViewModel>>(listBookListItems);
            PopulateListItem(bookMapModels, genres, allReviews);
            
            return new StaticPagedList<BookListItemViewModel>(
                bookMapModels.ToList(),
                pageNumber,
                pageSize,
                bookCount
                );
        }
        private List<BookListItemViewModel> ListBooksByTitleAndGenres(string bookTitle, List<GenreViewModel> genreViewModels, string userID, BookSortType sortType = BookSortType.Latest, string? genreFilter = null)
        {
            var bookGenres = _mapper.Map<List<Genre>>(genreViewModels);
            
            var booksByTitleAndGenre = _bookRepository.GetBooksByTitleAndGenres(bookTitle, bookGenres);
            var bookIds = booksByTitleAndGenre
                .Select(book => book.BookId)
                .ToList();
            var genres = _genreRepository.GetAllGenreAssignmentsByBookIds(bookIds);
            var allReviews = _reviewRepository.GetAllReviews();

            var bookViewModels = _mapper.Map<List<BookListItemViewModel>>(booksByTitleAndGenre);

            foreach (var model in bookViewModels)
            {
                model.Genres = genres
                    .Where(s => s.BookId == model.BookId)
                    .Select(s => s.Genre.Name)
                    .ToList();

                var bookReviews = allReviews
                    .Where(r => r.BookId == model.BookId)
                    .ToList();
                model.AverageRating = (decimal)(bookReviews.Any()
                    ? bookReviews.Average(r => r.Rating)
                    : 0);
            }

            return SortBook(bookViewModels, sortType, genreFilter)

                .ToList();
        }
        private IPagedList<BookListItemViewModel> ListPaginatedBooksByTitleAndGenres(string bookTitle, List<GenreViewModel> genreViewModels, string userId, int pageNumber, int pageSize, BookSortType sortType = BookSortType.Latest, string? genreFilter = null)
        {

            var bookGenres = _mapper.Map<List<Genre>>(genreViewModels);
            IQueryable<Book> queryableBookListItems;
            int bookCount;

            (queryableBookListItems, bookCount) = _bookRepository.GetPaginatedBooksByTitleAndGenres(bookTitle, bookGenres, pageNumber, pageSize, sortType);
            var listBookListItems = queryableBookListItems.ToList();
            var bookIds = listBookListItems.Select(s => s.BookId).ToList();
            var genres = _genreRepository.GetAllGenreAssignmentsByBookIds(bookIds);
            var allReviews = _reviewRepository.GetAllReviews();

            var bookMapModels = _mapper.Map<List<BookListItemViewModel>>(listBookListItems);
            PopulateListItem(bookMapModels, genres, allReviews);

            return new StaticPagedList<BookListItemViewModel>(
                bookMapModels.ToList(),
                pageNumber,
                pageSize,
                bookCount
                );
        }

        // Search And Sort Book Helper Function
        private IEnumerable<BookListItemViewModel> SortBook(IEnumerable<BookListItemViewModel> bookViewModels, BookSortType sortType = BookSortType.Latest, string? genreFilter = null)
        {
            if (!string.IsNullOrWhiteSpace(genreFilter))
            {
                bookViewModels = bookViewModels.Where(b => b.Genres.Any(g =>
                    string.Equals(g, genreFilter, StringComparison.OrdinalIgnoreCase)));
            }
            DateTime twoWeeksAgo = DateTime.UtcNow.AddDays(-14);
            return sortType switch
            {

                BookSortType.TitleAscending => bookViewModels.OrderBy(b => b.Title),
                BookSortType.TitleDescending => bookViewModels.OrderByDescending(b => b.Title),
                BookSortType.AuthorAscending => bookViewModels.OrderBy(b => b.Author),
                BookSortType.AuthorDescending => bookViewModels.OrderByDescending(b => b.Author),
                BookSortType.RatingAscending => bookViewModels.OrderByDescending(b => b.AverageRating),
                BookSortType.RatingDescending => bookViewModels.OrderBy(b => b.AverageRating),
                BookSortType.Oldest => bookViewModels.OrderBy(b => b.UpdatedTime),
                BookSortType.Latest => bookViewModels.OrderByDescending(b => b.UpdatedTime),
                BookSortType.NewBooksAscending => bookViewModels
                    .Where(b => b.CreatedTime >= twoWeeksAgo)
                    .OrderBy(b => b.CreatedTime),
                BookSortType.NewBooksDescending => bookViewModels
                    .Where(b => b.CreatedTime >= twoWeeksAgo)
                    .OrderByDescending(b => b.CreatedTime),
                _ => bookViewModels, // Default case

            };
        }

        // Upload and Delete CoverImage Helper Functions
        private async Task DeleteCoverImage(string pictureUrl)
        {
            var uri = new Uri(pictureUrl);
            var relativePath = uri.AbsolutePath.Replace(Const.StoragePath, string.Empty);

            var result = await _client.Storage
                .From(Const.BucketName)
                .Remove(new List<string> { relativePath });

            if (result == null)
            {
                throw new InvalidOperationException(Resources.Messages.Errors.ImageFailedToDelete);
            }
        }
        private async Task<string> UploadCoverImage(IFormFile file, string bookId)
        {
            var extension = Path.GetExtension(file.FileName);
            var fileName = Path.Combine(Const.BookDirectory, $"{bookId}-{Guid.NewGuid():N}{extension}");

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                var uploadResult = await _client.Storage
                    .From(Const.BucketName)
                    .Upload(fileBytes, fileName, new Supabase.Storage.FileOptions
                    {
                        ContentType = file.ContentType,
                        Upsert = true
                    });

                if (!string.IsNullOrEmpty(uploadResult))
                {
                    return _client.Storage
                        .From(Const.BucketName)
                        .GetPublicUrl(fileName);
                }
            }

            throw new InvalidOperationException(Resources.Messages.Errors.ImageFailedToUpload);
        }

        private void PopulateListItem(List<BookListItemViewModel> bookMapModels, IQueryable<BookGenreAssignment> genres, IQueryable<Review> allReviews)
        {
            foreach (var model in bookMapModels)
            {
                model.Genres = genres
                    .Where(s => s.BookId == model.BookId)
                    .Select(s => s.Genre.Name)
                    .ToList();

                var bookReviews = allReviews
                    .Where(r => r.BookId == model.BookId)
                    .ToList();
                model.AverageRating = (decimal)(bookReviews.Count > 0
                    ? bookReviews.Average(r => r.Rating)
                    : 0);
            }
        }
    }
}
