using Readiculous.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Data.Interfaces
{
    public interface IUserRepository
    {
        IQueryable<User> GetUsers();
        bool UserExists(string userId);
        bool EmailExists(string email);
        void AddUser(User user, string creatorId);
        void UpdateUser(User user);
        void DeleteUser(string userId, string deleterId);

        //User List Queries
        IQueryable<User> GetUsersByUsername(string username, UserSortType searchType = UserSortType.UsernameAscending);
        IQueryable<User> GetUsersByRoleAndUsername(RoleType role, string username, UserSortType searchType);
        User GetUserById(string id);
        User GetUserByEmail(string email);
    }
}
