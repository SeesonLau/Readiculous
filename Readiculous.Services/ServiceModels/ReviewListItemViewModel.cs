using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Readiculous.Services.ServiceModels
{
    public class ReviewListItemViewModel
    {
        public string ReviewId { get; set; } 
        public string Reviewer { get; set; }
        public string BookName { get; set; }
        public string Author { get; set; }
        public string PublicationYear { get; set; }
        public string ReviewBookCrImageUrl { get; set; }
        public string Comment { get; set; }
        public decimal Rating { get; set; }
        public DateTime CreatedTime { get; set; }

    }
}
