using System;

namespace Readiculous.Data.Models
{
    public partial class FavoriteBook
    {
        public int Id { get; set; } // TO BE REMOVED
        public string BookId { get; set; } // Foreign key to Book
        public string UserId { get; set; } // Foreign key to User
        public DateTime CreatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }

        // Navigation properties
        public virtual Book Book { get; set; } // Navigation property to Book
        public virtual User User { get; set; }

    }
}
