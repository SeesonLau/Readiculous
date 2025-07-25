﻿using Readiculous.Services.ServiceModels;
using System.Collections.Generic;

namespace Readiculous.WebApp.Models
{
    public class DashboardViewModel
    {
        public List<BookListItemViewModel> NewBooks { get; set; }
        public List<BookListItemViewModel> TopBooks { get; set; }
        public List<BookListItemViewModel> FavoriteBooks { get; set; }
        public string UserRole { get; set; }
    }
}
