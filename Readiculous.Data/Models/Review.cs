using System;

namespace Readiculous.Data.Models
{
    public partial class Review
    {
        public string BookId { get; set; } // Foreign key to Book
        public string UserId { get; set; } // Foreign key to User
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedTime { get; set; }

        // Navigation properties
        public virtual Book Book { get; set; } // Navigation property to Book
        public virtual User User { get; set; } // Navigation property to User
    }
}
