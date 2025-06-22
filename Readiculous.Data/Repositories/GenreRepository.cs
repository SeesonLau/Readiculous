using Basecode.Data.Repositories;
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
            return this.GetDbSet<Genre>().Any(g => g.Name == genreName &&
                                                g.DeletedTime == null);
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

        public void DeleteGenre(string genreId, string deleterId)
        {
            Genre genre = this.GetDbSet<Genre>()
                .FirstOrDefault(g => g.GenreId == genreId &&
                                    g.DeletedTime == null);

            if (genre != null)
            {
                genre.DeletedBy = deleterId;
                genre.DeletedTime = DateTime.UtcNow;
                this.GetDbSet<Genre>().Update(genre);
                UnitOfWork.SaveChanges();
            }
        }

        public IQueryable<Genre> GetGenresByName(string genreName, GenreSortType genreSortType = GenreSortType.CreatedTimeAscending)
        {
            var queryReturn = this.GetDbSet<Genre>()
                .Where(g => g.Name.ToLower().Contains(genreName.ToLower()) &&
                            g.DeletedTime == null)
                .OrderBy(g => g.Name);

            return (genreSortType) switch
            {
                GenreSortType.NameAscending => queryReturn.OrderBy(g => g.Name),
                GenreSortType.NameDescending => queryReturn.OrderByDescending(g => g.Name),
                GenreSortType.BookCountAscending => queryReturn.OrderBy(g => g.Books.Count),
                GenreSortType.BookCountDescending => queryReturn.OrderByDescending(g => g.Books.Count),
                GenreSortType.CreatedTimeAscending => queryReturn.OrderBy(g => g.CreatedTime),
                GenreSortType.CreatedTimeDescending => queryReturn.OrderByDescending(g => g.CreatedTime),
                _ => queryReturn, // Default case
            };
        }

        public Genre GetGenreById(string id)
        {
            return this.GetDbSet<Genre>()
                .FirstOrDefault(g => g.GenreId == id &&
                                    g.DeletedTime == null);
        }

        public Genre GetGenreByName(string name)
        {
            return this.GetDbSet<Genre>()
                .FirstOrDefault(g => g.Name == name &&
                                    g.DeletedTime == null);
        }
    }
}
