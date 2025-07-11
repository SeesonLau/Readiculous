using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Readiculous.Services.ServiceModels
{
    public class ReviewViewModel
    {
        public string ReviewId { get; set; } // Primary key
        public string BookId { get; set; }
        public string UserId { get; set; }
        [DisplayName("Name")]
        public string UserName { get; set; }
        public string BookTitle { get; set; }
        public string Email { get; set; }
        [Range(1, 5, ErrorMessage = ("Ratings must be between 1 to 5!"))]
        public int Rating { get; set; }
        [StringLength(300, ErrorMessage = "Comment must not exceed 200 characters!")]
        public string Comment { get; set; }
        public DateTime CreatedTime { get; set; }

    }
}
