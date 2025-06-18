using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IUserService
    {
        LoginResult AuthenticateUser(string userid, string password, RoleType roleType, ref User user);
        Task AddUserAsync(UserViewModel model);
        Task UpdateUserAsync(UserViewModel model);
        Task DeleteUserAsync(string userId);
        List<UserViewModel> SearchAllUsers();
        List<UserViewModel> SearchUsersByUsername(string username, UserSearchType searchType);
        List<UserViewModel> SearchUsersByRole(RoleType role, string username, UserSearchType searchType);
        UserViewModel SearchUserById(string userId);
    }
}
