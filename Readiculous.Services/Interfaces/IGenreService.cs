using Microsoft.AspNetCore.Mvc.Rendering;
using Readiculous.Services.ServiceModels;
using System.Collections.Generic;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Services.Interfaces
{
    public interface IGenreService
    {
        void AddGenre(GenreViewModel model, string creatorId);

        void UpdateGenre(GenreViewModel model, string updaterId);

        void DeleteGenre(string genreId, string deleterId);

        List<GenreListItemViewModel> GetGenreList(string genreName, GenreSortType sortType = GenreSortType.CreatedTimeDescending);

        GenreViewModel GetGenreEditById(string id);
        GenreDetailsViewModel GetGenreDetailsById(string id);
        List<string> GetSelectedGenreIds(List<GenreViewModel> genreViewModels);
        List<SelectListItem> GetGenreSortTypes();
        List<GenreViewModel> ConvertGenreListItemViewModelToGenreViewModel(List<GenreListItemViewModel> genreListItemViewModels);
        List<BookViewModel> GetBooksByGenreId(string genreId);
    }
}
