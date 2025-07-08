using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Readiculous.Data.Interfaces;
using Readiculous.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Data.Repositories
{
    public class GenreRepository : BaseRepository, IGenreRepository
    {
        public GenreRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public bool GenreIdExists(string genreId)
        {
            return this.GetDbSet<Genre>().Any(g => g.GenreId == genreId &&
                                                g.DeletedTime == null);
        }
        public bool GenreNameExists(string genreName)
        {
            return this.GetDbSet<Genre>()
                .Any(g => g.Name == genreName && g.DeletedTime == null);
        }

        public void AddGenre(Genre genre)
        {
            this.GetDbSet<Genre>().Add(genre);
            UnitOfWork.SaveChanges();
        }

        public void UpdateGenre(Genre genre)
        {
            this.GetDbSet<Genre>().Update(genre);
            UnitOfWork.SaveChanges();
        }

        public IQueryable<Genre> GetAllActiveGenres()
        {
            return this.GetDbSet<Genre>()
                .Include(g => g.Books)
                    .ThenInclude(bga => bga.Book)
                .Include(g => g.CreatedByUser)
                .Include(g => g.UpdatedByUser)
                .Where(g => g.DeletedTime == null);
        }
        public IQueryable<Genre> GetGenresByName(string genreName)
        {
            var queryReturn = this.GetDbSet<Genre>()
                .Include(g => g.Books)
                    .ThenInclude(bga => bga.Book)
                .Include(g => g.CreatedByUser)
                .Include(g => g.UpdatedByUser)
                .Where(g => g.Name.ToLower().Contains(genreName.ToLower()) &&
                            g.DeletedTime == null);

            return queryReturn;
        }

        public Genre GetGenreById(string id)
        {
            return this.GetDbSet<Genre>()
                .Include(g => g.Books)
                    .ThenInclude(bga => bga.Book)
                .Include(g => g.CreatedByUser)
                .Include(g => g.UpdatedByUser)
                .FirstOrDefault(g => g.GenreId == id &&
                                    g.DeletedTime == null);
        }

        public Genre GetGenreByName(string name)
        {
            return this.GetDbSet<Genre>()
                .Include(g => g.Books)
                .Include(g => g.CreatedByUser)
                .Include(g => g.UpdatedByUser)
                .FirstOrDefault(g => g.Name == name &&
                                    g.DeletedTime == null);
        }
        public IQueryable<string> GetGenreNamesByBookId(string bookId)
        {
            return this.GetDbSet<BookGenreAssignment>()
                .Where(bga => bga.BookId == bookId &&
                                bga.Genre.DeletedTime == null)
                .Select(bga => bga.Genre.Name);
        }        
    }
}
