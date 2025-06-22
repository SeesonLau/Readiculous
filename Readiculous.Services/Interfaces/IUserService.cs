using Readiculous.Data.Models;
using Readiculous.Services.ServiceModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Services.Interfaces
{
    public interface IUserService
    {
        LoginResult AuthenticateUserByEmail(string email, string password, ref User user);
        Task AddUserAsync(UserViewModel model, string creationId);

        Task UpdateUserAsync(UserViewModel model, string editorId);
        Task DeleteUserAsync(string userId, string deleterId);
        List<UserViewModel> SearchAllActiveUsers();
        List<UserViewModel> SearchUsersByUsername(string username, UserSortType searchType);
        List<UserViewModel> SearchUsersByRole(RoleType role, string username, UserSortType searchType);
        UserViewModel SearchUserById(string userId);
        User GetUserByEmail(string email);
    }
}