using Basecode.Data.Repositories;
using CsvHelper.TypeConversion;
using Microsoft.EntityFrameworkCore;
using Readiculous.Data.Interfaces;
using Readiculous.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public bool BookTitleAndAuthorExists(string bookTitle, string author)
        {
            return this.GetDbSet<Book>().Any(b => b.Title.ToLower() == bookTitle.ToLower() &&
                                                b.Author.ToLower() == author.ToLower() &&
                                                b.DeletedTime == null);
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

        public void DeleteBook(string bookId, string deleterId)
        {
            var book = this.GetDbSet<Book>()
                .FirstOrDefault(b => b.BookId == bookId && 
                                    b.DeletedTime == null);

            if (book != null)
            {
                book.DeletedTime = DateTime.UtcNow;
                book.DeletedBy = deleterId;
                this.UpdateBook(book);
            }
        }

        public IQueryable<Book> GetAllActiveBooks()
        {
            return this.GetDbSet<Book>()
                .Include(b => b.GenreAssociations)
                    .ThenInclude(g => g.Genre)
                .Include(b => b.CreatedByUser)
                .Include(b => b.UpdatedByUser)
                .Where(b => b.DeletedTime == null);
        }
        public IQueryable<Book> GetBooksByTitle(string bookTitle, BookSearchType searchType, BookSortType bookSortType)
        {
            var books = this.GetDbSet<Book>()
                .Include(book => book.GenreAssociations)
                    .ThenInclude(genre => genre.Genre)
                .Include(book => book.CreatedByUser)
                .Include(book => book.UpdatedByUser)
                .Where(b => b.Title.ToLower().Contains(bookTitle.ToLower()) && //Book title search is case-insensitive
                            b.DeletedTime == null);

            switch (searchType)
            {
                case BookSearchType.AllBooks:
                    break;
                case BookSearchType.TopBooks:
                    books = books.OrderByDescending(b => b.BookReviews.Any()
                        ? b.BookReviews.Average(r => r.Rating)
                        : 0);
                    break;
                case BookSearchType.NewBooks:
                    DateTime twoWeeksAgo = DateTime.UtcNow.AddDays(-14);
                    books = books.Where(b => b.CreatedTime >= twoWeeksAgo);
                    return books.OrderByDescending(b => b.CreatedTime);
                default:
                    break;
            }

            return bookSortType switch
            {
                BookSortType.GenreAscending => books.OrderBy(b => b.GenreAssociations
                    .Select(ga => ga.Genre.Name)
                    .OrderBy(name => name)
                    .FirstOrDefault()),
                BookSortType.GenreDescending => books.OrderByDescending(b => b.GenreAssociations
                    .Select(ga => ga.Genre.Name)
                    .OrderBy(name => name)
                    .FirstOrDefault()),
                BookSortType.TitleAscending => books.OrderBy(b => b.Title),
                BookSortType.TitleDescending => books.OrderByDescending(b => b.Title),
                BookSortType.AuthorAscending => books.OrderBy(b => b.Author),
                BookSortType.AuthorDescending => books.OrderByDescending(b => b.Author),
                BookSortType.RatingAscending => books.OrderByDescending(b => b.BookReviews.Any()
                    ? b.BookReviews.Average(r => r.Rating)
                    : 0),
                BookSortType.RatingDescending => books.OrderBy(b => b.BookReviews.Any()
                    ? b.BookReviews.Average(r => r.Rating)
                    : 0),
                BookSortType.SeriesAscending => books.OrderBy(b => b.SeriesNumber),
                BookSortType.SeriesDescending => books.OrderByDescending(b => b.SeriesNumber),
                BookSortType.CreatedTimeAscending => books.OrderBy(b => b.CreatedTime),
                BookSortType.CreatedTimeDescending => books.OrderByDescending(b => b.CreatedTime),
                _ => books, // Default case
            };
        }
        public IQueryable<Book> GetBooksByGenreList(List<Genre> genres, BookSearchType searchType, BookSortType sortType)
        {
            IQueryable<Book> books;
            if (genres == null || !genres.Any())
            {
                books = this.GetDbSet<Book>()
                    .Include(b => b.GenreAssociations)
                        .ThenInclude(g => g.Genre)
                    .Include(b => b.CreatedByUser)
                    .Include(b => b.UpdatedByUser)
                    .Where(b => b.DeletedTime == null);
            }
            else
            {
                books = this.GetDbSet<Book>()
                    .Include(b => b.GenreAssociations)
                        .ThenInclude(g => g.Genre)
                    .Include(b => b.CreatedByUser)
                    .Include(b => b.UpdatedByUser)
                    .Where(b => b.DeletedTime == null &&
                                b.GenreAssociations.Any(ga => genres.Any(g => g.GenreId == ga.GenreId)));
            }

            switch (searchType)
            {
                case BookSearchType.AllBooks:
                    break;

                case BookSearchType.TopBooks:
                    books = books.OrderByDescending(b => b.BookReviews.Any()
                        ? b.BookReviews.Average(r => r.Rating)
                        : 0);

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
                BookSortType.GenreAscending => books.OrderBy(b => b.GenreAssociations
                    .Select(ga => ga.Genre.Name)
                    .OrderBy(name => name)
                    .FirstOrDefault()),
                BookSortType.GenreDescending => books.OrderByDescending(b => b.GenreAssociations
                    .Select(ga => ga.Genre.Name)
                    .OrderBy(name => name)
                    .FirstOrDefault()),
                BookSortType.TitleAscending => books.OrderBy(b => b.Title),
                BookSortType.TitleDescending => books.OrderByDescending(b => b.Title),
                BookSortType.AuthorAscending => books.OrderBy(b => b.Author),
                BookSortType.AuthorDescending => books.OrderByDescending(b => b.Author),
                BookSortType.RatingAscending => books.OrderByDescending(b => b.BookReviews.Any()
                    ? b.BookReviews.Average(r => r.Rating)
                    : 0),
                BookSortType.RatingDescending => books.OrderBy(b => b.BookReviews.Any()
                    ? b.BookReviews.Average(r => r.Rating)
                    : 0),
                BookSortType.SeriesAscending => books.OrderBy(b => b.SeriesNumber),
                BookSortType.SeriesDescending => books.OrderByDescending(b => b.SeriesNumber),
                BookSortType.CreatedTimeAscending => books.OrderBy(b => b.CreatedTime),
                BookSortType.CreatedTimeDescending => books.OrderByDescending(b => b.CreatedTime),
                _ => books,
            };
        }
        public IQueryable<Book> GetBooksByTitleAndGenres(string bookTitle, List<Genre> genres, BookSearchType searchType, BookSortType sortType)
        {

            IQueryable<Book> books;
            if (genres == null || !genres.Any())
            {
                books = this.GetDbSet<Book>()
                    .Include(b => b.GenreAssociations)
                        .ThenInclude(g => g.Genre)
                    .Include(b => b.CreatedByUser)
                    .Include(b => b.UpdatedByUser)
                    .Where(b => b.DeletedTime == null &&
                            b.Title.ToLower().Contains(bookTitle.ToLower()));
            }
            else
            {
                books = this.GetDbSet<Book>()
                    .Include(b => b.GenreAssociations)
                        .ThenInclude(g => g.Genre)
                    .Include(b => b.CreatedByUser)
                    .Include(b => b.UpdatedByUser)
                    .Where(b => b.DeletedTime == null &&
                            b.Title.ToLower().Contains(bookTitle.ToLower()) &&
                                b.GenreAssociations.Any(ga => genres.Any(g => g.GenreId == ga.GenreId)));
            }

            switch (searchType)
            {
                case BookSearchType.AllBooks:
                    break;

                case BookSearchType.TopBooks:
                    books = books.OrderByDescending(b => b.BookReviews.Any()
                        ? b.BookReviews.Average(r => r.Rating)
                        : 0);

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
                BookSortType.GenreAscending => books.OrderBy(b => b.GenreAssociations
                    .Select(ga => ga.Genre.Name)
                    .OrderBy(name => name)
                    .FirstOrDefault()),
                BookSortType.GenreDescending => books.OrderByDescending(b => b.GenreAssociations
                    .Select(ga => ga.Genre.Name)
                    .OrderBy(name => name)
                    .FirstOrDefault()),
                BookSortType.TitleAscending => books.OrderBy(b => b.Title),
                BookSortType.TitleDescending => books.OrderByDescending(b => b.Title),
                BookSortType.AuthorAscending => books.OrderBy(b => b.Author),
                BookSortType.AuthorDescending => books.OrderByDescending(b => b.Author),
                BookSortType.RatingAscending => books.OrderByDescending(b => b.BookReviews.Any()
                    ? b.BookReviews.Average(r => r.Rating)
                    : 0),
                BookSortType.RatingDescending => books.OrderBy(b => b.BookReviews.Any()
                    ? b.BookReviews.Average(r => r.Rating)
                    : 0),
                BookSortType.SeriesAscending => books.OrderBy(b => b.SeriesNumber),
                BookSortType.SeriesDescending => books.OrderByDescending(b => b.SeriesNumber),
                BookSortType.CreatedTimeAscending => books.OrderBy(b => b.CreatedTime),
                BookSortType.CreatedTimeDescending => books.OrderByDescending(b => b.CreatedTime),
                _ => books,
            };
        }
        public Book GetBookById(string id)
        {
            return this.GetDbSet<Book>()
                .Include(book => book.GenreAssociations)
                    .ThenInclude(bga => bga.Genre)
                .Include(book => book.CreatedByUser)
                .Include(book => book.UpdatedByUser)
                .FirstOrDefault(b => b.BookId == id && 
                                    b.DeletedTime == null);
        }
    }
}
