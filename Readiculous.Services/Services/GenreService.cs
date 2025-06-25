using AutoMapper;
using Readiculous.Data.Interfaces;
using Readiculous.Data.Models;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Text;
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


        public void AddGenre(GenreViewModel model, string creatorId)
        {
            if(!_genreRepository.GenreNameExists(model.Name))
            {
                var genre = new Genre();
                model.GenreId = Guid.NewGuid().ToString();

                _mapper.Map(model, genre);
                genre.Name = genre.Name.Trim();
                genre.Description = genre.Description.Trim();
                genre.CreatedBy = creatorId;
                genre.CreatedTime = DateTime.UtcNow;
                genre.UpdatedBy = creatorId;
                genre.UpdatedTime = DateTime.UtcNow;

                _genreRepository.AddGenre(genre);
            }
            else
            {
                throw new InvalidOperationException(Resources.Messages.Errors.GenreExists);
            }
        }

        public void UpdateGenre(GenreViewModel model, string updaterId)
        {
            if (_genreRepository.GenreNameExists(model.Name))
            {
                var genre = new Genre();
                _mapper.Map(model, genre);
                genre.Name = genre.Name.Trim();
                genre.Description = genre.Description.Trim();
                genre.UpdatedBy = updaterId;
                genre.UpdatedTime = DateTime.UtcNow;
                _genreRepository.UpdateGenre(genre);
            }
            else
            {
                throw new InvalidOperationException(Resources.Messages.Errors.GenreNotExist);
            }
        }

        public void DeleteGenre(string genreId, string deleterId)
        {
            if (_genreRepository.GenreIdExists(genreId))
            {
                _genreRepository.DeleteGenre(genreId, deleterId);
            }
            else
            {
                throw new InvalidOperationException(Resources.Messages.Errors.GenreNotExist);
            }
        }

        public List<GenreListItemViewModel> GetAllActiveGenres()
        {
            var genres = _genreRepository.GetAllActiveGenres()
                .ToList()
                .Select(genre =>
                {
                    GenreListItemViewModel model = new GenreListItemViewModel();

                    _mapper.Map(genre, model);
                    model.CreatedByUsername = genre.CreatedByUser.Username;
                    model.UpdatedByUsername = genre.UpdatedByUser.Username;
                    model.BookCount = genre.Books.Count(bga => bga.Book.DeletedTime == null);
                    return model;
                })
                .ToList();

            return genres;
        }
        public List<GenreListItemViewModel> SearchGenresByName(string genreName, GenreSortType genreSortType = GenreSortType.CreatedTimeDescending)
        {
            var genre = _genreRepository.GetGenresByName(genreName, genreSortType)
                .Where(g => g.DeletedTime == null)
                .ToList()
                .Select(genre =>
                {
                    GenreListItemViewModel model = new GenreListItemViewModel();
                    _mapper.Map(genre, model);
                    model.BookCount = genre.Books.Count(bga => bga.Book.DeletedTime == null);
                    model.CreatedByUsername = genre.CreatedByUser.Username;
                    model.UpdatedByUsername = genre.UpdatedByUser.Username;
                    return model;
                })
                .ToList();

            return genre;
        }

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
    }
}
