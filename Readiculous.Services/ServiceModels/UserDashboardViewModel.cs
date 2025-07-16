using System.Collections.Generic;

namespace Readiculous.Services.ServiceModels
{

    public class UserDashboardViewModel
    {
        public List<BookListItemViewModel> NewBooks { get; set; }
        public List<BookListItemViewModel> TopBooks { get; set; }
        public List<BookListItemViewModel> FavoriteBooks { get; set; }
    }
}
