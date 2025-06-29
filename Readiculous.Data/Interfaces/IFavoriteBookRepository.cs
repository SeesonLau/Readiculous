using Readiculous.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Readiculous.Data.Interfaces
{
    public interface IFavoriteBookRepository
    {
        bool FavoriteBookExists(string bookId, string userId);

        void AddFavoriteBook(FavoriteBook favoriteBook);
        void RemoveFavoriteBook(FavoriteBook favoriteBook);

        IQueryable<FavoriteBook> GetFavoriteBooksByUserId(string userId);
        IQueryable<FavoriteBook> GetFavoriteBooksByBookId(string bookId);
        FavoriteBook GetFavoriteBookByBookIdAndUserId(string bookId, string userId);
    }
}
