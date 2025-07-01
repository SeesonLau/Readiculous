using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Readiculous.Services.ServiceModels
{
    public class FavoriteBookModel
    {
        public string BookId { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> BookGenres { get; set; } = [];
        public string Author { get; set; }
        public string CoverImageUrl { get; set; }
        public string PublicationYear { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
