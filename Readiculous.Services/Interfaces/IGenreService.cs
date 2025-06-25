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
    public interface IGenreService
    {
        void AddGenre(GenreViewModel model, string creatorId);

        void UpdateGenre(GenreViewModel model, string updaterId);

        void DeleteGenre(string genreId, string deleterId);

        List<GenreListItemViewModel> GetAllActiveGenres();
        List<GenreListItemViewModel> SearchGenresByName(string genreName, GenreSortType genreSortType = GenreSortType.CreatedTimeDescending);

        GenreViewModel GetGenreEditById(string id);
        GenreDetailsViewModel GetGenreDetailsById(string id);
    }
}
