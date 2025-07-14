using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using System;

namespace Readiculous.WebApp.Controllers
{
    [Authorize]
    public class BookDetailController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IReviewService _reviewService;

        public BookDetailController(IBookService bookService, IReviewService reviewService)
        {
            _bookService = bookService;
            _reviewService = reviewService;
        }

        [HttpPost]
        public IActionResult CreateReview([FromBody] ReviewViewModel review)
        {
            if (review == null)
                return BadRequest("Invalid data.");

            review.CreatedTime = DateTime.UtcNow;

            try
            {
                _reviewService.AddReview(review);

                return Json(new
                {
                    reviewer = review.UserName,
                    rating = review.Rating,
                    comment = review.Comment,
                    createdTime = review.CreatedTime.ToString("MMMM dd, yyyy")
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
