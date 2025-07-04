using Microsoft.AspNetCore.Mvc.Rendering;
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
        List<UserListItemViewModel> GetUserList(RoleType? role, string username, UserSortType sortType = UserSortType.Latest);
        Task DeleteUserAsync(string userId, string deleterId);
        UserViewModel SearchUserEditById(string userId);
        UserDetailsViewModel SearchUserDetailsById(string userId);
        List<SelectListItem> GetUserRoles();
        List<SelectListItem> GetUserSortTypes();
        
        // OTP Methods
        Task<bool> SendOtpForRegistrationAsync(string email);
        bool ValidateOtpForRegistration(string email, string otp);
        Task<bool> ResendOtpForRegistrationAsync(string email);
        // Optionally, you may want to expose a method to get the temp password for testing, but not required for production.
        string GetTempPasswordForEmail(string email);
        Task<bool> SendTempPasswordEmailAsync(string email, string tempPassword);
    }
}