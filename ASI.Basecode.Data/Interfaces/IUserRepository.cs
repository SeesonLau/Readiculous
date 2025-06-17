using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IUserRepository
    {
        IQueryable<User> GetUsers();
        bool UserExists(string userId);
        void AddUser(User user);
        public void UpdateUser(User user);
        public void DeleteUser(string userId);

        //User List Queries
        public IQueryable<User> GetUsersByUsername(string username);
        public IQueryable<User> GetUsersByUsername(string username, UserSearchType searchType);
        public IQueryable<User> GetUsersByRole(RoleType role);
        public IQueryable<User> GetUsersByRoleAndUsername(RoleType role, string username);
        public IQueryable<User> GetUsersByRoleAndUsername(RoleType role, string username, UserSearchType searchType);
    }
}
