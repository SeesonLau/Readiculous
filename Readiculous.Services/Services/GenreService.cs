using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Readiculous.Data.Interfaces;
using Readiculous.Data.Models;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using X.PagedList;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Services.Services
{
    public class GenreService : IGenreService
    {
        private readonly IGenreRepository _genreRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IMapper _mapper;

        public GenreService(IGenreRepository genreRepository, IBookRepository bookRepository, IReviewRepository reviewRepository, IMapper mapper)
        {
            _genreRepository = genreRepository;
            _bookRepository = bookRepository;
            _reviewRepository = reviewRepository;
            _mapper = mapper;
        }

        // CRUD operations for Genre
        public void AddGenre(GenreViewModel model, string creatorId)
        {
            if (_genreRepository.GenreNameExists(model.Name, model.GenreId))
            {
                throw new DuplicateNameException(Resources.Messages.Errors.GenreExists);
            }

            var genre = new Genre();
            model.GenreId = Guid.NewGuid().ToString();

            _mapper.Map(model, genre);
            genre.CreatedBy = creatorId;
            genre.CreatedTime = DateTime.UtcNow;
            genre.UpdatedBy = creatorId;
            genre.UpdatedTime = DateTime.UtcNow;

            _genreRepository.AddGenre(genre);
        }
        public void UpdateGenre(GenreViewModel model, string updaterId)
        {
            if (_genreRepository.GenreNameExists(model.Name, model.GenreId))
            {
                throw new DuplicateNameException(Resources.Messages.Errors.GenreExists);
            }

            var genre = new Genre();

            _mapper.Map(model, genre);
            genre.UpdatedBy = updaterId;
            genre.UpdatedTime = DateTime.UtcNow;

            _genreRepository.UpdateGenre(genre);
        }
        public void DeleteGenre(string genreId, string deleterId)
        {
            if (!_genreRepository.GenreIdExists(genreId))
            {
                throw new KeyNotFoundException(Resources.Messages.Errors.GenreNotExist);
            }

            var genre = _genreRepository.GetGenreById(genreId);
            genre.DeletedBy = deleterId;
            genre.DeletedTime = DateTime.UtcNow;

            _genreRepository.UpdateGenre(genre);
        }

        // Multiple Genre Listing methods
        public List<GenreListItemViewModel> GetGenreList(string genreName, GenreSortType sortType = GenreSortType.Latest)
        {
            if (string.IsNullOrEmpty(genreName) && sortType == GenreSortType.Latest)
            {
                return ListAllActiveGenres();
            }
            else if(string.IsNullOrEmpty(genreName))
            {
                return ListGenresByName(string.Empty, sortType);
            }
            else
            {
                return ListGenresByName(genreName, sortType);
            }
        }

        public IPagedList<GenreListItemViewModel> GetPaginatedGenreList(string genreName, int pageNumber, int pageSize = 10, GenreSortType sortType = GenreSortType.Latest)
        {

            if (string.IsNullOrEmpty(genreName) && sortType == GenreSortType.Latest)
            {
                return ListAllPaginatedActiveGenres(pageNumber, pageSize);
            }
            else if (string.IsNullOrEmpty(genreName))
            {
                return ListPaginatedGenresByName(string.Empty, pageNumber, pageSize, sortType);
            }
            else
            {
                return ListPaginatedGenresByName(genreName, pageNumber, pageSize, sortType);
            }
        }

        // Single Genre Retrieval methods
        public GenreViewModel GetGenreEditById(string id)
        {
            var genre = _genreRepository.GetGenreById(id);
            if (genre == null || genre.DeletedTime != null)
            {
                throw new KeyNotFoundException(Resources.Messages.Errors.GenreNotExist);
            }

            var model = new GenreViewModel();
            _mapper.Map(genre, model);
            return model;
        }
        public GenreDetailsViewModel GetGenreDetailsById(string id)
        {
            var genre = _genreRepository.GetGenreById(id);
            if (genre == null || genre.DeletedTime != null)
            {
                throw new KeyNotFoundException(Resources.Messages.Errors.GenreNotExist);
            }

            var model = new GenreDetailsViewModel();
            _mapper.Map(genre, model);
            model.BookCount = _bookRepository.GetBookCountByGenreId(genre.GenreId);

            return model;
        }

        // Genre Dropdown Fillup methods
        public List<string> GetSelectedGenreIds(List<GenreViewModel> genreViewModels)
        {
            if (genreViewModels == null || !genreViewModels.Any())
            {
                return new List<string>();
            }

            return genreViewModels.Select(g => g.GenreId).ToList();
        }
        public List<SelectListItem> GetGenreSortTypes(GenreSortType sortType)
        {
            return Enum.GetValues(typeof(GenreSortType))
                .Cast<GenreSortType>()
                .Select(t =>
                {
                    var displayName = t.GetType()
                                     .GetMember(t.ToString())
                                     .First()
                                     .GetCustomAttribute<DisplayAttribute>()?
                                     .Name ?? t.ToString();
                    return new SelectListItem
                    {
                        Value = ((int)t).ToString(),
                        Text = displayName,
                        Selected = t == sortType
                    };
                }).ToList();
        }
  
        public List<SelectListItem> GetAllGenreSelectListItems(string? genreFilter)
        {
            var genres = _genreRepository.GetAllActiveGenres()
                .Where(g => g.DeletedTime == null)
                .OrderBy(g => g.Name)
                .Select(g => new SelectListItem
                {
                    Value = g.GenreId,
                    Text = g.Name,
                    Selected = (genreFilter != null && g.Name.Equals(genreFilter, StringComparison.OrdinalIgnoreCase))
                })
                .ToList();
            return genres;
        }
        
        // Helper methods for Searching genres
        private List<GenreListItemViewModel> ListAllActiveGenres()
        {
            var allGenres = _genreRepository.GetAllActiveGenres();

            var genreIds = allGenres
                .Select(g => g.GenreId)
                .ToList();
            var booksByGenreList = _genreRepository.GetAllGenreAssignmentsByGenreIds(genreIds);

            var genreViewModels = _mapper.Map<List<GenreListItemViewModel>>(allGenres);

            foreach(var model in genreViewModels)
            {
                model.BookCount = booksByGenreList
                    .Where(b => b.GenreId == model.GenreId)
                    .Count();
            }

            return genreViewModels;
        }
        private IPagedList<GenreListItemViewModel> ListAllPaginatedActiveGenres(int pageNumber, int pageSize)
        {
            IQueryable<Genre> queryableGenreListItems;
            int genreCount;

            (queryableGenreListItems, genreCount) = _genreRepository.GetAllPaginatedActiveGenres(pageNumber, pageSize);
            var listGenreListItem = queryableGenreListItems.ToList();
            var genreIds = listGenreListItem.Select(g => g.GenreId).ToList();
            var genres = _genreRepository.GetAllGenreAssignmentsByGenreIds(genreIds);

            var genreMapModels = _mapper.Map<List<GenreListItemViewModel>>(listGenreListItem);

            foreach (var model in genreMapModels)
            {
                model.BookCount = genres
                    .Where(b => b.GenreId == model.GenreId)
                    .Count();
            }

            var result = genreMapModels
                .OrderByDescending(o => o.CreatedTime);
            return new StaticPagedList<GenreListItemViewModel>(
                result.ToList(),
                pageNumber,
                pageSize,
                genreCount);
        }
        private List<GenreListItemViewModel> ListGenresByName(string genreName, GenreSortType genreSortType = GenreSortType.Latest)
        {
            var genresByName = _genreRepository.GetGenresByName(genreName);

            var genreIds = genresByName
                .Select(g => g.GenreId)
                .ToList();
            var booksByGenreList = _genreRepository.GetAllGenreAssignmentsByGenreIds(genreIds);

            var genreViewModels = _mapper.Map<List<GenreListItemViewModel>>(genresByName);

            foreach (var model in genreViewModels)
            {
                model.BookCount = booksByGenreList
                    .Where(b => b.GenreId == model.GenreId)
                    .Count();
            }

            return (genreSortType) switch
            {
                GenreSortType.NameAscending => genreViewModels.OrderBy(g => g.Name).ToList(),
                GenreSortType.NameDescending => genreViewModels.OrderByDescending(g => g.Name).ToList(),
                GenreSortType.BookCountAscending => genreViewModels.OrderBy(g => g.BookCount).ToList(),
                GenreSortType.BookCountDescending => genreViewModels.OrderByDescending(g => g.BookCount).ToList(),
                GenreSortType.Oldest => genreViewModels.OrderBy(g => g.UpdatedTime).ToList(),
                GenreSortType.Latest => genreViewModels.OrderByDescending(g => g.UpdatedTime).ToList(),
                _ => genreViewModels, // Default case
            };
        }
        private IPagedList<GenreListItemViewModel> ListPaginatedGenresByName(string genreName,
            int pageNumber, int pageSize = 10, GenreSortType genreSortType = GenreSortType.Latest)
        {
            IQueryable<Genre> queryableGenreListItems;
            int genreCount;

            (queryableGenreListItems, genreCount) = _genreRepository.GetPaginatedGenresByName(genreName, pageNumber, pageSize, sortType: genreSortType);
            var listGenreListItem = queryableGenreListItems.ToList();
            var genreIds = listGenreListItem.Select(g => g.GenreId).ToList();
            var genres = _genreRepository.GetAllGenreAssignmentsByGenreIds(genreIds);

            var genreMapModels = _mapper.Map<List<GenreListItemViewModel>>(listGenreListItem);

            foreach (var model in genreMapModels)
            {
                model.BookCount = genres
                    .Where(b => b.GenreId == model.GenreId)
                    .Count();
            }

            IEnumerable<GenreListItemViewModel> result;
            switch(genreSortType)
            {
                case GenreSortType.NameAscending:
                    result = genreMapModels.OrderBy(g => g.Name);
                    break;
                case GenreSortType.NameDescending:
                    result = genreMapModels.OrderByDescending(g => g.Name);
                    break;
                case GenreSortType.BookCountAscending:
                    result = genreMapModels.OrderBy(g => g.BookCount);
                    break;
                case GenreSortType.BookCountDescending:
                    result = genreMapModels.OrderByDescending(g => g.BookCount);
                    break;
                case GenreSortType.Oldest:
                    result = genreMapModels.OrderBy(g => g.UpdatedTime);
                    break;
                case GenreSortType.Latest:
                    result = genreMapModels.OrderByDescending(g => g.UpdatedTime);
                    break;
                default:
                    result = genreMapModels;
                    break;
            }

            return new StaticPagedList<GenreListItemViewModel>(
                result.ToList(),
                pageNumber,
                pageSize,
                genreCount);
        }

        public List<GenreViewModel> ConvertGenreListItemViewModelToGenreViewModel(List<GenreListItemViewModel> genreListItemViewModels)
        {
            if (genreListItemViewModels == null || !genreListItemViewModels.Any())
            {
                return new List<GenreViewModel>();
            }
            return genreListItemViewModels.Select(g => new GenreViewModel
            {
                GenreId = g.GenreId,
                Name = g.Name,
            }).ToList();
        }

        public List<BookListItemViewModel> GetBooksByGenreId(string genreId)
        {
            var genre = _genreRepository.GetGenreWithBooksPropertiesById(genreId);
            if (genre == null || genre.DeletedTime != null)
            {
                return new List<BookListItemViewModel>();
            }

            var books = genre.Books
                .Where(bga => bga.Book != null && 
                    bga.Book.DeletedTime == null)
                .Select(bga => bga.Book)
                .ToList();

            var bookMapModels = _mapper.Map<List<BookListItemViewModel>>(books);
            return bookMapModels;
        }
    }
}
