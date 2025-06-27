using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Readiculous.Services.ServiceModels
{
    public class BookListItemViewModel
    {
        public string BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int SeriesNumber { get; set; }
        public List<string> Genres { get; set; } = [];
        public string CoverImageUrl { get; set; }
        public string PublicationYear { get; set; }
        public decimal AverageRating { get; set; }
        public bool IsFavorite { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedByUserName { get; set; }
        public DateTime UpdatedTime { get; set; }
    }
}
