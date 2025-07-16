using Microsoft.AspNetCore.Mvc.Rendering;
using Readiculous.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using X.PagedList;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Services.Interfaces
{
    public interface IBookService
    {
        Task AddBook(BookViewModel model, string creatorId);

        Task UpdateBook(BookViewModel model, string updaterId);

        Task DeleteBook(string bookId, string deleterId);

        List<BookListItemViewModel> GetBookList(string searchString, List<GenreViewModel> genres, string userID, BookSortType sortType = BookSortType.Latest, string? genreFilter = null);
        IPagedList<BookListItemViewModel> GetPaginatedBookList(string searchString, List<GenreViewModel> genres, string userId, int pageNumber, int pageSize = 10, BookSortType sortType = BookSortType.Latest, string? genreFilter = null);
        BookDetailsViewModel GetBookDetailsById(string id);
        BookViewModel GetBookEditById(string id);
        UserDashboardViewModel GetDashboardViewModel();
        List<SelectListItem> GetBookSortTypes(BookSortType sortType);

        void AddBookToFavorites(string bookId, string userId);
        void RemoveBookFromFavorites(string bookId, string userId);

        string GetTitleByBookId(string bookId);
    }
}
