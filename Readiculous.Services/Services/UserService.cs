using AutoMapper;
using AutoMapper.Configuration.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Readiculous.Data.Interfaces;
using Readiculous.Data.Models;
using Readiculous.Data.Repositories;
using Readiculous.Resources.Constants;
using Readiculous.Resources.Messages;
using Readiculous.Services.Interfaces;
using Readiculous.Services.Manager;
using Readiculous.Services.ServiceModels;
using Supabase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using X.PagedList;
using ZXing;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IGenreRepository _genreRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IFavoriteBookRepository _favoriteBookRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IMapper _mapper;
        private readonly Client _client;
        private readonly IEmailService _emailService;

        public UserService(IUserRepository userRepository, IGenreRepository genreRepository, IBookRepository bookRepository, IFavoriteBookRepository favoriteBookRepository, IReviewRepository reviewRepository, IMapper mapper, Client client, IEmailService emailService)
        {
            _userRepository = userRepository;
            _genreRepository = genreRepository;
            _bookRepository = bookRepository;
            _favoriteBookRepository = favoriteBookRepository;
            _reviewRepository = reviewRepository;
            _mapper = mapper;
            _client = client;
            _emailService = emailService;
        }

        // Authentication Methods
        public LoginResult AuthenticateUserByEmail(string email, string password, ref User user)
        {
            user = new User();
            var passwordKey = PasswordManager.EncryptPassword(password);
            user = _userRepository.GetUserByEmailAndPassword(email.Trim(), passwordKey);

            if (user == null)
                return LoginResult.Failed;
            return LoginResult.Success;
        }
        public bool IsCurrentPasswordCorrect(string userId, string currentPassword)
        {
            var user = _userRepository.GetUserById(userId);
            if (user == null)
            {
                throw new InvalidDataException(Errors.UserNotFound);
            }
            return PasswordManager.DecryptPassword(user.Password) == currentPassword;
        }
        public bool IsChangingPassword(EditProfileViewModel editProfileViewModel)
        {
            return !string.IsNullOrWhiteSpace(editProfileViewModel.CurrentPassword) ||
                !string.IsNullOrWhiteSpace(editProfileViewModel.NewPassword) ||
                !string.IsNullOrWhiteSpace(editProfileViewModel.ConfirmPassword);
        }

        // CRUD Operations
        public async Task AddUserAsync(UserViewModel model, string creatorId)
        {
            // Adds User when Email does not exist
            if (_userRepository.EmailExists(model.Email.Trim(), model.UserId))
            {
                throw new DuplicateNameException(Errors.EmailExists);
            }
            if (_userRepository.UsernameExists(model.Username.Trim(), model.UserId))
            {
                throw new DuplicateNameException(Errors.UsernameExists);
            }

            // Creation of New User Entity
            var user = new User();
            if (string.IsNullOrEmpty(model.UserId))
            {
                model.UserId = Guid.NewGuid().ToString();
            }

            // Map properties for Username, Email, CreatedTime and UpdatedTime
            _mapper.Map(model, user);
            user.CreatedTime = DateTime.UtcNow;
            user.UpdatedTime = DateTime.UtcNow;
            //user.AccessStatus = AccessStatus.FirstTime;

            // If a picture was uploaded
            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                // Upload picture
                user.ProfilePictureUrl = await UploadProfilePicture(model.ProfilePicture, user.UserId);
            }

            // Encrypt the password before saving
            user.Password = PasswordManager.EncryptPassword(model.Password);

            //Add User
            _userRepository.AddUser(user, creatorId);

            //Send temp password email if admin created
            /*if (isAdminCreated && !string.IsNullOrEmpty(tempPassword))
            {
                await _emailService.SendTempPasswordEmailAsync(user.Email, tempPassword);
            }*/
        }
        public async Task UpdateUserAsync(UserViewModel model, string editorId)
        {
            if(!_userRepository.UserExists(model.UserId))
            {
                throw new KeyNotFoundException(Errors.UserExists);
            }
            
            if(_userRepository.EmailExists(model.Email.Trim(), model.UserId))
            {
                throw new DuplicateNameException(Errors.EmailExists);
            }

            if(_userRepository.UsernameExists(model.Username.Trim(), model.UserId))
            {
                throw new DuplicateNameException(Errors.UsernameNotExist);
            }    

            var user = _userRepository.GetUserById(model.UserId);

            // Map properties for Updated Time, and UpdatedBy

            _mapper.Map(model, user);
            user.UpdatedTime = DateTime.UtcNow;
            user.UpdatedBy = editorId;

            // Only update password if a new one was provided
            if (!string.IsNullOrEmpty(model.Password))
            {
                user.Password = PasswordManager.EncryptPassword(model.Password);
            }

            // Handle profile picture changes
            if (model.RemoveProfilePicture) // If the remove checkbox was checked
            {
                if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                {
                    await DeleteProfilePicture(user.ProfilePictureUrl);
                    user.ProfilePictureUrl = null;
                }
            }
            // If a new picture was uploaded
            else if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                // Upload new picture
                if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                {
                    // Delete old picture first
                    await DeleteProfilePicture(user.ProfilePictureUrl);
                }
                user.ProfilePictureUrl = await UploadProfilePicture(model.ProfilePicture, user.UserId);
            }
            // If neither, the picture remains unchanged
            _userRepository.UpdateUser(user);
        }
        public async Task UpdateProfileAsync(EditProfileViewModel editProfileViewModel, string editorId)
        {
            if (!_userRepository.UserExists(editProfileViewModel.UserId))
            {
                throw new KeyNotFoundException(Errors.UserNotFound);
            }
            if (_userRepository.EmailExists(editProfileViewModel.Email.Trim(), editProfileViewModel.UserId))
            {
                throw new DuplicateNameException(Errors.EmailExists);
            }
            if(_userRepository.UsernameExists(editProfileViewModel.Username.Trim(), editProfileViewModel.UserId))
            {
                throw new DuplicateNameException(Errors.UsernameExists);
            }

            var user = _userRepository.GetUserById(editProfileViewModel.UserId);
            var userViewModel = new UserViewModel();

            _mapper.Map(editProfileViewModel, userViewModel);
            userViewModel.AccessStatus = user.AccessStatus;
            if (string.IsNullOrEmpty(editProfileViewModel.NewPassword))
            {
                userViewModel.Password = PasswordManager.DecryptPassword(user.Password);
            }

            await UpdateUserAsync(userViewModel, editorId);
        }
        public void DeleteUser(string userId, string deleterId)
        {
            if (!_userRepository.UserExists(userId))
            {
                throw new KeyNotFoundException(Errors.UserNotFound);
            }
            if (userId == deleterId)
            {
                throw new InvalidOperationException(Resources.Messages.Errors.UserCannotDeleteSelf);
            }

            var user = _userRepository.GetUserById(userId); 

            user.UserReviews = _reviewRepository.GetReviewsByUserId(userId).ToList();
            foreach (var review in user.UserReviews)
            {
                review.DeletedBy = deleterId;
                review.DeletedTime = DateTime.UtcNow;

                _reviewRepository.UpdateReview(review);
            }
            user.DeletedBy = deleterId;
            user.DeletedTime = DateTime.UtcNow;

            _userRepository.UpdateUser(user); 
        }
        // Multiple User Retrieval Methods
        public List<UserListItemViewModel> GetUserList(RoleType? role, string username, UserSortType sortType = UserSortType.Latest)
        {
            List<UserListItemViewModel> userViewModels = new();

            if (role.HasValue && !string.IsNullOrEmpty(username))
            {
                userViewModels = GetUsersByRoleAndUsername(role.Value, username, sortType);
            }
            else if (role.HasValue)
            {
                userViewModels = GetUsersByRoleAndUsername(role.Value, string.Empty, sortType);
            }
            else if (!string.IsNullOrEmpty(username))
            {
                userViewModels = GetUsersByUsername(username, sortType);
            }
            else
            {
                userViewModels = GetAllActiveUsers();
            }

            return userViewModels;

        }
        public IPagedList<UserListItemViewModel> GetPaginatedUserList(RoleType? role, string username, int pageNumber, int pageSize = 10, UserSortType sortType = UserSortType.Latest)
        {
            if (!role.HasValue && string.IsNullOrEmpty(username) && sortType == UserSortType.Latest)
            {
                return GetAllPaginatedActiveUsers(pageNumber, pageSize);
            }
            else if (!role.HasValue)
            {
                return GetPaginatedUsersByUsername(username, pageNumber, pageSize, sortType);
            }
            else if (string.IsNullOrEmpty(username))
            {
                return GetPaginatedUsersByRoleAndUsername(role.Value, string.Empty, pageNumber, pageSize, sortType);
            }
            else
            {
                return GetPaginatedUsersByRoleAndUsername(role.Value, username, pageNumber, pageSize, sortType);
            }
        }

        // Single User Retrieval Methods
        public UserViewModel GetUserEditById(string userId)
        {
            User user = _userRepository.GetUserById(userId);

            if (user != null)
            {
                var userViewModel = new UserViewModel();

                _mapper.Map(user, userViewModel);
                userViewModel.Password = PasswordManager.DecryptPassword(user.Password);
                return userViewModel;
            }
            else
            {
                throw new KeyNotFoundException(Errors.UserNotFound);
            }
        }
        public EditProfileViewModel GetEditProfileById(string userId)
        {
            User user = _userRepository.GetUserById(userId);
            if (user != null)
            {
                EditProfileViewModel userViewModel = new();
                _mapper.Map(user, userViewModel);
                userViewModel.NewPassword = PasswordManager.DecryptPassword(user.Password);
                userViewModel.RemoveProfilePicture = !string.IsNullOrEmpty(user.ProfilePictureUrl);
                return userViewModel;
            }
            else
            {
                throw new KeyNotFoundException(Errors.UserNotFound);
            }
        }
        public UserDetailsViewModel GetUserDetailsById(string userId)
        {
            User user = _userRepository.GetUserWithNavigationPropertiesById(userId);
            if (user == null)
                throw new KeyNotFoundException(Errors.UserNotFound);

            UserDetailsViewModel userDetails = new();
            _mapper.Map(user, userDetails);

            userDetails.FavoriteBookModels = _mapper.Map<List<FavoriteBookModel>>(user.UserFavoriteBooks);
            userDetails.UserReviewModels = _mapper.Map<List<ReviewListItemViewModel>>(user.UserReviews);

            var bookIds = userDetails.FavoriteBookModels
                .Select(x => x.BookId)
                .ToList();
            userDetails.TopGenres = _genreRepository.GetTopGenresFromBookIds(bookIds);

            userDetails.AverageRating = userDetails.UserReviewModels.Count > 0
                ? Math.Round(userDetails.UserReviewModels.Average(r => r.Rating), 2)
                : 0;

            return userDetails;
        }
        public User GetUserById(string userId)
        {
            return _userRepository.GetUserById(userId);
        }

        //Populating Dropdown Lists
        public List<SelectListItem> GetUserRoles()
        {
            return Enum.GetValues(typeof(RoleType))
                .Cast<RoleType>()
                .Select(r => new SelectListItem
                {
                    Value = ((int)r).ToString(),
                    Text = r.ToString()
                }).ToList();
        }
        public List<SelectListItem> GetUserSortTypes(UserSortType sortType)
        {
            return Enum.GetValues(typeof(UserSortType))
                .Cast<UserSortType>()
                .Select(t =>
                {
                    var displayName = t.GetType()
                                     .GetMember(t.ToString())
                                     .First()
                                     .GetCustomAttribute<DisplayAttribute>()?
                                     .Name ?? t.ToString();

                    return new SelectListItem
                    {
                        Value = ((int)t).ToString(),
                        Text = displayName,
                        Selected = t == sortType
                    };
                }).ToList();
        }
        // String Helper
        public string GetEmailByUserId(string userId)
        {
            var user = _userRepository.GetUserById(userId);

            if(user == null)
            {
                throw new KeyNotFoundException(Errors.UserNotFound);
            }

            return user.Email;
        }

        // Helper methods for searching users
        private List<UserListItemViewModel> GetAllActiveUsers()
        {
            var allUsers = _userRepository.GetUsersByUsername(string.Empty);
            var result = _mapper.Map<List<UserListItemViewModel>>(allUsers)
                .OrderByDescending(u => u.CreatedTime)
                .ToList();

            return result;
        }
        private IPagedList<UserListItemViewModel> GetAllPaginatedActiveUsers(int pageNumber, int pageSize)
        {
            IQueryable<User> queryableUserListItems;
            int userCount;

            (queryableUserListItems, userCount) = _userRepository.GetPaginatedUsersByUsername(string.Empty, pageNumber, pageSize);
            var listUserListItems = queryableUserListItems.ToList();

            var userMapModels = _mapper.Map<List<UserListItemViewModel>>(listUserListItems);
            var result = userMapModels
                .OrderByDescending(u => u.UpdatedTime);

            return new StaticPagedList<UserListItemViewModel>(
                result.ToList(),
                pageNumber,
                pageSize,
                userCount);
        }

        private List<UserListItemViewModel> GetUsersByUsername(string username, UserSortType sortType)
        {
            var usersByUsername = _userRepository.GetUsersByUsername(username.Trim());
            var result = _mapper.Map<List<UserListItemViewModel>>(usersByUsername);

            return sortType switch
            {
                UserSortType.UsernameAscending => result.OrderBy(u => u.UserName).ToList(),
                UserSortType.UsernameDescending => result.OrderByDescending(u => u.UserName).ToList(),
                UserSortType.Oldest => result.OrderBy(u => u.UpdatedTime).ToList(),
                UserSortType.Latest => result.OrderByDescending(u => u.UpdatedTime).ToList(),
                _ => result.OrderByDescending(u => u.UpdatedTime).ToList()
            };
        }

        private IPagedList<UserListItemViewModel> GetPaginatedUsersByUsername(string username, int pageNumber, int pageSize, UserSortType sortType)
        {
            IQueryable<User> queryableUserListItems;
            int userCount;

            (queryableUserListItems, userCount) = _userRepository.GetPaginatedUsersByUsername(username, pageNumber, pageSize);
            var listUserListItems = queryableUserListItems.ToList();

            var userMapModels = _mapper.Map<List<UserListItemViewModel>>(listUserListItems);
            var result = SortUsers(userMapModels, sortType);

            return new StaticPagedList<UserListItemViewModel>(
                result.ToList(),
                pageNumber,
                pageSize,
                userCount);
        }

        private List<UserListItemViewModel> GetUsersByRoleAndUsername(RoleType role, string username, UserSortType searchType)
        {
            var usersByRoleAndUsername = _userRepository.GetUsersByRoleAndUsername(role, username.Trim());
            var result = _mapper.Map<List<UserListItemViewModel>>(usersByRoleAndUsername);

            return searchType switch
            {
                UserSortType.UsernameAscending => result.OrderBy(u => u.UserName).ToList(),
                UserSortType.UsernameDescending => result.OrderByDescending(u => u.UserName).ToList(),
                UserSortType.Oldest => result.OrderBy(u => u.UpdatedTime).ToList(),
                UserSortType.Latest => result.OrderByDescending(u => u.UpdatedTime).ToList(),
                _ => result.OrderByDescending(u => u.UpdatedTime).ToList(),
            };
        }

        private IPagedList<UserListItemViewModel> GetPaginatedUsersByRoleAndUsername(RoleType role, string username, int pageNumber, int pageSize, UserSortType sortType)
        {
            IQueryable<User> queryableUserListItems;
            int userCount;

            (queryableUserListItems, userCount) = _userRepository.GetPaginatedUsersByRoleAndUsername(role, username, pageNumber, pageSize);
            var listUserListItems = queryableUserListItems.ToList();

            var userMapModels = _mapper.Map<List<UserListItemViewModel>>(listUserListItems);
            var result = SortUsers(userMapModels, sortType);

            return new StaticPagedList<UserListItemViewModel>(
                result.ToList(),
                pageNumber,
                pageSize,
                userCount);
        }

        // Helper methods for profile picture management
        private async Task DeleteProfilePicture(string pictureUrl)
        {
            var uri = new Uri(pictureUrl);
            var relativePath = uri.AbsolutePath.Replace(Const.StoragePath, string.Empty);

            var result = await _client.Storage
                .From(Const.BucketName)
                .Remove(new List<string> { relativePath });

            if (result == null)
            {
                throw new InvalidOperationException(Errors.ImageFailedToDelete);
            }
        }
        private async Task<string> UploadProfilePicture(IFormFile file, string userId)
        {
            var extension = Path.GetExtension(file.FileName);
            var fileName = Path.Combine(Const.UserDirectory, $"{userId}-{Guid.NewGuid():N}{extension}");

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                var uploadResult = await _client.Storage
                    .From(Const.BucketName)
                    .Upload(fileBytes, fileName, new Supabase.Storage.FileOptions
                    {
                        ContentType = file.ContentType,
                        Upsert = true
                    });

                if (!string.IsNullOrEmpty(uploadResult))
                {
                    return _client.Storage
                        .From(Const.BucketName)
                        .GetPublicUrl(fileName);
                }
            }

            throw new InvalidOperationException(Resources.Messages.Errors.ImageFailedToUpload);
        }
        private IEnumerable<UserListItemViewModel> SortUsers(IEnumerable<UserListItemViewModel> userViewModels, UserSortType sortType)
        {
            return sortType switch
            {
                UserSortType.UsernameAscending => userViewModels.OrderBy(u => u.UserName),
                UserSortType.UsernameDescending => userViewModels.OrderByDescending(u => u.UserName),
                UserSortType.Oldest => userViewModels.OrderBy(u => u.UpdatedTime),
                UserSortType.Latest => userViewModels.OrderByDescending(u => u.UpdatedTime),
                _ => userViewModels.OrderByDescending(u => u.UpdatedTime)
            };
        }

        // OTP Methods
        public async Task<bool> SendOtpForRegistrationAsync(string email)
        {
            try
            {
                // Check if email already exists
                if (_userRepository.EmailExists(email.Trim(), string.Empty))
                {
                    return false;
                }

                // Generate and store OTP only
                var otp = OtpManager.GenerateOtp();
                OtpManager.StoreOtpAndPassword(email, otp, null); // Store null for temp password for now

                // Send OTP via email service (no temp password)
                return await _emailService.SendOtpEmailAsync(email, otp, null);
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> ResendOtpForRegistrationAsync(string email)
        {
            try
            {
                // Check if email already exists
                if (_userRepository.EmailExists(email.Trim(), string.Empty))
                {
                    return false;
                }

                // Generate and store new OTP only
                var otp = OtpManager.GenerateOtp();
                OtpManager.StoreOtpAndPassword(email, otp, null);

                // Send OTP via email service (no temp password)
                return await _emailService.SendOtpEmailAsync(email, otp, null);
            }
            catch (Exception)
            {
                return false;
            }
        }
        // New method to send temp password after OTP is confirmed
        public async Task<bool> SendTempPasswordEmailAsync(string email, string tempPassword)
        {
            // Compose and send a new email with only the temp password
            return await _emailService.SendTempPasswordEmailAsync(email, tempPassword);
        }

        public bool ValidateOtpForRegistration(string email, string otp)
        {
            // Only validate OTP, do not remove temp password yet
            return OtpManager.ValidateOtp(email, otp);
        }

        public string GetTempPasswordForEmail(string email)
        {
            return OtpManager.GetTempPassword(email);
        }

        // Forgot Password Methods
        public async Task<bool> SendOtpForForgotPasswordAsync(string email)
        {
            try
            {
                // Check if email exists
                if (!_userRepository.EmailExists(email.Trim(), string.Empty))
                {
                    return false;
                }

                // Generate and store OTP for forgot password
                var otp = OtpManager.GenerateOtp();
                OtpManager.StoreOtpAndPassword(email, otp, null);

                // Send OTP via email service
                return await _emailService.SendOtpForForgotPasswordEmailAsync(email, otp);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ResendOtpForForgotPasswordAsync(string email)
        {
            try
            {
                // Check if email exists
                if (!_userRepository.EmailExists(email.Trim(), string.Empty))
                {
                    return false;
                }

                // Generate and store new OTP
                var otp = OtpManager.GenerateOtp();
                OtpManager.StoreOtpAndPassword(email, otp, null);

                // Send OTP via email service
                return await _emailService.SendOtpForForgotPasswordEmailAsync(email, otp);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ValidateOtpForForgotPassword(string email, string otp)
        {
            return OtpManager.ValidateOtp(email, otp);
        }

        public async Task<bool> UpdatePasswordAsync(string email, string newPassword)
        {
            try
            {
                var user = _userRepository.GetUserByEmail(email);
                if (user == null)
                {
                    return false;
                }

                user.Password = PasswordManager.EncryptPassword(newPassword);
                user.UpdatedTime = DateTime.UtcNow;
                _userRepository.UpdateUser(user);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool EmailExists(string email)
        {
            return _userRepository.EmailExists(email.Trim(), string.Empty);
        }

    }
}
