using System;

namespace ASI.Basecode.WebApp.Models
{
    public class ReviewModel
    {
        public string ReviewerName { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTime DatePosted { get; set; }
        public int BookId { get; set; }
    }
}
