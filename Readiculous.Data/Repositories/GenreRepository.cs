using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Readiculous.Data.Interfaces;
using Readiculous.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            var data = this.GetDbSet<Genre>()
                .Any(g => g.DeletedTime == null &&
                          g.GenreId == genreId);

            return data;
        }
        public bool GenreNameExists(string genreName, string genreId)
        {
            var data = this.GetDbSet<Genre>()
                .Any(g => g.DeletedTime == null && 
                          g.GenreId != genreId &&
                          g.Name == genreName);

            return data;
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
            var data = this.GetDbSet<Genre>()
                .Include(g => g.CreatedByUser)
                .Include(g => g.UpdatedByUser)
                .Where(g => g.DeletedTime == null);

            return data;
        }
        public (IQueryable<Genre>, int) GetAllPaginatedActiveGenres(int pageNumber, int pageSize)
        {
            var data = this.GetDbSet<Genre>()
                .Where(g => g.DeletedTime == null);
            var dataCount = data.Count();

            data = data
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(g => g.CreatedByUser)
                .Include(g => g.UpdatedByUser)
                .AsNoTracking();

            return (data, dataCount);
        }
        public IQueryable<Genre> GetGenresByName(string genreName)
        {
            var data = this.GetDbSet<Genre>()
                .Include(g => g.CreatedByUser)
                .Include(g => g.UpdatedByUser)
                .Where(g => g.DeletedTime == null &&
                            g.Name.ToLower().Contains(genreName.ToLower()));

            return data;
        }
        public (IQueryable<Genre>, int) GetPaginatedGenresByName(string genreName, int pageNumber, int pageSize)
        {
            var data = this.GetDbSet<Genre>()
                .Where(g => g.DeletedTime == null &&
                            g.Name.ToLower().Contains(genreName.ToLower()));
            var dataCount = data.Count();

            data = data
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(g => g.CreatedByUser)
                .Include(g => g.UpdatedByUser)
                .AsNoTracking();

            return (data, dataCount);
        }

        public IQueryable<string> GetGenreNamesByBookId(string bookId)
        {
            var data = this.GetDbSet<BookGenreAssignment>()
                .Where(bga => bga.Genre.DeletedTime == null &&
                              bga.BookId == bookId)
                .Select(bga => bga.Genre.Name);

            return data;
        }

        public Genre GetGenreById(string id)
        {
            var data = this.GetDbSet<Genre>()
                .Where(g => g.DeletedTime == null &&
                            g.GenreId == id)
                .Include(g => g.CreatedByUser)
                .Include(g => g.UpdatedByUser)
                .FirstOrDefault();
        
            return data;
        }
        public Genre GetGenreWithBooksPropertiesById(string id)
        {
            var data = this.GetDbSet<Genre>()
                .Where(g => g.DeletedTime == null &&
                            g.GenreId == id)
                .Include(g => g.Books)
                    .ThenInclude(g => g.Book)
                .Include(g => g.CreatedByUser)
                .Include(g => g.UpdatedByUser)
                .FirstOrDefault();

            return data;
        }

        public IQueryable<BookGenreAssignment> GetAllGenreAssignmentsByBookId(List<string> bookIds)
        {
            var data = this.GetDbSet<BookGenreAssignment>()
                .Where(bga => bga.Book.DeletedTime == null &&
                              bga.Genre.DeletedTime == null &&
                              bookIds.Any(a => a.Equals(bga.BookId)));

            return data;
        }

        public IQueryable<BookGenreAssignment> GetAllGenreAssignmentsByGenreIds(List<string> genreIds)
        {
            var data = this.GetDbSet<BookGenreAssignment>()
                .Where(bga => bga.Book.DeletedTime == null &&
                              bga.Genre.DeletedTime == null &&
                              genreIds.Any(a => a.Equals(bga.GenreId)));

            return data;
        }

        public int GetActiveGenreCount()
        {
            return this.GetDbSet<Genre>()
                .Count(g => g.DeletedTime == null);
        }

        public Dictionary<Genre, int> GetMostUsedGenresWithCount(int numberOfGenres)
        {
            return this.GetDbSet<Genre>()
                .Where(g => g.DeletedTime == null)
                .Select(g => new
                {
                    Genre = g,
                    Count = g.Books.Count(b => b.Book.DeletedTime == null)
                })
                .OrderByDescending(g => g.Count)
                .Take(numberOfGenres)
                .ToDictionary(g => g.Genre, g => g.Count);
        }
    }
}
