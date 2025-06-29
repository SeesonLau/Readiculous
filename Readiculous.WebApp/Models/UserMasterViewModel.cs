using Readiculous.Services.ServiceModels;
using System.Collections.Generic;

namespace Readiculous.WebApp.Models
{
    public class UserMasterViewModel
    {
        public List<UserListItemViewModel> Users { get; set; }
        public PaginationModel Pagination { get; set; }
    }
}
