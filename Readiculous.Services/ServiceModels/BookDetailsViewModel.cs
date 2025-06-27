using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Readiculous.Services.ServiceModels
{
    public class BookDetailsViewModel
    {
        public string BookId { get; set; }
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public int SeriesNumber { get; set; }
        public string Publisher { get; set; }
        public string PublicationYear { get; set; }
        public string CoverImageUrl { get; set; }
        public List<string> Genres { get; set; } = [];
        public List<BookListItemViewModel> SimilarBooks { get; set; } = [];
        public decimal AverageRating { get; set; }  
        public List<ReviewViewModel> Reviews { get; set; } = [];
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
        public string CreatedByUserName { get; set; }
        public string UpdatedByUserName { get; set; }
    }
}
