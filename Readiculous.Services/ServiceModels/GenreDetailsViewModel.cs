using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Readiculous.Services.ServiceModels
{
   public class GenreDetailsViewModel // For Details Screen of Genre
    {
        public string GenreId { get; set; }
        public string GenreName { get; set; }
        public string Description { get; set; }
        public int BookCount { get; set; }
        public string CreatedByUsername { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedByUsername { get; set; }
        public DateTime UpdatedTime { get; set; }

        public List<BookListItemViewModel> Books { get; set; } = [];

    }
}
