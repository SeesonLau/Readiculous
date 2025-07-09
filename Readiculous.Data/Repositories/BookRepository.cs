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
        //Searching
        public IQueryable<Book> GetAllActiveBooks()
        {
            return this.GetDbSet<Book>()
                .Include(b => b.CreatedByUser)
                .Include(b => b.UpdatedByUser)
                .Where(b => b.DeletedTime == null);
        }
        public IQueryable<Book> GetBooksByTitle(string bookTitle)
        {
            var books = this.GetDbSet<Book>()
                .Include(book => book.CreatedByUser)
                .Include(book => book.UpdatedByUser)
                .Where(b => b.Title.ToLower().Contains(bookTitle.ToLower()) && //Book title search is case-insensitive
                            b.DeletedTime == null);

            return books;
        }
        public IQueryable<Book> GetBooksByGenreList(List<Genre> genres)
        {
            IQueryable<Book> books;
            if (genres == null || !genres.Any())
            {
                books = this.GetDbSet<Book>()
                    .Include(b => b.CreatedByUser)
                    .Include(b => b.UpdatedByUser)
                    .Where(b => b.DeletedTime == null);
            }
            else
            {
                books = this.GetDbSet<Book>()
                    .Include(b => b.CreatedByUser)
                    .Include(b => b.UpdatedByUser)
                    .Where(b => b.DeletedTime == null &&
                                b.GenreAssociations.Any(ga => genres.Any(g => g.GenreId == ga.GenreId)));
            }

            return books;
        }
        public IQueryable<Book> GetBooksByTitleAndGenres(string bookTitle, List<Genre> genres)
        {

            IQueryable<Book> books;
            if (genres == null || !genres.Any())
            {
                books = this.GetDbSet<Book>()
                    .Include(b => b.CreatedByUser)
                    .Include(b => b.UpdatedByUser)
                    .Where(b => b.DeletedTime == null &&
                            b.Title.ToLower().Contains(bookTitle.ToLower()));
            }
            else
            {
                books = this.GetDbSet<Book>()
                    .Include(b => b.CreatedByUser)
                    .Include(b => b.UpdatedByUser)
                    .Where(b => b.DeletedTime == null &&
                            b.Title.ToLower().Contains(bookTitle.ToLower()) &&
                                b.GenreAssociations.Any(ga => genres.Any(g => g.GenreId == ga.GenreId)));
            }

            return books;
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
        public int GetBookCountByGenreId(string genreId)
        {
            return this.GetDbSet<BookGenreAssignment>()
                .Count(bga => bga.GenreId == genreId &&
                              bga.Book.DeletedTime == null);
        }
    }
}
