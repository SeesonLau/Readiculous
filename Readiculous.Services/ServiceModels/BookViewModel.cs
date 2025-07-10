using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Readiculous.Services.ServiceModels
{
    public class BookViewModel
    {
        [Required(ErrorMessage = "Title is required!")]
        public string Title { get; set; }

        [Required(ErrorMessage = "ISBN is required!")]
        public string ISBN { get; set; }

        [Required(ErrorMessage = "Author is required!")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Description is required!")]
        public string Author { get; set; }

        [Required(ErrorMessage = "Series number is required!")]
        public int SeriesNumber { get; set; }

        [Required(ErrorMessage = "Publisher is required!")]
        public string Publisher { get; set; }

        [Required(ErrorMessage = "Publication year is required!")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Publication year must be a valid year!")]
        public string PublicationYear { get; set; }

        [Required(ErrorMessage = "Genre is required!")]
        [MinLength(1, ErrorMessage = "At least one genre must be selected!")]
        public List<string> SelectedGenres { get; set; } = [];

        public List<GenreViewModel> AllAvailableGenres { get; set; } = [];
        public string BookId { get; set; }
        public string CoverImageUrl { get; set; }
        public IFormFile CoverImage { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; }

        public decimal Rating { get; set; }
    }
}
