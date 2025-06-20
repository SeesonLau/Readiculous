using Microsoft.AspNetCore.Authorization;

namespace ASI.Basecode.WebApp.Models
{
    public class BookViewModel
    {
        public string Title { get; set; }
        public string Author { get; set; }

        public string ImagePath { get; set; }
        public int Year { get; set; }
        public string Genre { get; set; }

        public double Rating { get; set; }

    }
}