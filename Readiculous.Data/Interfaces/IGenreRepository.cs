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
        bool GenreNameExists(string genreName, string genreId);
        void AddGenre(Genre genre);
        void UpdateGenre(Genre genre);

        IQueryable<Genre> GetAllActiveGenres();
        IQueryable<Genre> GetGenresByName(string genreName);
        IQueryable<string> GetGenreNamesByBookId(string bookId);
        Genre GetGenreById(string id);
        Genre GetGenreWithBooksPropertiesById(string id);
    }
}
