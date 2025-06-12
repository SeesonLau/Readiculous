using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Data.Models
{
    public class Account
    {
        public string AccountId { get; set; }
        public string AccountUsername { get; set; }
        public string AccountEmail { get; set; }
        public string AccountPassword { get; set; }
        public byte[] AccountProfilePicture { get; set; } 
        public RoleType AccountRoleType { get; set; }
        public string AccountCreatedBy { get; set; }
        public DateTime AccountCreatedAt { get; set; }
        public bool AccountIsEdited { get; set; }
        public string AccountEditedBy { get; set; }
        public DateTime AccountEditedAt { get; set; }
        public bool AccountIsDeleted { get; set; }
    }
}
