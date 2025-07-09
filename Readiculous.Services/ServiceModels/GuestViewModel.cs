using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Readiculous.Services.ServiceModels
{
    public class GuestViewModel
    {
        public List<BookViewModel> NewBooks { get; set; }
        public List<BookViewModel> TopBooks { get; set; }
    }
}
