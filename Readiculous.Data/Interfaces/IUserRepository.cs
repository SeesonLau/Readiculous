using Readiculous.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Data.Interfaces
{
    public interface IUserRepository
    {
        IQueryable<User> GetUsers();
        bool UserExists(string userId);
        bool EmailExists(string email, string userId);
        bool UsernameExists(string username, string userId);
        void AddUser(User user, string creatorId);
        void UpdateUser(User user);

        //User List Queries
        IQueryable<User> GetUsersByUsername(string username);
        (IQueryable<User>, int) GetPaginatedUsersByUsername(string username, int pageNumber, int pageSize, UserSortType sortType);
        IQueryable<User> GetUsersByRoleAndUsername(RoleType role, string username);
        (IQueryable<User>, int) GetPaginatedUsersByRoleAndUsername(RoleType role, string username, int pageNumber, int pageSize, UserSortType sortType);
        User GetUserById(string id);
        User GetUserWithNavigationPropertiesById(string id);
        User GetUserByEmailAndPassword(string email, string password);
        User GetUserByEmail(string email);
        int GetActiveUserCount();
        IQueryable<User> GetTopReviewers(int numberOfUsers);
    }
}
