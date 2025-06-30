using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Readiculous.Services.ServiceModels
{
    public class ReviewViewModel
    {
        public string ReviewId { get; set; } // Primary key
        public string BookId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedTime { get; set; }

    }
}
