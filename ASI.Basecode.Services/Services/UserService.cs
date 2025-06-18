using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using Supabase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;
        private readonly Client _client;

        public UserService(IUserRepository repository, IMapper mapper, Client client)
        {
            _mapper = mapper;
            _repository = repository;
            _client = client;
        }

        public LoginResult AuthenticateUser(string userId, string password, RoleType roleType, ref User user)
        {
            user = new User();
            var passwordKey = PasswordManager.EncryptPassword(password);
            user = _repository.GetUsers().Where(x => x.UserId == userId 
                                                        && x.Password == passwordKey
                                                        && !x.IsUpdated
                                                        && !x.IsDeleted)
            .FirstOrDefault();

            if (user == null)
                return LoginResult.Failed;
            if (user.Role != roleType)
                return LoginResult.InvalidRole;
            return LoginResult.Success;
        }

        public async Task AddUserAsync(UserViewModel model)
        {
            var user = new User();
            model.UserId = Guid.NewGuid().ToString();

            if (!_repository.UserExists(model.UserId))
            {
                _mapper.Map(model, user);
                user.UserId = model.UserId;
                user.Password = PasswordManager.EncryptPassword(model.Password);
                user.Role = RoleType.Reviewer;
                user.CreatedBy = Environment.UserName;
                user.CreatedTime = DateTime.Now;
                user.IsUpdated = false;
                user.UpdatedBy = string.Empty;
                user.UpdatedTime = DateTime.Now;
                user.IsDeleted = false;

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

                    _repository.AddUser(user);
                }
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserExists);
            }
        }

        public async Task UpdateUserAsync(UserViewModel model)
        {
            if (_repository.UserExists(model.UserId))
            {
                var user = _repository.GetUserById(model.UserId);
                if(string.IsNullOrEmpty(model.ProfilePictureUrl))
                {
                    model.ProfilePictureUrl = user.ProfilePictureUrl;
                }
                _mapper.Map(model, user);
                user.Password = PasswordManager.EncryptPassword(model.Password);
                user.UpdatedBy = System.Environment.UserName;
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
                    }
                }

                _repository.UpdateUser(user);
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserNotFound);
            }
        }

        public async Task DeleteUserAsync(string userId)
        {
            if (_repository.UserExists(userId))
            {
                var user = _repository.GetUserById(userId);

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

                _repository.DeleteUser(userId);
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserNotFound);
            }
        }

        public List<UserViewModel> SearchAllUsers()
        {
            List<UserViewModel> userViewModels = _repository.GetUsersByUsername(string.Empty)
                .ToList()
                .Select(user =>
                {
                    UserViewModel userViewModel = new();
                    _mapper.Map(user, userViewModel);

                    userViewModel.Password = PasswordManager.DecryptPassword(userViewModel.Password);
                    return userViewModel;
                })
                .ToList();

            return userViewModels;
        }

        public List<UserViewModel> SearchUsersByUsername(string username, UserSearchType searchType)
        {
            List<UserViewModel> userViewModels = _repository.GetUsersByUsername(username, searchType)
                .ToList()
                .Select(user =>
                {
                    UserViewModel userViewModel = new();
                    _mapper.Map(user, userViewModel);

                    userViewModel.Password = PasswordManager.DecryptPassword(userViewModel.Password);
                    return userViewModel;
                })
                .ToList();

            return userViewModels;
        }

        public List<UserViewModel> SearchUsersByRole(RoleType role, string username, UserSearchType searchType)
        {
            List<UserViewModel> userViewModels = _repository.GetUsersByRoleAndUsername(role, username, searchType)
                .ToList()
                .Select(user =>
                {
                    UserViewModel userViewModel = new();
                    _mapper.Map(user, userViewModel);

                    userViewModel.Password = PasswordManager.DecryptPassword(userViewModel.Password);
                    return userViewModel;
                })
                .ToList();
            return userViewModels;
        }

        public UserViewModel SearchUserById(string userId)
        {             
            User user = _repository.GetUsers()
                .Where(u => u.UserId == userId 
                            && !u.IsUpdated 
                            && !u.IsDeleted)
                .FirstOrDefault();

            if (user != null)
            {
                UserViewModel userViewModel = new();
                _mapper.Map(user, userViewModel);
                userViewModel.Password = PasswordManager.DecryptPassword(userViewModel.Password);
                return userViewModel;
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserNotFound);
            }
        }
    }
}
