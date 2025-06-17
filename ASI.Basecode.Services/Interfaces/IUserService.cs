using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.IO;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IUserService
    {
        LoginResult AuthenticateUser(string userid, string password, RoleType roleType, ref User user);
        void AddUser(UserViewModel model);
        public void UpdateUser(UserViewModel model);
        public void DeleteUser(string userId);
        public List<UserViewModel> SearchAllUsers();
        public List<UserViewModel> SearchUsersByUsername(string username, UserSearchType searchType);
        public List<UserViewModel> SearchUsersByRole(RoleType role, string username, UserSearchType searchType);
        public UserViewModel SearchUserById(string userId);
    }
}
