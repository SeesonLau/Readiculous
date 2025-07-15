using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Readiculous.Data.Interfaces;
using Readiculous.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Readiculous.Data.Repositories
{
    public class FavoriteBookRepository : BaseRepository, IFavoriteBookRepository
    {
        public FavoriteBookRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
        
        public bool FavoriteBookExists(string bookId, string userId)
        {
            return this.GetDbSet<FavoriteBook>().Any(fb => fb.UserId == userId 
                                                        && fb.BookId == bookId );
        }

        public void AddFavoriteBook(FavoriteBook favoriteBook)
        {
            this.GetDbSet<FavoriteBook>().Add(favoriteBook);
            this.UnitOfWork.SaveChanges();
        }
        public void RemoveFavoriteBook(FavoriteBook favoriteBook)
        {
            this.GetDbSet<FavoriteBook>().Remove(favoriteBook);
            this.UnitOfWork.SaveChanges();
        }

        public IQueryable<FavoriteBook> GetFavoriteBooksByUserId(string userId)
        {
            return this.GetDbSet<FavoriteBook>()
                .Where(fb => fb.UserId == userId)
                .Include(fb => fb.Book);
        }
        public IQueryable<FavoriteBook> GetFavoriteBooksByBookId(string bookId)
        {
            return this.GetDbSet<FavoriteBook>()
                .Where(fb => fb.BookId == bookId)
                .Include(fb => fb.Book);
        }
        public (IQueryable<FavoriteBook>, int) GetPaginatedFavoriteBooksByUserId(string userId, int pageNumber, int pageSize)
        {
            var data = this.GetDbSet<FavoriteBook>()
                .Where(FavoriteBook => FavoriteBook.UserId == userId);
            var dataCount = data.Count();

            data = data
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(fb => fb.Book)
                .OrderByDescending(fb => fb.CreatedTime)
                .AsNoTracking();

            return (data, dataCount);
        }
        public FavoriteBook GetFavoriteBookByBookIdAndUserId(string bookId, string userId)
        {
            return this.GetDbSet<FavoriteBook>()
                .FirstOrDefault(fb => fb.BookId == bookId 
                                    && fb.UserId == userId);
        }
    }
}
