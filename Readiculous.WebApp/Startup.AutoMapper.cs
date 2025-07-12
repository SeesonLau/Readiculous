using AutoMapper;
using Readiculous.Data.Models;
using Readiculous.Services.ServiceModels;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Collections.Generic;

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
                CreateMap<EditProfileViewModel, User>();
                CreateMap<User, EditProfileViewModel>();
                CreateMap<EditProfileViewModel, UserViewModel>();


                // Genre Mappings
                CreateMap<Genre, GenreListItemViewModel>()
                    .ForMember(dest => dest.CreatedByUsername,
                               opt => opt.MapFrom(
                                  src => src.CreatedByUser.Username))
                    .ForMember(dest => dest.UpdatedByUsername,
                               opt => opt.MapFrom(src => src.UpdatedByUser.Username));
                CreateMap<Genre, GenreDetailsViewModel>();
                CreateMap<GenreViewModel, Genre>();
                CreateMap<Genre, GenreViewModel>();

                // Book Mappings
                CreateMap<Book, BookDetailsViewModel>();
                CreateMap<Book, BookListItemViewModel>()
                    .ForMember(dest => dest.CreatedByUserName, 
                               opt => opt.MapFrom(src => src.CreatedByUser.Username))
                    .ForMember(dest => dest.UpdatedByUserName,
                               opt => opt.MapFrom(src => src.UpdatedByUser.Username));
                CreateMap<BookViewModel, Book>();
                CreateMap<Book, BookViewModel>();
                CreateMap<Book, FavoriteBookModel>();


                // Review Mappings
                CreateMap<ReviewViewModel, Review>();
                CreateMap<Review, ReviewViewModel>();
                CreateMap<Review, ReviewListItemViewModel>();
            }
        }
    }
}
