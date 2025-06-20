using System;
using System.Collections;
using System.Collections.Generic;

namespace Readiculous.Data.Models
{
    public partial class Book
    {
        public string BookId { get; set; }
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public int SeriesNumber { get; set; }
        public string Publisher { get; set; }
        public string PublicationYear { get; set; }
        public decimal AverageRating { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedTime { get; set; }

        // Navigation properties
        public virtual User CreatedByUser { get; set; }
        public virtual User UpdatedByUser { get; set; }
        public virtual User DeletedByUser { get; set; }
        public virtual Genre BookGenre { get; set; }

        // Collection navigation properties
        public virtual ICollection<BookGenreAssignment> GenreAssociations { get; set; } = [];
        public virtual ICollection<FavoriteBook> FavoritedbyUsers { get; set; } = [];
        public virtual ICollection<Review> BookReviews { get; set; } = [];
    }
}
