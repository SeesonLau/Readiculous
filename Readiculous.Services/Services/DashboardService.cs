using AutoMapper;
using NetTopologySuite.Index.HPRtree;
using Readiculous.Data.Interfaces;
using Readiculous.Data.Models;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using Supabase.Gotrue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Services.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUserRepository _userRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IGenreRepository _genreRepository;
        private readonly IFavoriteBookRepository _favoriteBookRepository;
        private readonly IMapper _mapper;

        public DashboardService(IUserRepository userRepository, IBookRepository bookRepository, IGenreRepository genreRepository, IFavoriteBookRepository favoriteBookRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _bookRepository = bookRepository;
            _genreRepository = genreRepository;
            _favoriteBookRepository = favoriteBookRepository;
            _mapper = mapper;
        }

        public UserDashboardViewModel GetUserDashboardViewModel(string userId)
        {
            var userDashboardViewModel = new UserDashboardViewModel();

            IQueryable<Book> newBooks;
            IQueryable<Book> topBooks;
            IQueryable<FavoriteBook> favoriteBooks;

            (newBooks, _) = _bookRepository.GetPaginatedNewBooks(1, 5);
            (topBooks, _) = _bookRepository.GetPaginatedTopBooks(1, 5);
            (favoriteBooks, _) = _favoriteBookRepository.GetPaginatedFavoriteBooksByUserId(userId, 1, 5);

            userDashboardViewModel.NewBooks = _mapper.Map<List<BookListItemViewModel>>(newBooks);
            userDashboardViewModel.TopBooks = _mapper.Map<List<BookListItemViewModel>>(topBooks);
            userDashboardViewModel.FavoriteBooks = _mapper.Map<List<FavoriteBookModel>>(favoriteBooks);



            foreach (var model in userDashboardViewModel.NewBooks)
            {
                var book = newBooks.FirstOrDefault(b => b.BookId == model.BookId);
                if (book != null)
                {
                    var validReviews = book.BookReviews.Where(r => r.DeletedTime == null).ToList();
                    model.AverageRating = validReviews.Any() ? (decimal)validReviews.Average(r => r.Rating) : 0;
                    model.TotalReviews = validReviews.Count;
                }
            }

            foreach (var model in userDashboardViewModel.TopBooks)
            {
                var book = topBooks.FirstOrDefault(b => b.BookId == model.BookId);
                if (book != null)
                {
                    var validReviews = book.BookReviews.Where(r => r.DeletedTime == null).ToList();
                    model.AverageRating = validReviews.Any() ? (decimal)validReviews.Average(r => r.Rating) : 0;
                    model.TotalReviews = validReviews.Count;
                }
            }

            return userDashboardViewModel;
        }

        public AdminDashboardViewModel GetAdminDashboardViewModel()
        {
            var adminDashboardViewModel = new AdminDashboardViewModel();

            adminDashboardViewModel.UserCount = _userRepository.GetActiveUserCount();
            adminDashboardViewModel.BookCount = _bookRepository.GetActiveBookCount();
            adminDashboardViewModel.GenreCount = _genreRepository.GetActiveGenreCount();

            var queryableTopReviewers = _userRepository.GetTopReviewers(5);
            var listTopReviewers = queryableTopReviewers.ToList();

            List<UserDetailsViewModel> topReviewerDetails = new();
            foreach(var reviewer in listTopReviewers)
            {
                UserDetailsViewModel userDetails = new();
                _mapper.Map(reviewer, userDetails);

                userDetails.FavoriteBookModels = _mapper.Map<List<FavoriteBookModel>>(reviewer.UserFavoriteBooks);
                userDetails.UserReviewModels = _mapper.Map<List<ReviewListItemViewModel>>(reviewer.UserReviews);

                var bookIds = userDetails.FavoriteBookModels
                    .Select(x => x.BookId)
                    .ToList();
                userDetails.TopGenres = _genreRepository.GetTopGenresFromBookIds(bookIds);

                userDetails.AverageRating = userDetails.UserReviewModels.Count > 0
                    ? Math.Round(userDetails.UserReviewModels.Average(r => r.Rating), 2)
                    : 0;

                topReviewerDetails.Add(userDetails);
            }

            adminDashboardViewModel.TopReviewers = topReviewerDetails
                .OrderByDescending(r => r.AverageRating) 
                .ToList();

            var queryableMostUsedGenres = _genreRepository.GetMostUsedGenresWithCount(5);
            var listMostUsedGenres = queryableMostUsedGenres.ToList();
            adminDashboardViewModel.MostUsedGenres = listMostUsedGenres
            .ToDictionary(
                kvp => _mapper.Map<GenreListItemViewModel>(kvp.Key),
                kvp => kvp.Value
            );

            return adminDashboardViewModel;
        }
    }
}
