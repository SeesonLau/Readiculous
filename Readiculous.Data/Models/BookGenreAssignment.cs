using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Readiculous.Data.Models
{
    public class BookGenreAssignment
    {
        public string BookId { get; set; } // Foreign key to Book
        public string GenreId { get; set; } // Foreign key to BookGenre

        // Navigation properties
        public virtual Book Book { get; set; } // Navigation property to Book
        public virtual Genre Genre { get; set; } // Navigation property to BookGenre
    }
}
