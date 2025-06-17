using System;

namespace ASI.Basecode.Data.Models
{
    public partial class Review
    {
        public string Id { get; set; }
        public string BookId { get; set; } // Foreign key to Book
        public string UserId { get; set; } // Foreign key to User
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
