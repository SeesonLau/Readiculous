using Basecode.Data.Repositories;
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
            var book = this.GetDbSet<Book>().FirstOrDefault(b => b.BookId == bookId && b.DeletedTime == null);
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
                .Include(book => book.GenreAssociations)
                    .ThenInclude(genre => genre.Genre)
                .AsNoTracking()
                .Where(b => b.DeletedTime == null);
        }
        public IQueryable<Book> GetBooksByTitle(string bookTitle, BookSortType bookSortType )
        {

            var books = this.GetDbSet<Book>()
                .Include(book => book.GenreAssociations)
                    .ThenInclude(genre => genre.Genre)
                .AsNoTracking()
                .Where(b => b.Title.ToLower().Contains(bookTitle.ToLower()) && //Book title search is case-insensitive
                            b.DeletedTime == null);

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
                //BookSortType.RatingAscending => books.OrderBy(b => b.Rating),
                //BookSortType.RatingDescending => books.OrderByDescending(b => b.Rating),
                BookSortType.SeriesAscending => books.OrderBy(b => b.SeriesNumber),
                BookSortType.SeriesDescending => books.OrderByDescending(b => b.SeriesNumber),
                BookSortType.CreatedTimeAscending => books,
                BookSortType.CreatedTimeDescending => books.Reverse(),
                _ => books, // Default case
            };
        }

        public Book GetBookById(string id)
        {
            return this.GetDbSet<Book>()
                .Include(book => book.GenreAssociations)
                .FirstOrDefault(b => b.BookId == id && 
                                    b.DeletedTime == null);
        }

        public Book GetBookByTitle(string title)
        {
            return this.GetDbSet<Book>()
                .Include(book => book.GenreAssociations)
                .FirstOrDefault(b => b.Title == title && 
                                    b.DeletedTime == null);
        }

        public Book GetBookByAuthor(string authorName)
        {
            return this.GetDbSet<Book>()
                .Include(book => book.GenreAssociations)
                .FirstOrDefault(b => b.Author == authorName && 
                                    b.DeletedTime == null);
        }

        public IQueryable<Book> GetBooksByGenre(string genreId, BookSortType bookSortType = BookSortType.CreatedTimeAscending)
        {
            var books = this.GetDbSet<Book>()
                .Where(b => b.DeletedBy == null && 
                            b.GenreAssociations.Any(g => g.GenreId == genreId));

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
                BookSortType.SeriesAscending => books.OrderBy(b => b.SeriesNumber),
                BookSortType.SeriesDescending => books.OrderByDescending(b => b.SeriesNumber),
                BookSortType.CreatedTimeAscending => books.OrderBy(b => b.CreatedTime),
                BookSortType.CreatedTimeDescending => books.OrderByDescending(b => b.CreatedTime),
                _ => books,
            };
        }
    }
}
