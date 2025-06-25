using AutoMapper;
using Readiculous.Data.Models;
using Readiculous.Services.ServiceModels;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Readiculous.WebApp
{
    // AutoMapper configuration
    internal partial class StartupConfigurer
    {
        /// <summary>
        /// Configure auto mapper
        /// </summary>
        private void ConfigureAutoMapper()
        {
            var mapperConfiguration = new MapperConfiguration(config =>
            {
                config.AddProfile(new AutoMapperProfileConfiguration());
            });

            this._services.AddSingleton<IMapper>(sp => mapperConfiguration.CreateMapper());
        }

        private class AutoMapperProfileConfiguration : Profile
        {
            public AutoMapperProfileConfiguration()
            {

                CreateMap<UserViewModel, User>();
                CreateMap<User, UserViewModel>();
                CreateMap<GenreViewModel, Genre>();
                CreateMap<Genre, GenreViewModel>();
                CreateMap<BookViewModel, Book>();
                CreateMap<Book, BookViewModel>();

                //User Mappings
                CreateMap<User, UserListItemViewModel>()
                    .ForMember(dest => dest.Role,
                               opt => opt.MapFrom(
                                  src => src.Role.ToString()))
                    .ForMember(dest => dest.CreatedByUsername,
                               opt => opt.MapFrom(src => src.CreatedByUser.Username))
                    .ForMember(dest => dest.UpdatedByUsername,
                               opt => opt.MapFrom(src => src.UpdatedByUser.Username));

                // Genre Mappings
                CreateMap<Genre, GenreListItemViewModel>();

                CreateMap<Genre, GenreDetailsViewModel>()
                    .ForMember(dest => dest.BookCount,
                               opt => opt.MapFrom(src =>
                                    src.Books.Count(bga => bga.Book.DeletedTime == null)))
                    .ForMember(dest => dest.CreatedByUsername,
                               opt => opt.MapFrom(src => src.CreatedByUser.Username))
                    .ForMember(dest => dest.UpdatedByUsername,
                               opt => opt.MapFrom(src => src.UpdatedByUser.Username));

                // Book Mappings
                CreateMap<Book, BookDetailsViewModel>()
                    .ForMember(dest => dest.Genres,
                               opt => opt.MapFrom(src => src.GenreAssociations
                                   .Where(ga => ga.Genre.DeletedTime == null)
                                   .Select(ga => ga.Genre.Name).ToList()))
                    .ForMember(dest => dest.CreatedByUserName,
                               opt => opt.MapFrom(src => src.CreatedByUser.Username))
                    .ForMember(dest => dest.UpdatedByUserName,
                               opt => opt.MapFrom(src => src.UpdatedByUser.Username));

                // Average rating still requires mapping: mapping applied after review is implemented
                CreateMap<Book, BookListItemViewModel>()
                    .ForMember(dest => dest.Genres,
                               opt => opt.MapFrom(src => src.GenreAssociations
                                   .Where(ga => ga.Genre.DeletedTime == null)
                                   .Select(ga => ga.Genre.Name).ToList()));
            }
        }
    }
}
