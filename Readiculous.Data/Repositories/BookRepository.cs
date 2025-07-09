using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Readiculous.Data;
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
        private readonly ReadiculousDbContext _context;

        public BookRepository(IUnitOfWork unitOfWork, ReadiculousDbContext context)
            : base(unitOfWork)
        {
            _context = context;
        }

        public bool BookIdExists(string bookId)
        {
            return this.GetDbSet<Book>().Any(b => b.BookId == bookId && b.DeletedTime == null);
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

        public IQueryable<Book> GetAllActiveBooks()
        {
            return this.GetDbSet<Book>()
                .Include(b => b.GenreAssociations).ThenInclude(g => g.Genre)
                .Include(b => b.CreatedByUser)
                .Include(b => b.UpdatedByUser)
                .Where(b => b.DeletedTime == null);
        }

        public IQueryable<Book> GetBooksByTitle(string bookTitle)
        {
            return this.GetDbSet<Book>()
                .Include(b => b.GenreAssociations).ThenInclude(g => g.Genre)
                .Include(b => b.CreatedByUser)
                .Include(b => b.UpdatedByUser)
                .Where(b => b.Title.ToLower().Contains(bookTitle.ToLower()) &&
                            b.DeletedTime == null);
        }

        public IQueryable<Book> GetBooksByGenreList(List<Genre> genres)
        {
            var query = this.GetDbSet<Book>()
                .Include(b => b.GenreAssociations).ThenInclude(g => g.Genre)
                .Include(b => b.CreatedByUser)
                .Include(b => b.UpdatedByUser)
                .Where(b => b.DeletedTime == null);

            if (genres != null && genres.Any())
            {
                query = query.Where(b => b.GenreAssociations
                    .Any(ga => genres.Any(g => g.GenreId == ga.GenreId)));
            }

            return query;
        }

        public IQueryable<Book> GetBooksByTitleAndGenres(string bookTitle, List<Genre> genres)
        {
            var query = this.GetDbSet<Book>()
                .Include(b => b.GenreAssociations).ThenInclude(g => g.Genre)
                .Include(b => b.CreatedByUser)
                .Include(b => b.UpdatedByUser)
                .Where(b => b.DeletedTime == null &&
                            b.Title.ToLower().Contains(bookTitle.ToLower()));

            if (genres != null && genres.Any())
            {
                query = query.Where(b => b.GenreAssociations
                    .Any(ga => genres.Any(g => g.GenreId == ga.GenreId)));
            }

            return query;
        }

        public Book GetBookById(string id)
        {
            return this.GetDbSet<Book>()
                .Include(b => b.GenreAssociations).ThenInclude(bga => bga.Genre)
                .Include(b => b.CreatedByUser)
                .Include(b => b.UpdatedByUser)
                .FirstOrDefault(b => b.BookId == id && b.DeletedTime == null);
        }

        public IEnumerable<Book> GetLatestBooks()
        {
            
            return this.GetDbSet<Book>()
                .Where(b => b.DeletedTime == null)
                .OrderByDescending(b => b.BookId) 
                .Take(10)
                .ToList();
        }

        public IEnumerable<Book> GetTopRatedBooks()
        {
            return this.GetDbSet<Book>()
                .Where(b => b.DeletedTime == null)
                .OrderByDescending(b => b.Rating)
                .Take(10)
                .ToList();
        }
    }
}
