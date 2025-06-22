using AutoMapper;
using Readiculous.Data.Interfaces;
using Readiculous.Data.Models;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Services.Services
{
    public class GenreService : IGenreService
    {
        private readonly IGenreRepository _genreRepository;
        private readonly IMapper _mapper;

        public GenreService(IGenreRepository genreRepository, IMapper mapper)
        {
            _genreRepository = genreRepository;
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

        public List<GenreViewModel> GetActiveGenres()
        {
            var genres = _genreRepository.GetGenres()
                .Where(g => g.DeletedTime == null)
                .ToList()
                .Select(genre =>
                {
                    GenreViewModel model = new GenreViewModel();
                    _mapper.Map(genre, model);
                    return model;
                })
                .ToList();
            return genres;
        }

        public List<GenreViewModel> SearchGenresByName(string genreName, GenreSortType genreSortType = GenreSortType.CreatedTimeAscending)
        {
            var genres = _genreRepository.GetGenresByName(genreName.Trim(), genreSortType)
                .Where(g => g.DeletedTime == null)
                .ToList()
                .Select(genre =>
                {
                    GenreViewModel model = new GenreViewModel();
                    _mapper.Map(genre, model);
                    return model;
                })
                .ToList();
            return genres;
        }

        public GenreViewModel GetGenreById(string id)
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

        public GenreViewModel GetGenreByName(string name)
        {
            var genre = _genreRepository.GetGenreByName(name.Trim());
            
            var model = new GenreViewModel();
            _mapper.Map(genre, model);
            return model;
        }
    }
}
