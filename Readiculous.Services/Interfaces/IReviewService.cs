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
        List<ReviewListItemViewModel> GetReviewListFromBookId(string bookId);
        List<ReviewListItemViewModel> GetReviewListFromUserId(string userId);
    }
}
