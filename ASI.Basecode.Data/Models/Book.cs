using System;

namespace ASI.Basecode.Data.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Genre { get; set; } = "";
        public string Series { get; set; } = "";
        public double Rating { get; set; }
        public int Year { get; set; }
        public string ImagePath { get; set; }
        public DateTime? AddedDate { get; set; }
        public string Description { get; set; }
    }
}
