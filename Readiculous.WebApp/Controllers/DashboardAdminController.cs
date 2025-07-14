using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Readiculous.Data.Models;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using Readiculous.WebApp.Models;
using Readiculous.WebApp.Mvc;
using static Readiculous.Resources.Constants.Enums;
using System.Threading.Tasks;
using Readiculous.WebApp.Authentication;

namespace Readiculous.WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardAdminController : ControllerBase<DashboardAdminController>
    {
        private readonly IBookService _bookService;
        private readonly IGenreService _genreService;
        private readonly IUserService _userService;
        private readonly SignInManager _signInManager;
        private readonly IMapper _mapper;

        public DashboardAdminController(
            IBookService bookService,
            IGenreService genreService,
            IUserService userService,
            IMapper mapper,
            SignInManager signInManager,
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration
        ) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _bookService = bookService;
            _genreService = genreService;
            _userService = userService;
            _signInManager = signInManager;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult DashboardScreen()
        {
            var dashboardViewModel = new DashboardViewModel();
            dashboardViewModel.NewBooks = _bookService.GetBookList(
                searchString: string.Empty,
                genres: new List<GenreViewModel>(),
                userID: null,
                sortType: BookSortType.Latest
                )
                .Take(5)
                .ToList();
            dashboardViewModel.TopBooks = _bookService.GetBookList(
                searchString: string.Empty,
                genres: new List<GenreViewModel>(),
                userID: null,
                sortType: BookSortType.Latest
                )
                .Take(5)
                .ToList();

            return View("~/Views/Dashboard/DashboardAdminScreen.cshtml", dashboardViewModel);
        }
    }
} 