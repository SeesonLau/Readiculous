using System;

namespace ASI.Basecode.Data.Models
{
    public partial class FavoriteBook
    {
        public int Id { get; set; }
        public string BookId { get; set; } // Foreign key to Book
        public string UserId { get; set; } // Foreign key to User
        public DateTime CreatedTime { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedTime { get; set; }
    }
}
