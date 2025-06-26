using Readiculous.Data.Models;
using Readiculous.Services.ServiceModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Services.Interfaces
{
    public interface IUserService
    {
        LoginResult AuthenticateUserByEmail(string email, string password, ref User user);
        Task AddUserAsync(UserViewModel model, string creationId);

        Task UpdateUserAsync(UserViewModel model, string editorId);
        Task DeleteUserAsync(string userId, string deleterId);
        List<UserListItemViewModel> GetUserList(RoleType? role, string username, UserSortType sortType = UserSortType.CreatedTimeDescending);
        UserViewModel SearchUserEditById(string userId);
        UserDetailsViewModel SearchUserDetailsById(string userId);
        List<SelectListItem> GetUserRoles();
        List<SelectListItem> GetUserSortTypes();
    }
}