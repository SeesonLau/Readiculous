using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Readiculous.Data.Interfaces;
using Readiculous.Data.Models;
using System;
using System.Linq;

namespace Readiculous.Data.Repositories
{
    public class ReviewRepository : BaseRepository, IReviewRepository
    {
        public ReviewRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void AddReview(Review review)
        {
            this.GetDbSet<Review>().Add(review);
            UnitOfWork.SaveChanges();
        }

        public IQueryable<Review> GetReviewsByBookId(string bookId)
        {
            return this.GetDbSet<Review>()
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r => r.BookId == bookId);
        }

        public IQueryable<Review> GetReviewsByUserId(string userId)
        {
            return this.GetDbSet<Review>()
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r => r.UserId == userId);
        }

        public bool ReviewExists(string bookId, string userId)
        {
            return this.GetDbSet<Review>().Any(r => r.BookId == bookId && 
                                                  r.UserId == userId);
        }
    }
}
