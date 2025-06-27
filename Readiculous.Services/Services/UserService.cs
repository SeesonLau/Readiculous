using Readiculous.Data.Interfaces;
using Readiculous.Data.Models;
using Readiculous.Resources.Constants;
using Readiculous.Services.Interfaces;
using Readiculous.Services.Manager;
using Readiculous.Services.ServiceModels;
using AutoMapper;
using Supabase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly Client _client;

        public UserService(IUserRepository repository, IMapper mapper, Client client)
        {
            _mapper = mapper;
            _userRepository = repository;
            _client = client;
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
                if (string.IsNullOrEmpty(model.ProfilePictureUrl))
                {
                    model.ProfilePictureUrl = user.ProfilePictureUrl;
                }

                _mapper.Map(model, user);
                user.Username = model.Username.Trim();
                user.Email = model.Email.Trim();
                user.Password = PasswordManager.EncryptPassword(model.Password);
                user.UpdatedTime = DateTime.UtcNow;
                user.UpdatedBy = editorId;

                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    var uri = new Uri(user.ProfilePictureUrl);
                    var relativePath = uri.AbsolutePath.Replace(Const.StoragePath, string.Empty);

                    var result = await _client.Storage
                        .From(Const.BucketName)
                        .Remove(new List<string> { relativePath });

                    if (result == null)
                    {
                        throw new InvalidOperationException(Resources.Messages.Errors.ImageFailedToDelete);
                    }

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
                    }
                }

                _userRepository.UpdateUser(user);
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserNotFound);
            }
        }
        public async Task DeleteUserAsync(string userId, string deleterId)
        {
            if (_userRepository.UserExists(userId))
            {
                var user = _userRepository.GetUserById(userId);

                user.DeletedBy = deleterId;
                user.DeletedTime = DateTime.UtcNow;

                _userRepository.UpdateUser(user);
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserNotFound);
            }
        }

        // Multiple User Retrieval Methods
        public List<UserListItemViewModel> GetUserList(RoleType? role, string username, UserSortType sortType = UserSortType.CreatedTimeDescending)
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
                userViewModel.Role = user.Role.ToString();
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
                .Select(r => new SelectListItem
                {
                    Value = ((int)r).ToString(),
                    Text = r.ToString()
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
                UserSortType.CreatedTimeAscending => userViewModels.OrderBy(u => u.CreatedTime).ToList(),
                UserSortType.CreatedTimeDescending => userViewModels.OrderByDescending(u => u.CreatedTime).ToList(),
                _ => userViewModels.ToList(),
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
                UserSortType.CreatedTimeAscending => userViewModels.OrderBy(u => u.CreatedTime).ToList(),
                UserSortType.CreatedTimeDescending => userViewModels.OrderByDescending(u => u.CreatedTime).ToList(),
                _ => userViewModels.ToList(),
            };
        }
    }
}
