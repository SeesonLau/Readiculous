using Readiculous.Data.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Readiculous.Services.ServiceModels
{
    public class UserDetailsViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ProfileImageUrl { get; set; }
        public string Role { get; set; }
        public string CreatedByUserName { get; set; }
        public string CreatedTime { get; set; }
        public string UpdatedByUserName { get; set; }
        public string UpdatedTime { get; set; }
        public decimal AverageRating { get; set; }

        public List<string> TopGenres { get; set; }
        public List<ReviewListItemViewModel> UserReviewModels { get; set; } = [];
        public List<FavoriteBookModel> FavoriteBookModels { get; set; } = [];



    }
}
