using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class Genre
    {
        public string GenreId { get; set; } 
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedTime { get; set; }

        // Navigation properties
        public virtual User CreatedByUser { get; set; }
        public virtual User UpdatedByUser { get; set; }
        public virtual User DeletedByUser { get; set; }

        // Collection navigation properties
        public virtual ICollection<BookGenreAssignment> Books { get; set; } = []; 
    }
}
