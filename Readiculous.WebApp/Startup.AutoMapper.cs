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
                //User Mappings
                CreateMap<User, UserListItemViewModel>();
                CreateMap<User, UserDetailsViewModel>();
                CreateMap<UserViewModel, User>();
                CreateMap<User, UserViewModel>();

                // Genre Mappings
                CreateMap<Genre, GenreListItemViewModel>();
                CreateMap<Genre, GenreDetailsViewModel>();
                CreateMap<GenreViewModel, Genre>();
                CreateMap<Genre, GenreViewModel>();

                // Book Mappings
                CreateMap<Book, BookDetailsViewModel>();
                CreateMap<Book, BookListItemViewModel>();
                CreateMap<BookViewModel, Book>();
                CreateMap<Book, BookViewModel>();
                CreateMap<Book, FavoriteBookModel>();
            }
        }
    }
}
