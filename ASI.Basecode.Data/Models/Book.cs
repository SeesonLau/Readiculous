using System;

namespace ASI.Basecode.Data.Models
{
    public partial class Book
    {
        public int Id { get; set; }
        public string BookId { get; set; }
        public string BookGenreId { get; set; } // Foreign key to BookGenre
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public int SeriesNumber { get; set; }
        public string Publisher { get; set; }
        public DateTime PublicationDate { get; set; }
        public decimal AverageRating { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public bool IsUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
