using RTools_NTS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Readiculous.Services.ServiceModels
{
    public class UserListItemViewModel // The one beiong displayed as a list item in the UI
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string Role { get; set; }

        //Audit Trail
        public string CreatedByUsername { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedByUsername { get; set; }
        public DateTime UpdatedTime { get; set; }

    }
}
