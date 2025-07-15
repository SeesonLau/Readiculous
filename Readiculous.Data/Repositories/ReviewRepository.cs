using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
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
        public void UpdateReview(Review review)
        {
            this.GetDbSet<Review>().Update(review);
            UnitOfWork.SaveChanges();
        }

        public IQueryable<Review> GetAllReviews()
        {
            return this.GetDbSet<Review>()
                .Where(r => r.DeletedTime == null &&
                            r.Book.DeletedTime == null &&
                            r.User.DeletedTime == null);
        }

        public IQueryable<Review> GetReviewsByBookId(string bookId)
        {
            return this.GetDbSet<Review>()
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r => r.DeletedTime == null &&
                            r.User.DeletedTime == null &&
                            r.BookId == bookId);
        }

        public IQueryable<Review> GetReviewsWithNavigationPropertiesByUserId(string userId)
        {
            return this.GetDbSet<Review>()
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r => r.DeletedTime == null &&
                            r.Book.DeletedTime == null &&
                            r.UserId == userId);
        }
        public IQueryable<Review> GetReviewsByGenreId(string genreId) 
        {
            return this.GetDbSet<Review>()
                .Where(r => r.DeletedTime == null && 
                            r.DeletedTime == null &&
                            r.Book.GenreAssociations
                    .Any(ga => ga.GenreId == genreId &&
                                ga.Genre.DeletedTime == null));
        }
        public IQueryable<Review> GetReviewsByUserId(string userId)
        {
            return this.GetDbSet<Review>()
                .Where(r => r.UserId == userId &&
                            r.DeletedTime == null);
        }
        public Review GetReviewByBookIdAndUserId(string bookId, string userId)
        {
            return this.GetDbSet<Review>()
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefault(r => r.DeletedTime == null && 
                                     r.UserId == userId &&
                                     r.BookId == bookId);
        }

        public bool ReviewExists(string bookId, string userId)
        {
            return this.GetDbSet<Review>().Any(r => r.DeletedTime == null && 
                                                    r.UserId == userId &&
                                                    r.BookId == bookId);
        }
    }
}
