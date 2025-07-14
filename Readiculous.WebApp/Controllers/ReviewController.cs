using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;

namespace Readiculous.WebApp.Controllers
{
    [Authorize(Roles = "Reviewer")]
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
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
                // ✅ This is the correct call
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
