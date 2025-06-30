using AutoMapper;
using Readiculous.Data.Interfaces;
using Readiculous.Data.Models;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Readiculous.Services.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IMapper _mapper;

        public ReviewService(IReviewRepository reviewRepository, IMapper mapper)
        {
            _reviewRepository = reviewRepository;
            _mapper = mapper;
        }
        
        public bool ReviewExists(string bookId, string userId)
        {
            return _reviewRepository.ReviewExists(bookId, userId);
        }
        public void AddReview(ReviewViewModel model)
        {
            if (_reviewRepository.ReviewExists(model.BookId, model.UserId))
            {
                throw new InvalidOperationException("Review already exists for this book by the user.");
            }

            var review = new Review();

            _mapper.Map(model, review);
            review.ReviewId = Guid.NewGuid().ToString();
            review.CreatedTime = DateTime.UtcNow; 
            review.UpdatedTime = DateTime.UtcNow;
            review.UpdatedBy = model.UserId; 

            _reviewRepository.AddReview(review);
        }
        public void UpdateReview(ReviewViewModel model, string editorId)
        {
            if (!_reviewRepository.ReviewExists(model.BookId, model.UserId))
            {
                throw new InvalidOperationException("Review does not exist for this book by the user.");
            }
            var review = _reviewRepository.GetReviewByBookIdAndUserId(model.BookId, model.UserId);
            if (review == null)
            {
                throw new InvalidOperationException("Review not found.");
            }
            _mapper.Map(model, review);
            review.UpdatedBy = editorId;
            review.UpdatedTime = DateTime.UtcNow;

            _reviewRepository.UpdateReview(review);
        }

        public void DeleteReview(string bookId, string userId, string deleterId)
        {
            if (!_reviewRepository.ReviewExists(bookId, userId))
            {
                throw new InvalidOperationException("Review does not exist for this book by the user.");
            }

            var review = _reviewRepository.GetReviewByBookIdAndUserId(bookId, userId);
            if (review == null)
            {
                throw new InvalidOperationException("Review not found.");
            }

            review.DeletedBy = deleterId;
            review.DeletedTime = DateTime.UtcNow;

            _reviewRepository.UpdateReview(review);
        }

        public List<ReviewListItemViewModel> GetReviewListFromBookId(string bookId)
        {
            var reviews = _reviewRepository.GetReviewsByBookId(bookId)
                .ToList()
                .Select(r =>
                {
                    var reviewViewModel = new ReviewListItemViewModel();

                    _mapper.Map(r, reviewViewModel);
                    reviewViewModel.Reviewer = r.User.Username;
                    reviewViewModel.BookName = r.Book.Title;
                    reviewViewModel.Author = r.Book.Author;
                    reviewViewModel.PublicationYear = r.Book.PublicationYear;
                    reviewViewModel.ReviewBookCrImageUrl = r.Book.CoverImageUrl;

                    return reviewViewModel;
                })
                .ToList();

            return reviews;
        }
        public List<ReviewListItemViewModel> GetReviewListFromUserId(string userId)
        {
            var reviews = _reviewRepository.GetReviewsByUserId(userId)
                .ToList()
                .Select(r =>
                {
                    var reviewViewModel = new ReviewListItemViewModel();

                    _mapper.Map(r, reviewViewModel);
                    reviewViewModel.Reviewer = r.User.Username;
                    reviewViewModel.BookName = r.Book.Title;
                    reviewViewModel.Author = r.Book.Author;
                    reviewViewModel.PublicationYear = r.Book.PublicationYear;
                    reviewViewModel.ReviewBookCrImageUrl = r.Book.CoverImageUrl;

                    return reviewViewModel;
                })
                .ToList();
            return reviews;
        }

        public ReviewViewModel GetReviewByBookIdAndUserId(string bookId, string userId)
        {
            var review = _reviewRepository.GetReviewByBookIdAndUserId(bookId, userId);
            if (review == null)
            {
                throw new InvalidOperationException("Review not found for the specified book and user.");
            }

            var reviewViewModel = new ReviewViewModel();

            _mapper.Map(review, reviewViewModel);
            reviewViewModel.Email = review.User.Email;
            reviewViewModel.UserName = review.User.Username;

            return reviewViewModel;
        }
    }
}
