using Readiculous.Services.ServiceModels;
using System.Collections.Generic;

namespace Readiculous.WebApp.Models
{
    public class NewBookModel
    {
        public IEnumerable<BookViewModel> Books { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
