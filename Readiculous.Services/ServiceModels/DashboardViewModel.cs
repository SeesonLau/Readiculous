using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Readiculous.Services.ServiceModels
{
    public class DashboardViewModel
    {
        public IEnumerable<BookViewModel> NewBooks { get; set; }
        public IEnumerable<BookViewModel> TopBooks { get; set; }
    }
}
    