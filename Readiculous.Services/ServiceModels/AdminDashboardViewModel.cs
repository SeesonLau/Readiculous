using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Readiculous.Services.ServiceModels
{
    public class AdminDashboardViewModel
    {
        public int UserCount { get; set; }
        public int BookCount { get; set; }
        public int GenreCount { get; set; }
        public List<UserListItemViewModel> TopReviewers { get; set; }
        public Dictionary<GenreListItemViewModel, int> MostUsedGenres { get; set; }
    }
}
