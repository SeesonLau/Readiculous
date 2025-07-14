using Readiculous.Data.Models;
using Readiculous.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Readiculous.Services.Interfaces
{
    public interface IReviewService
    {
        void AddReview(ReviewViewModel model);
        void UpdateReview(ReviewViewModel model, string editorId);
        void DeleteReview(string bookId, string userId, string deleterId);
        bool ReviewExists(string bookId, string userId);
        List<ReviewListItemViewModel> GetReviewListFromBookId(string bookId);
        List<ReviewListItemViewModel> GetReviewListFromUserId(string userId);
        ReviewViewModel GetReviewByBookIdAndUserId(string bookId, string userId);
        ReviewViewModel GenerateInitialReviewViewModel(string bookId, string userId, string userName);
    }
}
