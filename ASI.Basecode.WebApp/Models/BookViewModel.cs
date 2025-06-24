using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace ASI.Basecode.WebApp.Models
{
    public class BookViewModel
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public int Id { get; set; }
        public string ImagePath { get; set; }
        public int Year { get; set; }
        public string Genre { get; set; }
        public double Rating { get; set; }
        public string Series { get; set; }
        public string ISBN { get; set; }
        public DateTime? PublishDate { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public DateTime? AddedDate { get; set; }

    }
}