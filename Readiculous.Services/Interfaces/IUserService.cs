﻿using Microsoft.AspNetCore.Mvc.Rendering;
using Readiculous.Data.Models;
using Readiculous.Services.ServiceModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using X.PagedList;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Services.Interfaces
{
    public interface IUserService
    {
        // Authentication Methods
        LoginResult AuthenticateUserByEmail(string email, string password, ref User user);
        bool IsCurrentPasswordCorrect(string userId, string currentPassword);
        bool IsChangingPassword(EditProfileViewModel editProfileViewModel);

        // CRUD Methods
        Task AddUserAsync(UserViewModel model, string creationId);
        Task UpdateUserAsync(UserViewModel model, string editorId);
        Task UpdateProfileAsync(EditProfileViewModel editProfileViewModel, string editorId);
        void DeleteUser(string userId, string deleterId);

        // Retrieval Methods
        List<UserListItemViewModel> GetUserList(RoleType? role, string username, UserSortType sortType = UserSortType.Latest);
        IPagedList<UserListItemViewModel> GetPaginatedUserList(RoleType? role, string username, int pageNumber, int pageSize = 10, UserSortType sortType = UserSortType.Latest);
        UserViewModel GetUserEditById(string userId);
        EditProfileViewModel GetEditProfileById(string userId);
        UserDetailsViewModel GetUserDetailsById(string userId);
        User GetUserById(string userId);
        string GetEmailByUserId(string userId);

        // Dropdown Filler Methods
        List<SelectListItem> GetUserRoles();
        List<SelectListItem> GetUserSortTypes(UserSortType sortType);
        
        // OTP Methods
        Task<bool> SendOtpForRegistrationAsync(string email);
        bool ValidateOtpForRegistration(string email, string otp);
        Task<bool> ResendOtpForRegistrationAsync(string email);
        // Optionally, you may want to expose a method to get the temp password for testing, but not required for production.
        string GetTempPasswordForEmail(string email);
        Task<bool> SendTempPasswordEmailAsync(string email, string tempPassword);
        
        // Forgot Password Methods
        Task<bool> SendOtpForForgotPasswordAsync(string email);
        bool ValidateOtpForForgotPassword(string email, string otp);
        Task<bool> ResendOtpForForgotPasswordAsync(string email);
        Task<bool> UpdatePasswordAsync(string email, string newPassword);

        // Utility
        bool EmailExists(string email);
    }
}