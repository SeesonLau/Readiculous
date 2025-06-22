// Models/UserViewModel.cs
using ASI.Basecode.Data.Models;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public class UserViewModel
    {
        public List<Book> NewBooks { get; set; }
        public List<Book> TopBooks { get; set; }
    }
}
