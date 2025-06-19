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

        public LoginResult AuthenticateUserByEmail(string email, string password, ref User user)
        {
            user = new User();
            var passwordKey = PasswordManager.EncryptPassword(password);
            user = _repository.GetUsers().Where(x => x.Email == email 
                                                        && x.Password == passwordKey
                                                        && x.DeletedTime != null)
            .FirstOrDefault();

            if (user == null)
                return LoginResult.Failed;
            return LoginResult.Success;
        }

        public async Task AddUserAsync(UserViewModel model, string creatorId)
        {
            var user = new User();
            model.UserId = Guid.NewGuid().ToString();

            if (!_repository.EmailExists(model.Email))
            {
                _mapper.Map(model, user);
                user.Password = PasswordManager.EncryptPassword(model.Password);
                user.Role = model.Role;
                user.CreatedTime = DateTime.Now;
                user.UpdatedTime = DateTime.Now;

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

                _repository.AddUser(user, creatorId);
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserExists);
            }
        }

        public async Task UpdateUserAsync(UserViewModel model, string editorId)
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

                _repository.UpdateUser(user);
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserNotFound);
            }
        }

        public async Task DeleteUserAsync(string userId, string deleterId)
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

                _repository.DeleteUser(userId, deleterId);
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
                            && u.DeletedTime == null)
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
        public User GetUserByEmail(string email)
        {
            var user = _repository.GetUserByEmail(email);

            UserViewModel userViewModel = new();
            _mapper.Map(user, userViewModel);

            return user;
        }
    }
}
