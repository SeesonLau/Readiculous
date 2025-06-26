using Readiculous.Data.Models;
using Readiculous.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Services.Interfaces
{
    public interface IGenreService
    {
        void AddGenre(GenreViewModel model, string creatorId);

        void UpdateGenre(GenreViewModel model, string updaterId);

        void DeleteGenre(string genreId, string deleterId);

        List<GenreListItemViewModel> GetGenreList(string genreName, GenreSortType sortType = GenreSortType.CreatedTimeDescending);
        List<GenreListItemViewModel> ListAllActiveGenres();
        List<GenreListItemViewModel> ListGenresByName(string genreName, GenreSortType genreSortType = GenreSortType.CreatedTimeDescending);

        GenreViewModel GetGenreEditById(string id);
        GenreDetailsViewModel GetGenreDetailsById(string id);
        List<string> GetSelectedGenreIds(List<GenreViewModel> genreViewModels);
        List<SelectListItem> GetGenreSortTypes();
    }
}
