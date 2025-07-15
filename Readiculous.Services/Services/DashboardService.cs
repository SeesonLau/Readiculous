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

namespace Readiculous.Services.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IFavoriteBookRepository _favoriteBookRepository;
        private readonly IMapper _mapper;

        public DashboardService(IBookRepository bookRepository, IFavoriteBookRepository favoriteBookRepository, IMapper mapper)
        {
            _bookRepository = bookRepository;
            _favoriteBookRepository = favoriteBookRepository;
            _mapper = mapper;
        }

        public UserDashboardViewModel GetUserDashboardViewModel(string userId)
        {
            var dashboardViewModel = new UserDashboardViewModel();

            IQueryable<Book> newBooks;
            IQueryable<Book> topBooks;
            IQueryable<FavoriteBook> favoriteBooks;

            (newBooks, _) = _bookRepository.GetPaginatedNewBooks(1, 5);
            (topBooks, _) = _bookRepository.GetPaginatedTopBooks(1, 5);
            (favoriteBooks, _) = _favoriteBookRepository.GetPaginatedFavoriteBooksByUserId(userId, 1, 5);

            dashboardViewModel.NewBooks = _mapper.Map<List<BookListItemViewModel>>(newBooks);
            dashboardViewModel.TopBooks = _mapper.Map<List<BookListItemViewModel>>(topBooks);
            dashboardViewModel.FavoriteBooks = _mapper.Map<List<FavoriteBookModel>>(favoriteBooks);

            return dashboardViewModel;
        }
    }
}
