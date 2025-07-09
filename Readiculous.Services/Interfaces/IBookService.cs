using Microsoft.AspNetCore.Mvc.Rendering;
using Readiculous.Data.Models;
using Readiculous.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Services.Interfaces
{
    public interface IBookService
    {
        Task AddBook(BookViewModel model, string creatorId);

        Task UpdateBook(BookViewModel model, string updaterId);

        void DeleteBook(string bookId, string deleterId);

        List<BookListItemViewModel> GetBookList(string searchString, List<GenreViewModel> genres, string userID, BookSearchType searchType = BookSearchType.AllBooks, BookSortType sortType = BookSortType.CreatedTimeDescending);
        BookDetailsViewModel GetBookDetailsById(string id);
        BookViewModel GetBookEditById(string id);
        List<SelectListItem> GetBookSearchTypes(BookSearchType searchType);
        List<SelectListItem> GetBookSortTypes(BookSortType sortType);

        List<BookViewModel> GetNewBooks();
        List<BookViewModel> GetTopBooks();
        List<BookViewModel> GetBooksForGuest(string section);

        IEnumerable<Book> GetTopRatedBooks();
        IEnumerable<Book> GetLatestBooks();
        void AddBookToFavorites(string bookId, string userId);
        void RemoveBookFromFavorites(string bookId, string userId);

      

    }
}
