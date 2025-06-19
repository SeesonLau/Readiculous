using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IUserService
    {
        LoginResult AuthenticateUserByEmail(string email, string password, ref User user);
        Task AddUserAsync(UserViewModel model, string creationId);

        Task UpdateUserAsync(UserViewModel model, string editorId);
        Task DeleteUserAsync(string userId, string deleterId);
        List<UserViewModel> SearchAllUsers();
        List<UserViewModel> SearchUsersByUsername(string username, UserSearchType searchType);
        List<UserViewModel> SearchUsersByRole(RoleType role, string username, UserSearchType searchType);
        UserViewModel SearchUserById(string userId);
        User GetUserByEmail(string email);
    }
}