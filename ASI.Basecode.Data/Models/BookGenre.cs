using System;

namespace ASI.Basecode.Data.Models
{
    public partial class BookGenre
    {
        public int Id { get; set; }
        public string BookGenreId { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public bool IsUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
