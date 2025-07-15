using Microsoft.AspNetCore.Mvc;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using System.Collections.Generic;
using System.Linq;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.WebApp.Controllers
{
    public class DashboardAdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IBookService _bookService;
        private readonly IGenreService _genreService;
        public IActionResult AdminScreen()
        {
            return View();
        }
    }
}
