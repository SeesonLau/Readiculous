using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
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

        public void AddUser(UserViewModel model)
        {
            var user = new User();
            model.UserId = Guid.NewGuid().ToString();

            if (!_repository.UserExists(model.UserId))
            {
                _mapper.Map(model, user);
                user.Password = PasswordManager.EncryptPassword(model.Password);
                user.Role = RoleType.Reviewer;
                user.CreatedBy = System.Environment.UserName;
                user.CreatedTime = DateTime.Now;
                user.IsUpdated = false;
                user.UpdatedBy = string.Empty;
                user.UpdatedTime = DateTime.Now;
                user.IsDeleted = false;

                _repository.AddUser(user);
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserExists);
            }
        }

        public void UpdateUser(UserViewModel model)
        {
            var user = new User();

            if (_repository.UserExists(model.UserId))
            {
                _mapper.Map(model, user);
                user.Password = PasswordManager.EncryptPassword(model.Password);
                user.UpdatedBy = System.Environment.UserName;
                user.UpdatedTime = DateTime.UtcNow;
                _repository.UpdateUser(user);
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserNotFound);
            }
        }

        public void DeleteUser(string userId)
        {
            if (_repository.UserExists(userId))
            {
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
