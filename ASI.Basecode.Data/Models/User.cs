using System;
using System.Collections.Generic;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Data.Models
{
    public partial class User
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public RoleType Role { get; set; } 
        public string ProfilePictureUrl { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? DeletedTime { get; set; }
        public AccessStatus AccessStatus { get; set; }

        // Navigation properties
        public virtual User CreatedByUser { get; set; }
        public virtual User UpdatedByUser { get; set; }
        public virtual User DeletedByUser { get; set; }

        // Collection navigation properties
        public virtual ICollection<FavoriteBook> UserFavoriteBooks { get; set; } = [];
        public virtual ICollection<Review> UserReviews { get; set; } = [];
    }
}
