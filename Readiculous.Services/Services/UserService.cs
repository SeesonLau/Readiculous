using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Readiculous.Data.Interfaces;
using Readiculous.Data.Models;
using Readiculous.Resources.Constants;
using Readiculous.Services.Interfaces;
using Readiculous.Services.Manager;
using Readiculous.Services.ServiceModels;
using Supabase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IFavoriteBookRepository _favoriteBookRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IMapper _mapper;
        private readonly Client _client;
        private readonly IEmailService _emailService;

        public UserService(IUserRepository userRepository, IBookRepository bookRepository, IFavoriteBookRepository favoriteBookRepository, IReviewRepository reviewRepository, IMapper mapper, Client client, IEmailService emailService)
        {
            _userRepository = userRepository;
            _bookRepository = bookRepository;
            _favoriteBookRepository = favoriteBookRepository;
            _reviewRepository = reviewRepository;
            _mapper = mapper;
            _client = client;
            _emailService = emailService;
        }

        // Authentication Method
        public LoginResult AuthenticateUserByEmail(string email, string password, ref User user)
        {
            user = new User();
            var passwordKey = PasswordManager.EncryptPassword(password);
            user = _userRepository.GetUserByEmailAndPassword(email.Trim(), passwordKey);

            if (user == null)
                return LoginResult.Failed;
            return LoginResult.Success;
        }
        // CRUD Operations
        public async Task AddUserAsync(UserViewModel model, string creatorId)
        {
            if (!_userRepository.EmailExists(model.Email.Trim()))
            {
                var user = new User();
                if (string.IsNullOrEmpty(model.UserId))
                {
                    model.UserId = Guid.NewGuid().ToString();
                }

                _mapper.Map(model, user);
                user.Username = user.Username.Trim();
                user.Email = user.Email.Trim();
                user.Password = PasswordManager.EncryptPassword(model.Password);
                user.CreatedTime = DateTime.UtcNow;
                user.UpdatedTime = DateTime.UtcNow;
                user.AccessStatus = AccessStatus.FirstTime;

                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    var extension = Path.GetExtension(model.ProfilePicture.FileName);
                    var fileName = Path.Combine(Const.UserDirectory, $"{user.UserId}-{Guid.NewGuid():N}{extension}");

                    using (var memoryStream = new MemoryStream())
                    {
                        await model.ProfilePicture.CopyToAsync(memoryStream);
                        var fileBytes = memoryStream.ToArray();

                        var uploadResult = await _client.Storage
                            .From(Const.BucketName)
                            .Upload(fileBytes, fileName, new Supabase.Storage.FileOptions
                            {
                                ContentType = model.ProfilePicture.ContentType,
                                Upsert = true
                            });

                        if (!string.IsNullOrEmpty(uploadResult))
                        {
                            user.ProfilePictureUrl = _client.Storage
                                .From(Const.BucketName)
                                .GetPublicUrl(fileName);
                        }
                        else
                        {
                            throw new Exception(Resources.Messages.Errors.ImageFailedToUpload);
                        }
                    }
                }

                _userRepository.AddUser(user, creatorId);
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserExists);
            }
        }
        public async Task UpdateUserAsync(UserViewModel model, string editorId)
        {
            if (_userRepository.UserExists(model.UserId) && _userRepository.EmailExists(model.Email.Trim()))
            {
                var user = _userRepository.GetUserById(model.UserId);

                // Map properties for Username, Email, Updated Time, and UpdatedBy
                _mapper.Map(model, user);
                user.Username = model.Username.Trim();
                user.Email = model.Email.Trim();
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
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserNotFound);
            }
        }
        public async Task DeleteUserAsync(string userId, string deleterId)
        {
            if (await Task.Run(() => _userRepository.UserExists(userId)))
            {
                var user = await Task.Run(() => _userRepository.GetUserById(userId));

                user.UserReviews = _reviewRepository.GetReviewsByUserId(userId).ToList();
                foreach (var review in user.UserReviews)
                {
                    review.DeletedBy = deleterId;
                    review.DeletedTime = DateTime.UtcNow;

                    _reviewRepository.UpdateReview(review);
                }
                user.DeletedBy = deleterId;
                user.DeletedTime = DateTime.UtcNow;

                await Task.Run(() => _userRepository.UpdateUser(user));
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserNotFound);
            }
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

        // Single User Retrieval Methods
        public UserViewModel SearchUserEditById(string userId)
        {
            User user = _userRepository.GetUserById(userId);

            if (user != null)
            {
                UserViewModel userViewModel = new();

                _mapper.Map(user, userViewModel);
                userViewModel.Password = PasswordManager.DecryptPassword(user.Password);
                return userViewModel;
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserNotFound);
            }
        }
        public UserDetailsViewModel SearchUserDetailsById(string userId)
        {
            User user = _userRepository.GetUserById(userId);

            if (user != null)
            {
                UserDetailsViewModel userViewModel = new();

                _mapper.Map(user, userViewModel);
                userViewModel.ProfileImageUrl = user.ProfilePictureUrl;
                userViewModel.Role = user.Role.ToString();
                userViewModel.FavoriteBookModels = _favoriteBookRepository.GetFavoriteBooksByUserId(userId)
                    .ToList()
                    .Select(fb =>
                    {
                        var favoriteBookModel = new FavoriteBookModel();
                        var book = _bookRepository.GetBookById(fb.BookId);

                        _mapper.Map(book, favoriteBookModel);
                        favoriteBookModel.BookGenres = book.GenreAssociations.Select(bg => bg.Genre.Name)
                            .ToList();

                        return favoriteBookModel;
                    })
                    .ToList();
                userViewModel.UserReviewModels = _reviewRepository.GetReviewsByUserId(userId)
                    .ToList()
                    .Select(r =>
                    {
                        var reviewViewModel = new ReviewListItemViewModel();

                        _mapper.Map(r, reviewViewModel);
                        reviewViewModel.Reviewer = r.User.Username;
                        reviewViewModel.BookName = r.Book.Title;
                        reviewViewModel.Author = r.Book.Author;
                        reviewViewModel.PublicationYear = r.Book.PublicationYear;
                        reviewViewModel.ReviewBookCrImageUrl = r.Book.CoverImageUrl;

                        return reviewViewModel;
                    })
                    .ToList();
                userViewModel.CreatedByUserName = user.CreatedByUser.Username;
                userViewModel.UpdatedByUserName = user.UpdatedByUser.Username;

                return userViewModel;
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserNotFound);
            }
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
        public List<SelectListItem> GetUserSortTypes()
        {
            return Enum.GetValues(typeof(UserSortType))
                .Cast<UserSortType>()
                .Select(r => {
                    var displayName = r.GetType()
                                     .GetMember(r.ToString())
                                     .First()
                                     .GetCustomAttribute<DisplayAttribute>()?
                                     .Name ?? r.ToString();

                    return new SelectListItem
                    {
                        Value = ((int)r).ToString(),
                        Text = displayName,
                        Selected = r == UserSortType.Latest 
                    };
                }).ToList();
        }
        // Helper methods for searching users
        private List<UserListItemViewModel> GetAllActiveUsers()
        {
            var userViewModels = _userRepository.GetUsersByUsername(string.Empty)
                .ToList()
                .Select(user =>
                {
                    UserListItemViewModel userViewModel = new();

                    _mapper.Map(user, userViewModel);
                    userViewModel.Role = user.Role.ToString();
                    userViewModel.CreatedByUsername = user.CreatedByUser.Username;
                    userViewModel.UpdatedByUsername = user.UpdatedByUser.Username;

                    return userViewModel;
                });

            return userViewModels.OrderByDescending(u => u.CreatedTime).ToList();
        }
        private List<UserListItemViewModel> GetUsersByUsername(string username, UserSortType searchType)
        {
            var userViewModels = _userRepository.GetUsersByUsername(username.Trim())
                .ToList()
                .Select(user =>
                {
                    UserListItemViewModel userViewModel = new();
                    _mapper.Map(user, userViewModel);
                    return userViewModel;
                });

            return searchType switch
            {
                UserSortType.UsernameAscending => userViewModels.OrderBy(u => u.UserName).ToList(),
                UserSortType.UsernameDescending => userViewModels.OrderByDescending(u => u.UserName).ToList(),
                UserSortType.Oldest => userViewModels.OrderBy(u => u.UpdatedTime).ToList(),
                UserSortType.Latest => userViewModels.OrderByDescending(u => u.UpdatedTime).ToList(),
                _ => userViewModels.OrderByDescending(u => u.UpdatedTime).ToList()
            };
        }
        private List<UserListItemViewModel> GetUsersByRoleAndUsername(RoleType role, string username, UserSortType searchType)
        {
            var userViewModels = _userRepository.GetUsersByRoleAndUsername(role, username.Trim())
                .ToList()
                .Select(user =>
                {
                    UserListItemViewModel userViewModel = new();

                    _mapper.Map(user, userViewModel);
                    userViewModel.Role = user.Role.ToString();
                    userViewModel.CreatedByUsername = user.CreatedByUser.Username;
                    userViewModel.UpdatedByUsername = user.UpdatedByUser.Username;

                    return userViewModel;
                });

            return searchType switch
            {
                UserSortType.UsernameAscending => userViewModels.OrderBy(u => u.UserName).ToList(),
                UserSortType.UsernameDescending => userViewModels.OrderByDescending(u => u.UserName).ToList(),
                UserSortType.Oldest => userViewModels.OrderBy(u => u.UpdatedTime).ToList(),
                UserSortType.Latest => userViewModels.OrderByDescending(u => u.UpdatedTime).ToList(),
                _ => userViewModels.OrderByDescending(u => u.UpdatedTime).ToList(),
            };
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
                throw new InvalidOperationException(Resources.Messages.Errors.ImageFailedToDelete);
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

        // OTP Methods
        public async Task<bool> SendOtpForRegistrationAsync(string email)
        {
            try
            {
                // Check if email already exists
                if (_userRepository.EmailExists(email.Trim()))
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
                if (_userRepository.EmailExists(email.Trim()))
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
    }
}
