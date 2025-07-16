using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Readiculous.Data.Interfaces;
using Readiculous.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Data.Repositories
{
    public class BookRepository : BaseRepository, IBookRepository
    {
        public BookRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public bool BookIdExists(string bookId) 
        {
            return this.GetDbSet<Book>().Any(b => b.BookId == bookId &&
                                                  b.DeletedTime == null);
        }
        public bool BookTitleAndAuthorExists(string bookTitle, string author, string id)
        {
            return this.GetDbSet<Book>().Any(b => b.DeletedTime == null &&
                                                  b.Author.ToLower() == author.ToLower() &&
                                                  b.BookId != id &&
                                                  b.Title.ToLower() == bookTitle.ToLower());
        }

       
        public bool ISBNExists(string id, string isbn)
        {
            return this.GetDbSet<Book>().Any(b => b.ISBN == isbn &&
                                                  b.BookId != id && 
                                                  b.DeletedTime == null);
        }

        public void AddBook(Book book)
        {
            this.GetDbSet<Book>().Add(book);
            this.UnitOfWork.SaveChanges();
        }

        public void UpdateBook(Book book)
        {
            this.GetDbSet<Book>().Update(book);
            this.UnitOfWork.SaveChanges();
        }

        public IQueryable<Book> GetAllActiveBooks()
        {
            return this.GetDbSet<Book>()
                .AsNoTracking()
                .Where(b => b.DeletedTime == null)
                .Include(b => b.CreatedByUser)
                .Include(b => b.UpdatedByUser);
        }

       public (IQueryable<Book>, int) GetAllPaginatedBooks(int pageNumber, int pageSize = 10)
        {
            var data = this.GetDbSet<Book>()
                .Where(b => b.DeletedTime == null);
            var dataCount = data.Count();
            data = data
                .Skip((pageNumber - 1) * 10)
                .Take(pageSize)
                .Include(b => b.CreatedByUser)
                .Include(b => b.UpdatedByUser)
                .AsNoTracking(); 

            return (data, dataCount);
        }
        public IQueryable<Book> GetBooksByTitle(string bookTitle)
        {
            var books = this.GetDbSet<Book>()
                .AsNoTracking()
                .Include(book => book.CreatedByUser)
                .Include(book => book.UpdatedByUser)
                .Where(b => b.DeletedTime == null && //Book title search is case-insensitive
                            b.Title.ToLower().Contains(bookTitle.ToLower()));

            return books;
        }
        public (IQueryable<Book>, int) GetPaginatedBooksByTitle(string bookTitle, int pageNumber, int pageSize, BookSortType sortType)
        {

            var data = this.GetDbSet<Book>()
                .Where(b => b.DeletedTime == null &&
                            b.Title.ToLower().Contains(bookTitle.ToLower()));
            var dataCount = data.Count();

            data = SortBooks(data, sortType)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(book => book.CreatedByUser)
                .Include(book => book.UpdatedByUser)
                .AsNoTracking();
            return (data, dataCount);
        }
        public IQueryable<Book> GetBooksByGenreList(List<Genre> genres)
        {
            IQueryable<Book> books;
            if (genres == null || !genres.Any())
            {
                books = GetAllActiveBooks();
            }
            else
            {
                books = this.GetDbSet<Book>()
                    .Where(b => b.DeletedTime == null &&
                                b.GenreAssociations
                                    .Any(ga => genres
                                        .Any(g => g.GenreId == ga.GenreId)))

                    .Include(b => b.CreatedByUser)
                    .Include(b => b.UpdatedByUser);
            }

            return books;
        }
        public (IQueryable<Book>, int) GetPaginatedBooksByGenreList(List<Genre> genres, int pageNumber, int pageSize, BookSortType sortType)
        {
            IQueryable<Book> data;
            if(genres == null || !genres.Any())
            {
                data = GetAllActiveBooks();
            }
            else
            {
                data = this.GetDbSet<Book>()
                    .Where(b => b.DeletedTime == null &&
                                b.GenreAssociations
                                    .Any(ga => genres
                                        .Any(g => g.GenreId == ga.GenreId)));
            }

            var dataCount = data.Count();

            data = SortBooks(data, sortType)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(b => b.CreatedByUser)
                .Include(b => b.UpdatedByUser)
                .AsNoTracking();

            return (data, dataCount);
        }

        public IQueryable<Book> GetBooksByTitleAndGenres(string bookTitle, List<Genre> genres)
        {

            IQueryable<Book> books;
            if (genres == null || !genres.Any())
            {
                books = this.GetDbSet<Book>()
                    .AsNoTracking()
                    .Include(b => b.CreatedByUser)
                    .Include(b => b.UpdatedByUser)
                    .Where(b => b.DeletedTime == null &&
                                b.Title.ToLower().Contains(bookTitle.ToLower()));
            }
            else
            {
                books = this.GetDbSet<Book>()
                    .AsNoTracking()
                    .Include(b => b.CreatedByUser)
                    .Include(b => b.UpdatedByUser)
                    .Where(b => b.DeletedTime == null &&
                                b.Title.ToLower().Contains(bookTitle.ToLower()) &&
                                b.GenreAssociations.Any(ga => genres.Any(g => g.GenreId == ga.GenreId)));
            }

            return books;
        }
        public (IQueryable<Book>, int) GetPaginatedBooksByTitleAndGenres(string bookTitle, List<Genre> genres, int pageNumber, int pageSize, BookSortType sortType)
        {
            IQueryable<Book> data;
            int dataCount;
            if(genres == null || !genres.Any())
            {
                data = this.GetDbSet<Book>()
                    .Where(b => b.DeletedTime == null &&
                                b.Title.ToLower().Contains(bookTitle.ToLower()));
                dataCount = data.Count();
                data = data
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageNumber)
                    .Include(b => b.CreatedByUser)
                    .Include(b => b.UpdatedByUser)
                    .AsNoTracking();
            }
            else
            {
                data = this.GetDbSet<Book>()
                    .Where(b => b.DeletedTime == null &&
                                b.Title.ToLower().Contains(bookTitle.ToLower()) &&
                                b.GenreAssociations
                                    .Any(ga => genres
                                        .Any(g => g.GenreId == ga.GenreId)));
                dataCount = data.Count();
                data = SortBooks(data, sortType)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Include(b => b.CreatedByUser)
                    .Include(b => b.UpdatedByUser)
                    .AsNoTracking();
            }

            return (data, dataCount);
        }

        public (IQueryable<Book>, int) GetPaginatedNewBooks(int pageNumber, int pageSize)
        {
            var twoWeeksAgo = DateTime.UtcNow.AddDays(-14);
            var data = this.GetDbSet<Book>()
                .Where(b => b.DeletedTime == null &&
                            b.CreatedTime >= twoWeeksAgo);
            var dataCount = data.Count();
            data = data
                .Skip((pageNumber - 1) + pageSize)
                .Take(pageSize)
                .Include(b => b.BookReviews)
                .Include(b => b.CreatedByUser)
                .Include(b => b.UpdatedByUser)
                .AsNoTracking();

            return (data, dataCount);
        }

        public (IQueryable<Book>, int) GetPaginatedTopBooks(int pageNumber, int pageSize)
        {
            var booksWithAverageRatings = this.GetDbSet<Book>()
                .Where(b => b.DeletedTime == null)
                .Select(b => new
                {
                    Book = b,
                    AverageRating = b.BookReviews
                        .Where(r => r.DeletedTime == null)
                        .Select(r => (double?)r.Rating)
                        .DefaultIfEmpty() 
                        .Average()
                })
                .OrderByDescending(x => x.AverageRating);

            var dataCount = booksWithAverageRatings.Count();

            var data = booksWithAverageRatings
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(b => b.Book)
                .Include(b => b.BookReviews)
                .Include(b => b.CreatedByUser)
                .Include(b => b.UpdatedByUser)
                .AsNoTracking();

            return (data, dataCount);
        }

        public Book GetBookById(string id)
        {
            return this.GetDbSet<Book>()
                .Where(b => b.DeletedTime == null &&
                            b.BookId == id)
                .Include(book => book.GenreAssociations)
                    .ThenInclude(bga => bga.Genre)
                .Include(book => book.CreatedByUser)
                .Include(book => book.UpdatedByUser)
                .FirstOrDefault();
        }

        public int GetBookCountByGenreId(string genreId)
        {
            return this.GetDbSet<BookGenreAssignment>()
                .Count(bga => bga.Book.DeletedTime == null &&
                              bga.GenreId == genreId);
        }

        public int GetActiveBookCount()
        {
            return this.GetDbSet<Book>()
                .Count(b => b.DeletedTime == null);
        }

        private IQueryable<Book> SortBooks(IQueryable<Book> books, BookSortType sortType)
        {
            DateTime twoWeeksAgo = DateTime.UtcNow.AddDays(-14);
            return sortType switch
            {

                BookSortType.TitleAscending => books.OrderBy(b => b.Title),
                BookSortType.TitleDescending => books.OrderByDescending(b => b.Title),
                BookSortType.AuthorAscending => books.OrderBy(b => b.Author),
                BookSortType.AuthorDescending => books.OrderByDescending(b => b.Author),
                BookSortType.RatingAscending => books.OrderBy(b => b.BookReviews.Any() ? b.BookReviews.Average(r => r.Rating) : 0),
                BookSortType.RatingDescending => books.OrderByDescending(b => b.BookReviews.Any() ? b.BookReviews.Average(r => r.Rating) : 0),
                BookSortType.Oldest => books.OrderBy(b => b.UpdatedTime),
                BookSortType.Latest => books.OrderByDescending(b => b.UpdatedTime),
                BookSortType.NewBooksAscending => books
                    .Where(b => b.CreatedTime >= twoWeeksAgo)
                    .OrderBy(b => b.CreatedTime),
                BookSortType.NewBooksDescending => books
                    .Where(b => b.CreatedTime >= twoWeeksAgo)
                    .OrderByDescending(b => b.CreatedTime),
                _ => books, // Default case

            };
        }
    }
}
