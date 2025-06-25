using AutoMapper;
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

        // BookEditViewModel: Could use create a create model, depends on the requirements
        public async Task AddBook(BookViewModel model, string creatorId)
        {
            // Books can only be added if the title and author combination does not already exist.
            if (_bookRepository.BookTitleAndAuthorExists(model.Title.Trim(), model.Author.Trim()))
            {
                throw new InvalidOperationException(Resources.Messages.Errors.BookTitleAndAuthorExists);
            }
            if(string.IsNullOrEmpty(model.CoverImageUrl))
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
            if (!_bookRepository.BookTitleAndAuthorExists(model.Title, model.Author))
            {
                throw new InvalidOperationException(Resources.Messages.Errors.BookTitleAndAuthorExists);
            }

            var book = _bookRepository.GetBookById(model.BookId);
            if (book == null)
            {
                throw new InvalidOperationException("Book does not exist.");
            }
            if(string.IsNullOrEmpty(model.CoverImageUrl))
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

            _bookRepository.DeleteBook(bookId, deleterId);
        }

        //List All Active Books
        public List<BookListItemViewModel> ListAllActiveBooks()
        {
            var books = _bookRepository.GetAllActiveBooks()
                .ToList()
                .Select(book =>
                {
                    var model = new BookListItemViewModel();
                    _mapper.Map(book, model);
                    return model;
                })
                .ToList();
            return books;
        }

        public List<BookListItemViewModel> ListBooksByTitle(string bookTitle, BookSortType bookSortType = BookSortType.CreatedTimeAscending)
        {
            var books = _bookRepository.GetBooksByTitle(bookTitle.Trim(), bookSortType)
                .Where(b => b.DeletedTime == null)
                .ToList()
                .Select(book =>
                {
                    var model = new BookListItemViewModel();
                    _mapper.Map(book, model);
                    return model;
                })
                .ToList();

            return books;
        }

        //BookEditViewModel or BookDetailsViewModel, depending on the source
        //Create a mapping for EditViewModel in AutoMapper configuration
        
        public BookDetailsViewModel GetBookDetailsById(string id)
        {
            var book = _bookRepository.GetBookById(id);
            if (book == null)
            {
                return null;
            }

            var model = new BookDetailsViewModel();
            _mapper.Map(book, model);
            model.Genres = book.GenreAssociations
                .Where(ga => ga.Genre.DeletedTime == null)
                .Select(ga => ga.Genre.Name)
                .ToList();

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

        /*
        public BookEditViewModel GetBookEditById(string id)
        {
            var book = _bookRepository.GetBookById(id);
            if (book == null)
            {
                return null;
            }

            var model = new BookEditViewModel();
            _mapper.Map(book, model);
            model.SelectedGenres = book.GenreAssociations.Select(ga => ga.GenreId).ToList();
            model.AvailableGenres = _genreRepository.GetAllActiveGenres()
                .Where(g => g.DeletedTime == null)
                .Select(g => new GenreViewModel
                {
                    GenreId = g.GenreId,
                    Name = g.Name
                }).ToList();

            return model;
        }
        */
    }
}
