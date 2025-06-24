using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Readiculous.Services.ServiceModels
{
    public class GenreViewModel
    {
        [Required(ErrorMessage = "Genre Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Genre Description is required.")]
        public string Description { get; set; }

        public string GenreId { get; set; }
        //public List<BookViewModel> Books { get; set; } = new List<BookViewModel>();
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
