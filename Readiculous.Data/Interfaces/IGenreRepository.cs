using Readiculous.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Data.Interfaces
{
    public interface IGenreRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="genreId"></param>
        /// <returns></returns>
        bool GenreIdExists(string genreId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="genreName"></param>
        /// <param name="genreId"></param>
        /// <returns></returns>
        bool GenreNameExists(string genreName, string genreId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="genre"></param>
        void AddGenre(Genre genre);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="genre"></param>
        void UpdateGenre(Genre genre);

        IQueryable<Genre> GetAllActiveGenres();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        (IQueryable<Genre>, int) GetAllPaginatedActiveGenres(int pageNumber, int pageSize, GenreSortType sortType);
        IQueryable<Genre> GetGenresByName(string genreName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="genreName"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        (IQueryable<Genre>, int) GetPaginatedGenresByName(string genreName, int pageNumber, int pageSize, GenreSortType sortType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>
        IQueryable<string> GetGenreNamesByBookId(string bookId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Genre GetGenreById(string id);
        Genre GetGenreWithBooksPropertiesById(string id);
        IQueryable<BookGenreAssignment> GetAllGenreAssignmentsByBookIds(List<string> bookIds);
        IQueryable<BookGenreAssignment> GetAllGenreAssignmentsByGenreIds(List<string> genreIds);

        int GetActiveGenreCount();
        Dictionary<Genre, int> GetMostUsedGenresWithCount(int numberOfGenres);
        List<string> GetTopGenresFromBookIds(List<string> bookIds);
    }
}
