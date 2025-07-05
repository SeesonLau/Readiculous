using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Readiculous.Data.Interfaces;
using Readiculous.Data.Models;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Services.Services
{
    public class GenreService : IGenreService
    {
        private readonly IGenreRepository _genreRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;

        public GenreService(IGenreRepository genreRepository, IBookRepository bookRepository, IMapper mapper)
        {
            _genreRepository = genreRepository;
            _bookRepository = bookRepository;
            _mapper = mapper;
        }

        // CRUD operations for Genre
        public async Task AddGenre(GenreViewModel model, string creatorId)
        {
            if (_genreRepository.GenreNameExists(model.Name))
            {
                throw new InvalidOperationException(Resources.Messages.Errors.GenreExists);
            }

            var genre = new Genre();
            model.GenreId = Guid.NewGuid().ToString();

            _mapper.Map(model, genre);
            genre.Name = genre.Name.Trim();
            genre.Description = genre.Description.Trim();
            genre.CreatedBy = creatorId;
            genre.CreatedTime = DateTime.UtcNow;
            genre.UpdatedBy = creatorId;
            genre.UpdatedTime = DateTime.UtcNow;

            await Task.Run(() => _genreRepository.AddGenre(genre));
        }
        public async Task UpdateGenre(GenreViewModel model, string updaterId)
        {
            if (!_genreRepository.GenreNameExists(model.Name))
            {
                throw new InvalidOperationException(Resources.Messages.Errors.GenreNotExist);
            }

            var genre = new Genre();

            _mapper.Map(model, genre);
            genre.Name = genre.Name.Trim();
            genre.Description = genre.Description.Trim();
            genre.UpdatedBy = updaterId;
            genre.UpdatedTime = DateTime.UtcNow;

            await Task.Run(() => _genreRepository.UpdateGenre(genre));
        }
        public async Task DeleteGenre(string genreId, string deleterId)
        {
            if (!_genreRepository.GenreIdExists(genreId))
            {
                throw new InvalidOperationException(Resources.Messages.Errors.GenreNotExist);
            }

            var genre = _genreRepository.GetGenreById(genreId);
            genre.DeletedBy = deleterId;
            genre.DeletedTime = DateTime.UtcNow;

            await Task.Run(() => _genreRepository.UpdateGenre(genre));
        }

        // Multiple Genre Listing methods
        public List<GenreListItemViewModel> GetGenreList(string genreName, GenreSortType sortType = GenreSortType.CreatedTimeDescending)
        {
            if (string.IsNullOrEmpty(genreName) && sortType == GenreSortType.CreatedTimeDescending)
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

        // Single Genre Retrieval methods
        public GenreViewModel GetGenreEditById(string id)
        {
            var genre = _genreRepository.GetGenreById(id);
            if (genre == null || genre.DeletedTime != null)
            {
                throw new InvalidOperationException(Resources.Messages.Errors.GenreNotExist);
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
                throw new InvalidOperationException(Resources.Messages.Errors.GenreNotExist);
            }

            var model = new GenreDetailsViewModel();
            _mapper.Map(genre, model);
            model.BookCount = genre.Books.Count(bga => bga.Book.DeletedTime == null);
            model.CreatedByUsername = genre.CreatedByUser.Username;
            model.UpdatedByUsername = genre.UpdatedByUser.Username;

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
        public List<SelectListItem> GetGenreSortTypes()
        {
            return Enum.GetValues(typeof(GenreSortType))
                .Cast<GenreSortType>()
                .Select(t => new SelectListItem
                {
                    Value = ((int)t).ToString(),
                    Text = t.ToString(),
                }).ToList();
        }

        // Helper methods for Searching genres
        private List<GenreListItemViewModel> ListAllActiveGenres()
        {
            var genres = _genreRepository.GetAllActiveGenres()
                .ToList()
                .Select(genre =>
                {
                    GenreListItemViewModel model = new();

                    _mapper.Map(genre, model);
                    model.CreatedByUsername = genre.CreatedByUser != null ? genre.CreatedByUser.Username : string.Empty;
                    model.UpdatedByUsername = genre.UpdatedByUser != null ? genre.UpdatedByUser.Username : string.Empty;
                    model.BookCount = genre.Books != null ? genre.Books.Count(bga => bga.Book != null && bga.Book.DeletedTime == null) : 0;

                    return model;
                })
                .OrderByDescending(g => g.CreatedTime)
                .ToList();

            return genres;
        }
        private List<GenreListItemViewModel> ListGenresByName(string genreName, GenreSortType genreSortType = GenreSortType.CreatedTimeDescending)
        {
            var genres = _genreRepository.GetGenresByName(genreName)
                .Where(g => g.DeletedTime == null)
                .ToList()
                .Select(genre =>
                {
                    GenreListItemViewModel model = new GenreListItemViewModel();
                    _mapper.Map(genre, model);
                    model.BookCount = genre.Books != null ? genre.Books.Count(bga => bga.Book != null && bga.Book.DeletedTime == null) : 0;
                    model.CreatedByUsername = genre.CreatedByUser != null ? genre.CreatedByUser.Username : string.Empty;
                    model.UpdatedByUsername = genre.UpdatedByUser != null ? genre.UpdatedByUser.Username : string.Empty;
                    return model;
                })
                .ToList();

            return (genreSortType) switch
            {
                GenreSortType.NameAscending => genres.OrderBy(g => g.Name).ToList(),
                GenreSortType.NameDescending => genres.OrderByDescending(g => g.Name).ToList(),
                GenreSortType.BookCountAscending => genres.OrderBy(g => g.BookCount).ToList(),
                GenreSortType.BookCountDescending => genres.OrderByDescending(g => g.BookCount).ToList(),
                GenreSortType.CreatedTimeAscending => genres.OrderBy(g => g.CreatedTime).ToList(),
                GenreSortType.CreatedTimeDescending => genres.OrderByDescending(g => g.CreatedTime).ToList(),
                _ => genres, // Default case
            };
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
            var genre = _genreRepository.GetGenreById(genreId);
            if (genre == null || genre.DeletedTime != null)
            {
                return new List<BookListItemViewModel>();
            }
            var books = genre.Books
                .Where(bga => bga.Book != null && bga.Book.DeletedTime == null)
                .Select(bga => bga.Book)
                .ToList();
            return books.Select(book =>
            {
                var model = new BookListItemViewModel();
                _mapper.Map(book, model);
                return model;
            }).ToList();
        }
    }
}
