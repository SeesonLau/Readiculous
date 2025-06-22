using System;

namespace ASI.Basecode.Data.Models
{
    public class ReviewModel
    {
        public string ReviewerName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime DatePosted { get; set; }
    }
}
