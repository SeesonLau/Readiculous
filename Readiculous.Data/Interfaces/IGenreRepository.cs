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
        bool GenreIdExists(string genreId);
        bool GenreNameExists(string genreName);
        void AddGenre(Genre genre);
        void UpdateGenre(Genre genre);
        void DeleteGenre(string genreId, string deleterId);

        IQueryable<Genre> GetAllActiveGenres();
        IQueryable<Genre> GetGenresByName(string genreName, GenreSortType genreSortType = GenreSortType.CreatedTimeAscending);
        Genre GetGenreById(string id);
        Genre GetGenreByName(string name);
    }
}
