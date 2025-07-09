using Readiculous.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Readiculous.Data.Interfaces
{
    public interface IReviewRepository
    {
        bool ReviewExists(string bookId, string userId);
        void AddReview(Review review);
        void UpdateReview(Review review);

        IQueryable<Review> GetReviewsByBookId(string bookId);
        IQueryable<Review> GetReviewsWithNavigationPropertiesByUserId(string userId);
        IQueryable<Review> GetReviewsByUserId(string userId);
        IQueryable<Review> GetReviewsByGenreId(string genreId);
        Review GetReviewByBookIdAndUserId(string bookId, string userId);
    }
}
