using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Readiculous.Services.ServiceModels
{
    public class BookViewModel
    {
        [Required(ErrorMessage = "Title is required!")]
        [StringLength(50, ErrorMessage = "Title must not exceed 50 characters!")]
        public string Title { get; set; }

        [Required(ErrorMessage = "ISBN is required!")]
        [RegularExpression("^(?=(?:[^0-9]*[0-9]){10}(?:(?:[^0-9]*[0-9]){3})?$)[\\d-]+$", ErrorMessage = "Invalid ISBN Format!")]
        public string ISBN { get; set; }

        [Required(ErrorMessage = "Description is required!")]
        [StringLength(150, ErrorMessage = "Description must not exceed characters!")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Author is required!")]
        [StringLength(50, ErrorMessage = "Author must not exceed 50 characters!")]
        public string Author { get; set; }

        [Required(ErrorMessage = "Series number is required!")]
        [Range(0, Int32.MaxValue, ErrorMessage = "Series number must be 0 or above!")]
        public int SeriesNumber { get; set; }

        [Required(ErrorMessage = "Publisher is required!")]
        [StringLength(50, ErrorMessage = "Publisher must not exceed 50 characters!")]
        public string Publisher { get; set; }

        [Required(ErrorMessage = "Publication year is required!")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Publication year must be a valid year!")]
        public string PublicationYear { get; set; }

        [Required(ErrorMessage = "Genre is required!")]
        [MinLength(1, ErrorMessage = "At least one genre must be selected!")]
        public List<string> SelectedGenres { get; set; } = [];
        public double AverageRating { get; set; }
        public List<GenreViewModel> AllAvailableGenres { get; set; } = [];
        public string BookId { get; set; }
        public string CoverImageUrl { get; set; }
        public IFormFile CoverImage { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; }
    }
}
