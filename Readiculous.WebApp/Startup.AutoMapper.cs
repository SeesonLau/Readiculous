using AutoMapper;
using Readiculous.Data.Models;
using Readiculous.Services.ServiceModels;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

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
                CreateMap<User, UserListItemViewModel>()
                    .ForMember(dest => dest.CreatedByUsername,
                               opt => opt.MapFrom(src => src.CreatedByUser.Username))
                    .ForMember(dest => dest.UpdatedByUsername,
                               opt => opt.MapFrom(src => src.UpdatedByUser.Username))
                    .ForMember(dest => dest.Role,
                               opt => opt.MapFrom(src => src.Role.ToString()));
                CreateMap<User, UserDetailsViewModel>()
                    .ForMember(dest => dest.ProfileImageUrl,
                               opt => opt.MapFrom(src => src.ProfilePictureUrl))
                    .ForMember(dest => dest.Role, 
                               opt => opt.MapFrom(src => src.Role.ToString()));
                CreateMap<UserViewModel, User>()
                    .ForMember(dest => dest.Username,
                               opt => opt.MapFrom(src => src.Username.Trim()))
                    .ForMember(dest => dest.Email,
                               opt => opt.MapFrom(src => src.Email.Trim()));
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
                CreateMap<Genre, GenreDetailsViewModel>()
                    .ForMember(dest => dest.CreatedByUsername,
                               opt => opt.MapFrom(src => src.CreatedByUser.Username))
                    .ForMember(dest => dest.UpdatedByUsername,
                               opt => opt.MapFrom(src => src.UpdatedByUser.Username));
                CreateMap<GenreViewModel, Genre>()
                    .ForMember(dest => dest.Name,
                               opt => opt.MapFrom(src => src.Name.Trim()))
                    .ForMember(dest => dest.Description,
                               opt => opt.MapFrom(src => src.Description.Trim()));
                CreateMap<Genre, GenreViewModel>();

                // Book Mappings
                CreateMap<Book, BookDetailsViewModel>()
                    .ForMember(dest => dest.CreatedByUserName,
                               opt => opt.MapFrom(src => src.CreatedByUser.Username))
                    .ForMember(dest => dest.UpdatedByUserName,
                               opt => opt.MapFrom(src => src.UpdatedByUser.Username));
                CreateMap<Book, BookListItemViewModel>()
                    .ForMember(dest => dest.CreatedByUserName, 
                               opt => opt.MapFrom(src => src.CreatedByUser.Username))
                    .ForMember(dest => dest.UpdatedByUserName,
                               opt => opt.MapFrom(src => src.UpdatedByUser.Username));
                CreateMap<BookViewModel, Book>()
                    .ForMember(dest => dest.Title,
                               opt => opt.MapFrom(src => src.Title.Trim()))
                    .ForMember(dest => dest.Description,
                               opt => opt.MapFrom(src => src.Description.Trim()))
                    .ForMember(dest => dest.Author,
                               opt => opt.MapFrom(src => src.Author.Trim()))
                    .ForMember(dest => dest.ISBN,
                               opt => opt.MapFrom(src => src.ISBN.Trim()));
                CreateMap<Book, BookViewModel>();
                CreateMap<Book, FavoriteBookModel>();


                // Review Mappings
                CreateMap<ReviewViewModel, Review>();
                CreateMap<Review, ReviewViewModel>();
                CreateMap<Review, ReviewListItemViewModel>()
                    .ForMember(dest => dest.Reviewer,
                               opt => opt.MapFrom(src => src.User.Username))
                    .ForMember(dest => dest.BookName, 
                               opt => opt.MapFrom(src => src.Book.Title))
                    .ForMember(dest => dest.Author,
                               opt => opt.MapFrom(src => src.Book.Author))
                    .ForMember(dest => dest.PublicationYear,
                               opt => opt.MapFrom(src => src.Book.PublicationYear))
                    .ForMember(dest => dest.ReviewBookCrImageUrl,
                               opt => opt.MapFrom(src => src.Book.CoverImageUrl));
            }
        }
    }
}
