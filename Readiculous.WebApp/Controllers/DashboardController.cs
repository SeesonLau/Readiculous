using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using Readiculous.WebApp.Models;

namespace Readiculous.WebApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IBookService _bookService;
        public DashboardController(IBookService bookService)
        {
            _bookService = bookService;
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult DashboardScreen()
        {
            var model = new DashboardViewModel
            {
                NewBooks = new List<BookViewModel>(),
                TopBooks = new List<BookViewModel>()
            };

            return View(model);
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ViewTopBooks()
        {
            return View();
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ViewNewBooks()
        {
            ViewBag.ShowReviewerNav = true;
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GenreScreen()
        {
          
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult BookDetailScreen(int id)
        {
            return View();
        }


    }
}
