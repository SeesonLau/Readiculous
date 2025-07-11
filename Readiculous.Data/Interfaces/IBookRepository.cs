using Readiculous.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Data.Interfaces
{
    public interface IBookRepository
    {
        bool BookIdExists(string bookId);
        bool BookTitleAndAuthorExists(string bookTitle, string author, string id);
        bool ISBNExists(string id, string isbn);
        void AddBook(Book book);
        void UpdateBook(Book book);


        IQueryable<Book> GetAllActiveBooks();
        IQueryable<Book> GetBooksByTitle(string bookTitle);
        IQueryable<Book> GetBooksByGenreList(List<Genre> genres);
        IQueryable<Book> GetBooksByTitleAndGenres(string bookTitle, List<Genre> genres);
        Book GetBookById(string id);

        int GetBookCountByGenreId(string genreId);

    }
}
