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
        List<BookListItemViewModel> ListBooksByTitle(string bookTitle, BookSortType bookSortType);
        BookDetailsViewModel GetBookDetailsById(string id);
        BookViewModel GetBookEditById(string id);
    }
}
