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
        bool EmailExists(string email);
        void AddUser(User user, string creatorId);
        void UpdateUser(User user);

        //User List Queries
        IQueryable<User> GetUsersByUsername(string username);
        IQueryable<User> GetUsersByRoleAndUsername(RoleType role, string username);
        User GetUserById(string id);
        User GetUserWithFilledNavigationPropertiesById(string id);
        User GetUserByEmailAndPassword(string email, string password);
    }
}
