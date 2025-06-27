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
using System.Data.Entity;
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
        private readonly IMapper _mapper;
        private readonly Client _client;

        public BookService(IBookRepository bookRepository, IGenreRepository genreRepository, IMapper mapper, Client client)
        {
            _bookRepository = bookRepository;
            _genreRepository = genreRepository;
            _mapper = mapper;
            _client = client;
        }

        // CRUD Operations for Books
        public async Task AddBook(BookViewModel model, string creatorId)
        {
            // Books can only be added if the title and author combination does not already exist.
            if (_bookRepository.BookTitleAndAuthorExists(model.Title.Trim(), model.Author.Trim()) && !_bookRepository.ISBNExists(model.BookId, model.ISBN.Trim()))
            {
                throw new InvalidOperationException(Resources.Messages.Errors.BookTitleAndAuthorExists);
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
            if (!_bookRepository.BookTitleAndAuthorExists(model.Title, model.Author) && !_bookRepository.ISBNExists(model.BookId, model.ISBN.Trim()))
            {
                throw new InvalidOperationException(Resources.Messages.Errors.BookTitleAndAuthorExists);
            }

            var book = _bookRepository.GetBookById(model.BookId);
            if (book == null)
            {
                throw new InvalidOperationException("Book does not exist.");
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
        public void DeleteBook(string bookId, string deleterId)
        {
            if (!_bookRepository.BookIdExists(bookId))
            {
                throw new InvalidOperationException("Book does not exist.");
            }

            var book = _bookRepository.GetBookById(bookId);
            book.DeletedBy = deleterId;
            book.DeletedTime = DateTime.UtcNow;
            _bookRepository.UpdateBook(book);
        }

        // Multiple Book Listing Methods
        public List<BookListItemViewModel> GetBookList(string searchString, List<GenreViewModel> genres, BookSearchType searchType = BookSearchType.AllBooks, BookSortType sortType = BookSortType.CreatedTimeDescending)
        {
            if (string.IsNullOrEmpty(searchString) && (genres == null || !genres.Any()) && searchType == BookSearchType.AllBooks && sortType == BookSortType.CreatedTimeDescending)
            {
                return ListAllActiveBooks();
            }
            else if (string.IsNullOrEmpty(searchString))
            {
                return ListBooksByGenreList(genres: genres, searchType: searchType, sortType: sortType);
            }
            else if (genres == null || !genres.Any())
            {
                return ListBooksByTitle(bookTitle: searchString, searchType: searchType, sortType: sortType);
            }
            else
            {
                return ListBooksByTitleAndGenres(bookTitle: searchString, genres: genres, searchType: searchType, sortType: sortType);
            }
        }

        // Single Book Retrival Methods
        public BookDetailsViewModel GetBookDetailsById(string id)
        {
            var book = _bookRepository.GetBookById(id);
            if (book == null)
            {
                return null;
            }

            var model = new BookDetailsViewModel();


            //TO ADD: REVIEW COUNT AND RATING AVERAGE
            _mapper.Map(book, model);
            model.Genres = book.GenreAssociations
                .Where(ga => ga.Genre.DeletedTime == null)
                .Select(ga => ga.Genre.Name)
                .ToList();
            model.Reviews = book.BookReviews
                .Select(br =>
                {
                    ReviewViewModel reviewModel = new ReviewViewModel();
                    _mapper.Map(br, reviewModel);
                    return reviewModel;
                })
                .ToList();
            model.CreatedByUserName = book.CreatedByUser.Username;
            model.UpdatedByUserName = book.UpdatedByUser.Username;
            return model;
        }
        public BookViewModel GetBookEditById(string id)
        {
            var book = _bookRepository.GetBookById(id);
            if (book == null)
            {
                return null;
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
                .Select(t => new SelectListItem
                {
                    Value = ((int)t).ToString(),
                    Text = t.ToString(),
                    Selected = t == searchType
                }).ToList();
        }
        public List<SelectListItem> GetBookSortTypes(BookSortType sortType)
        {
            return Enum.GetValues(typeof(BookSortType))
                .Cast<BookSortType>()
                .Select(t => new SelectListItem
                {
                    Value = ((int)t).ToString(),
                    Text = t.ToString(),
                    Selected = t == sortType
                }).ToList();
        }

        // Private Helper Methods for Book Listing
        private List<BookListItemViewModel> ListAllActiveBooks()
        {
            var books = _bookRepository.GetAllActiveBooks()
                .ToList()
                .Select(book =>
                {
                    var model = new BookListItemViewModel();

                    //TO ADD: REVIEW COUNT AND RATING AVERAGE
                    _mapper.Map(book, model);
                    model.Genres = book.GenreAssociations
                        .Where(ga => ga.Genre.DeletedTime == null)
                        .Select(ga => ga.Genre.Name)
                        .ToList();
                    model.CreatedByUserName = book.CreatedByUser.Username;
                    model.UpdatedByUserName = book.UpdatedByUser.Username;
                    return model;
                })
                .OrderByDescending(b => b.CreatedTime)
                .ToList();
            return books;
        }
        private List<BookListItemViewModel> ListBooksByTitle(string bookTitle, BookSearchType searchType = BookSearchType.AllBooks, BookSortType sortType = BookSortType.CreatedTimeDescending)
        {
            var books = _bookRepository.GetBooksByTitle(bookTitle.Trim())
                .Where(b => b.DeletedTime == null)
                .ToList()
                .Select(book =>
                {
                    var model = new BookListItemViewModel();

                    //TO ADD: REVIEW COUNT AND RATING AVERAGE
                    _mapper.Map(book, model);
                    model.Genres = book.GenreAssociations
                        .Where(ga => ga.Genre.DeletedTime == null)
                        .Select(ga => ga.Genre.Name)
                        .ToList();
                    model.CreatedByUserName = book.CreatedByUser.Username;
                    model.UpdatedByUserName = book.UpdatedByUser.Username;
                    model.AverageRating = (decimal)(book.BookReviews.Count != 0
                        ? book.BookReviews.Average(r => r.Rating)
                        : 0);
                    return model;
                });

            return SearchAndSortBook(books, searchType, sortType)
                .ToList();
        }
        private List<BookListItemViewModel> ListBooksByGenreList(List<GenreViewModel> genres, BookSearchType searchType = BookSearchType.AllBooks, BookSortType sortType = BookSortType.CreatedTimeDescending)
        {
            var bookGenres = genres.Select(g =>
            {
                var genre = _genreRepository.GetGenreById(g.GenreId);
                return genre;
            })
                .ToList();

            var books = _bookRepository.GetBooksByGenreList(bookGenres)
                .Where(b => b.DeletedTime == null)
                .ToList()
                .Select(book =>
                {
                    var model = new BookListItemViewModel();
                    _mapper.Map(book, model);
                    model.Genres = book.GenreAssociations
                        .Where(ga => ga.Genre.DeletedTime == null)
                        .Select(ga => ga.Genre.Name)
                        .ToList();
                    model.CreatedByUserName = book.CreatedByUser.Username;
                    model.UpdatedByUserName = book.UpdatedByUser.Username;
                    model.AverageRating = (decimal)(book.BookReviews.Count != 0
                        ? book.BookReviews.Average(r => r.Rating)
                        : 0);
                    return model;
                });

            return SearchAndSortBook(books, searchType, sortType)
                .ToList();
        }
        private List<BookListItemViewModel> ListBooksByTitleAndGenres(string bookTitle, List<GenreViewModel> genres, BookSearchType searchType = BookSearchType.AllBooks, BookSortType sortType = BookSortType.CreatedTimeAscending)
        {
            var bookGenres = genres.Select(g =>
            {
                var genre = _genreRepository.GetGenreById(g.GenreId);
                return genre;
            })
                .ToList();

            var books = _bookRepository.GetBooksByTitleAndGenres(bookTitle, bookGenres)
                .Where(b => b.DeletedTime == null)
                .ToList()
                .Select(book =>
                {
                    var model = new BookListItemViewModel();
                    _mapper.Map(book, model);
                    model.Genres = book.GenreAssociations
                        .Where(ga => ga.Genre.DeletedTime == null)
                        .Select(ga => ga.Genre.Name)
                        .ToList();
                    model.CreatedByUserName = book.CreatedByUser.Username;
                    model.UpdatedByUserName = book.UpdatedByUser.Username;
                    model.AverageRating = (decimal)(book.BookReviews.Count != 0
                        ? book.BookReviews.Average(r => r.Rating)
                        : 0);
                    return model;
                });

            return SearchAndSortBook(books, searchType, sortType)
                .ToList();
        }
        
        private IEnumerable<BookListItemViewModel> SearchAndSortBook(IEnumerable<BookListItemViewModel> books, BookSearchType searchType, BookSortType sortType)
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

            return sortType switch
            {
                BookSortType.GenreAscending => books.OrderBy(b => b.Genres
                    .Select(name => name)
                    .OrderBy(name => name)
                    .FirstOrDefault()),
                BookSortType.GenreDescending => books.OrderBy(b => b.Genres
                    .Select(name => name)
                    .OrderByDescending(name => name)
                    .FirstOrDefault()),
                BookSortType.TitleAscending => books.OrderBy(b => b.Title),
                BookSortType.TitleDescending => books.OrderByDescending(b => b.Title),
                BookSortType.AuthorAscending => books.OrderBy(b => b.Author),
                BookSortType.AuthorDescending => books.OrderByDescending(b => b.Author),
                BookSortType.RatingAscending => books.OrderByDescending(b => b.AverageRating),
                BookSortType.RatingDescending => books.OrderBy(b => b.AverageRating),
                BookSortType.SeriesAscending => books.OrderBy(b => b.SeriesNumber),
                BookSortType.SeriesDescending => books.OrderByDescending(b => b.SeriesNumber),
                BookSortType.CreatedTimeAscending => books.OrderBy(b => b.CreatedTime),
                BookSortType.CreatedTimeDescending => books.OrderByDescending(b => b.CreatedTime),
                _ => books, // Default case
            };
        }
    }
}
