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
        bool BookTitleAndAuthorExists(string bookTitle, string author);
        void AddBook(Book book);
        void UpdateBook(Book book);
        void DeleteBook(string bookId, string deleterId);

        IQueryable<Book> GetAllActiveBooks();
        IQueryable<Book> GetBooksByTitle(string bookTitle, BookSortType bookSortType = BookSortType.CreatedTimeAscending);
        Book GetBookById(string id);
        Book GetBookByTitle(string title);
        Book GetBookByAuthor(string authorName);
        IQueryable<Book> GetBooksByGenre(string genreId, BookSortType bookSortType = BookSortType.CreatedTimeAscending);
    }
}
