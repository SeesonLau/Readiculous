using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Readiculous.Data.Interfaces;
using Readiculous.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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
                .Include(b => b.CreatedByUser)
                .Include(b => b.UpdatedByUser)
                .Where(b => b.DeletedTime == null);
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
        public Book GetBookById(string id)
        {
            return this.GetDbSet<Book>()
                .Include(book => book.GenreAssociations)
                    .ThenInclude(bga => bga.Genre)
                .Include(book => book.CreatedByUser)
                .Include(book => book.UpdatedByUser)
                .FirstOrDefault(b => b.DeletedTime == null &&
                                     b.BookId == id);
        }
        public int GetBookCountByGenreId(string genreId)
        {
            return this.GetDbSet<BookGenreAssignment>()
                .Count(bga => bga.Book.DeletedTime == null &&
                              bga.GenreId == genreId);
        }
    }
}
