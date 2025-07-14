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
        /// <summary>
        /// Checks if a Book ID exists in the database
        /// </summary>
        /// <param name="bookId"> The Book ID being checked</param>
        /// <returns
        /// <c>true</c> if a book with the specified ID exists and is not soft-deleted; otherwise, <c>false</c>.
        /// </returns>
        bool BookIdExists(string bookId);

        /// <summary>
        /// Checks if a book with a given title and author exists in the database ensuring only one book with the title and author exists
        /// </summary>
        /// <param name="bookTitle">The Book Title being checked</param>
        /// <param name="author">The Author being checked</param>
        /// <param name="id">The ID of the book with the book title and author</param>
        bool BookTitleAndAuthorExists(string bookTitle, string author, string id);

        /// <summary>
        /// Checks if an ISBN exists in the database ensuring that only one book with the ISB exists
        /// </summary>
        /// <param name="id">The ID of the book with the ISBN</param>
        /// <param name="isbn">The ISBN being checked</param>
        bool ISBNExists(string id, string isbn);

        /// <summary>
        /// Adds a book in the database
        /// </summary>
        /// <param name="book">The Book entity to be added</param>
        void AddBook(Book book);

        /// <summary>
        /// Updates a book in the database
        /// </summary>
        /// <param name="book">The Book entity to be updated</param>
        void UpdateBook(Book book);

        // Book Retrieval Methods
        IQueryable<Book> GetAllActiveBooks(); // TO BE DELETED

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        (IQueryable<Book>, int) GetAllPaginatedBooks(int pageNumber, int pageSize = 10);
        IQueryable<Book> GetBooksByTitle(string bookTitle); // TO BE DELETED

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bookTitle"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        (IQueryable<Book>, int) GetPaginatedBooksByTitle(string bookTitle, int pageNumber, int pageSize = 10);
        IQueryable<Book> GetBooksByGenreList(List<Genre> genres); // TO BE DELETED

        /// <summary>
        /// 
        /// </summary>
        /// <param name="genres"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        (IQueryable<Book>, int) GetPaginatedBooksByGenreList(List<Genre> genres, int pageNumber, int pageSize = 10);
        IQueryable<Book> GetBooksByTitleAndGenres(string bookTitle, List<Genre> genres); // TO BE DELETED

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bookTitle"></param>
        /// <param name="genres"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        (IQueryable<Book>, int) GetPaginatedBooksByTitleAndGenres(string bookTitle, List<Genre> genres, int pageNumber, int pageSize = 10);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Book GetBookById(string id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="genreId"></param>
        /// <returns></returns>
        int GetBookCountByGenreId(string genreId);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetActiveBookCount();
    }
}
