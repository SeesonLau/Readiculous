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

        public LoginResult AuthenticateUserByEmail(string email, string password, ref User user)
        {
            user = new User();
            var passwordKey = PasswordManager.EncryptPassword(password);
            user = _userRepository.GetUsers().Where(x => x.Email == email.Trim() 
                                                        && x.Password == passwordKey
                                                        && x.DeletedTime == null)
            .FirstOrDefault();

            if (user == null)
                return LoginResult.Failed;
            return LoginResult.Success;
        }

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
                    var fileName = Path.Combine(Const.UserDirectory, $"{user.UserId}{extension}");

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
                if(string.IsNullOrEmpty(model.ProfilePictureUrl))
                {
                    model.ProfilePictureUrl = user.ProfilePictureUrl;
                }

                _mapper.Map(model, user);
                user.Username = model.Username.Trim();
                user.Email = model.Email.Trim();
                user.Password = PasswordManager.EncryptPassword(model.Password);
                user.UpdatedTime = DateTime.UtcNow;
                user.UpdatedBy = editorId;

                //DIfferentiate Update and Delete Date Fields

                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    var extension = Path.GetExtension(model.ProfilePicture.FileName);
                    var fileName = Path.Combine(Const.UserDirectory, $"{user.UserId}{extension}");

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
                /*
                if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                {
                    try
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
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Image deletion error: {ex.Message}");
                    }
                }
                */

                _userRepository.DeleteUser(userId, deleterId);
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserNotFound);
            }
        }

        public List<UserViewModel> SearchAllActiveUsers()
        {
            List<UserViewModel> userViewModels = _userRepository.GetUsersByUsername(string.Empty)
                .ToList()
                .Select(user =>
                {
                    UserViewModel userViewModel = new();
                    _mapper.Map(user, userViewModel);

                    userViewModel.Password = PasswordManager.DecryptPassword(user.Password);
                    return userViewModel;
                })
                .ToList();

            return userViewModels;
        }

        public List<UserViewModel> SearchUsersByUsername(string username, UserSortType searchType)
        {
            List<UserViewModel> userViewModels = _userRepository.GetUsersByUsername(username.Trim(), searchType)
                .ToList()
                .Select(user =>
                {
                    UserViewModel userViewModel = new();
                    _mapper.Map(user, userViewModel);

                    userViewModel.Password = PasswordManager.DecryptPassword(user.Password);
                    return userViewModel;
                })
                .ToList();

            return userViewModels;
        }

        public List<UserViewModel> SearchUsersByRole(RoleType role, string username, UserSortType searchType)
        {
            List<UserViewModel> userViewModels = _userRepository.GetUsersByRoleAndUsername(role, username.Trim(), searchType)
                .ToList()
                .Select(user =>
                {
                    UserViewModel userViewModel = new();
                    _mapper.Map(user, userViewModel);

                    userViewModel.Password = PasswordManager.DecryptPassword(user.Password);
                    return userViewModel;
                })
                .ToList();
            return userViewModels;
        }

        public UserViewModel SearchUserById(string userId)
        {             
            User user = _userRepository.GetUsers()
                .Where(u => u.UserId == userId 
                            && u.DeletedTime == null)
                .FirstOrDefault();

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
        public User GetUserByEmail(string email)
        {
            var user = _userRepository.GetUserByEmail(email.Trim());

            UserViewModel userViewModel = new();
            _mapper.Map(user, userViewModel);

            userViewModel.Password = PasswordManager.DecryptPassword(user.Password);
            return user;
        }
    }
}
