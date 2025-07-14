using System.Collections.Generic;

namespace Readiculous.Services.ServiceModels
{

    public class DashboardViewModel
    {
        public List<BookListItemViewModel> NewBooks { get; set; }
        public List<BookListItemViewModel> TopBooks { get; set; }
        public string UserRole { get; set; }
    }
}
