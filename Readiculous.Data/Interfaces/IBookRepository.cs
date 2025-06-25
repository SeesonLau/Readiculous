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
        bool ISBNExists(string id, string isbn);
        void AddBook(Book book);
        void UpdateBook(Book book);
        void DeleteBook(string bookId, string deleterId);

        IQueryable<Book> GetAllActiveBooks();
        IQueryable<Book> GetBooksByTitle(string bookTitle, BookSearchType searchType, BookSortType bookSortType);
        IQueryable<Book> GetBooksByGenreList(List<Genre> genres, BookSearchType searchType, BookSortType sortType);
        IQueryable<Book> GetBooksByTitleAndGenres(string bookTitle, List<Genre> genres, BookSearchType searchType, BookSortType sortType);
        Book GetBookById(string id);

        // Book GetBookByTitle(string title); //NOT USED AT THE MOMENT
        // Book GetBookByAuthor(string authorName); //NOT USED AT THE MOMENT
    }
}
