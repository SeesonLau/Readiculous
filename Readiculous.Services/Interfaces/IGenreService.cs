using Microsoft.AspNetCore.Mvc.Rendering;
using Readiculous.Services.ServiceModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using X.PagedList;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Services.Interfaces
{
    public interface IGenreService
    {
        void AddGenre(GenreViewModel model, string creatorId);

        void UpdateGenre(GenreViewModel model, string updaterId);

        void DeleteGenre(string genreId, string deleterId);


        List<GenreListItemViewModel> GetGenreList(string genreName, GenreSortType sortType = GenreSortType.Latest);

        GenreViewModel GetGenreEditById(string id);
        GenreDetailsViewModel GetGenreDetailsById(string id);
        List<string> GetSelectedGenreIds(List<GenreViewModel> genreViewModels);
        List<SelectListItem> GetGenreSortTypes(GenreSortType sortType);
        List<SelectListItem> GetAllGenreSelectListItems(string? genreFilter);
        List<GenreViewModel> ConvertGenreListItemViewModelToGenreViewModel(List<GenreListItemViewModel> genreListItemViewModels);
        List<BookListItemViewModel> GetBooksByGenreId(string genreId);
        public IPagedList<GenreListItemViewModel> GetPaginatedGenreList(string genreName, int pageNumber, int pageSize = 10, GenreSortType sortType = GenreSortType.Latest);
    }
}
