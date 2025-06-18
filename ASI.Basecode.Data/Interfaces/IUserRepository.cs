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
        void UpdateUser(User user);
        void DeleteUser(string userId);

        //User List Queries
        IQueryable<User> GetUsersByUsername(string username);
        IQueryable<User> GetUsersByUsername(string username, UserSearchType searchType);
        IQueryable<User> GetUsersByRole(RoleType role);
        IQueryable<User> GetUsersByRoleAndUsername(RoleType role, string username);
        IQueryable<User> GetUsersByRoleAndUsername(RoleType role, string username, UserSearchType searchType);
        User GetUserById(string id);
    }
}
