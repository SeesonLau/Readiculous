using Basecode.Data.Repositories;
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
                                                        && fb.BookId == bookId 
                                                        && fb.DeletedTime == null);
        }

        public void AddFavoriteBook(FavoriteBook favoriteBook)
        {
            this.GetDbSet<FavoriteBook>().Add(favoriteBook);
            this.UnitOfWork.SaveChanges();
        }
        public void RemoveFavoriteBook(FavoriteBook favoriteBook)
        {
            this.GetDbSet<FavoriteBook>().Update(favoriteBook);
            this.UnitOfWork.SaveChanges();
        }

        public IQueryable<FavoriteBook> GetFavoriteBooksByUserId(string userId)
        {
            return this.GetDbSet<FavoriteBook>()
                .Where(fb => fb.UserId == userId 
                            && fb.DeletedTime == null);
        }
        public IQueryable<FavoriteBook> GetFavoriteBooksByBookId(string bookId)
        {
            return this.GetDbSet<FavoriteBook>()
                .Where(fb => fb.BookId == bookId 
                            && fb.DeletedTime == null);
        }
        public FavoriteBook GetFavoriteBookByBookIdAndUserId(string bookId, string userId)
        {
            return this.GetDbSet<FavoriteBook>()
                .FirstOrDefault(fb => fb.BookId == bookId 
                                    && fb.UserId == userId 
                                    && fb.DeletedTime == null);
        }
    }
}
