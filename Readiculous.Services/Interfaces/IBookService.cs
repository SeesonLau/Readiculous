using Readiculous.Data.Models;
using Readiculous.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Services.Interfaces
{
    public interface IBookService
    {
        Task AddBook(BookViewModel model, string creatorId);

        Task UpdateBook(BookViewModel model, string updaterId);

        void DeleteBook(string bookId, string deleterId);

        List<BookListItemViewModel> ListAllActiveBooks();
        List<BookListItemViewModel> ListBooksByTitle(string bookTitle, BookSearchType searchType = BookSearchType.AllBooks, BookSortType sortType = BookSortType.CreatedTimeDescending);
        List<BookListItemViewModel> ListBooksByGenreList(List<GenreViewModel> genreViewModels, BookSearchType searchType = BookSearchType.AllBooks, BookSortType sortType = BookSortType.CreatedTimeAscending);
        List<BookListItemViewModel> ListBooksByTitleAndGenres(string bookTitle, List<GenreViewModel> genreViewModels, BookSearchType searchType = BookSearchType.AllBooks, BookSortType sortType = BookSortType.CreatedTimeAscending);
        BookDetailsViewModel GetBookDetailsById(string id);
        BookViewModel GetBookEditById(string id);
    }
}
