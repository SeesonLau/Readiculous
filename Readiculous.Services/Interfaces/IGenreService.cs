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
        public void AddGenre(GenreViewModel model, string creatorId);

        public void UpdateGenre(GenreViewModel model, string updaterId);

        public void DeleteGenre(string genreId, string deleterId);

        public List<GenreViewModel> SearchGenresByName(string genreName, GenreSortType genreSortType = GenreSortType.CreatedTimeAscending);

        public GenreViewModel GetGenreById(string id);

        public GenreViewModel GetGenreByName(string name);
    }
}
