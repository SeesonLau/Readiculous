using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Readiculous.Services.ServiceModels
{
    public class GenreListItemViewModel
    {
        public string GenreId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int BookCount { get; set; }

        //Audit Trail
        public string CreatedByUsername { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedByUsername { get; set; }
        public DateTime UpdatedTime { get; set; }
    }
}
