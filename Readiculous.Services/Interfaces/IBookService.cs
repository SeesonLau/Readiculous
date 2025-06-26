using Microsoft.AspNetCore.Mvc.Rendering;
using Readiculous.Services.ServiceModels;
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

        List<BookListItemViewModel> GetBookList(string searchString, List<GenreViewModel> genres, BookSearchType searchType = BookSearchType.AllBooks, BookSortType sortType = BookSortType.CreatedTimeDescending);
        BookDetailsViewModel GetBookDetailsById(string id);
        BookViewModel GetBookEditById(string id);
        List<SelectListItem> GetBookSearchTypes(BookSearchType searchType);
        List<SelectListItem> GetBookSortTypes(BookSortType sortType);
    }
}
