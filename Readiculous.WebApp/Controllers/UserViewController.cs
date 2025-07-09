using Microsoft.AspNetCore.Mvc;
using Readiculous.Services.Interfaces;
using Readiculous.WebApp.Models;
using Readiculous.Data.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System;
using static Readiculous.Resources.Constants.Enums;
using Readiculous.Data;
using Readiculous.Services.ServiceModels;
using Microsoft.AspNetCore.Http;


namespace Readiculous.WebApp.Controllers
{
    [AllowAnonymous]
    public class UserViewController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IUserService _userService;
        private readonly IGuestViewService _guestViewService;
        private readonly ReadiculousDbContext _context;

        public UserViewController(IBookService bookService, IUserService userService, IGuestViewService guestViewService, ReadiculousDbContext context)
        {
            _bookService = bookService;
            _userService = userService;
            _guestViewService = guestViewService;
            _context = context;
        }
        [HttpGet]
        public IActionResult GuestView(string section, string modal = null)
        {
            var newBooks = _bookService.GetBooksForGuest("new-books");
            var topBooks = _bookService.GetBooksForGuest("top-books");

            ViewBag.Section = section;
            ViewBag.Modal = modal;

            var model = new GuestViewModel
            {
                NewBooks = newBooks,
                TopBooks = topBooks
            };

            return View("GuestView", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RegisterAjax(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();

                return Json(new
                {
                    success = false,
                    message = errors.Any() ? string.Join(" ", errors) : "Please complete all fields correctly."
                });
            }

            if (_context.Users.Any(u => u.Email == model.Email))
            {
                return Json(new { success = false, message = "Email is already registered." });
            }

            var newUser = new User
            {
                Email = model.Email,
                Password = model.Password,
                Role = RoleType.Reviewer,
                CreatedTime = DateTime.Now
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Json(new { success = true });
        }


        public IActionResult GenreBooks()
        {

            return View();
        }

        public IActionResult ViewTopBooks(int page = 1)
        {
            return View();
        }

        public IActionResult ViewNewBooks(int page = 1)
        {
            return View();
        }

        public IActionResult GuestView(string section)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            // If Admin, redirect elsewhere
            if (!string.IsNullOrEmpty(userRole) && userRole == "Admin")
            {
                return RedirectToAction("Index", "Home");
            }

            // If logged in Reviewer, do NOT set modal
            if (!string.IsNullOrEmpty(userRole) && userRole == "Reviewer")
            {
                ViewBag.Modal = null;
            }
            else
            {
                // Only set modal for guest
                if (section == "login")
                    ViewBag.Modal = "login";
                else if (section == "register")
                    ViewBag.Modal = "register";
                else
                    ViewBag.Modal = null;
            }

            // Load data for GuestViewModel
            var model = _guestViewService.LoadGuestViewModel();

            return View(model);
        }


    }
}
