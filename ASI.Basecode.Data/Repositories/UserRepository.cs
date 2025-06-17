using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Data.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(IUnitOfWork unitOfWork) : base(unitOfWork) 
        {

        }

        public IQueryable<User> GetUsers()
        {
            return this.GetDbSet<User>();
        }

        public bool UserExists(string userId)
        {
            return this.GetDbSet<User>().Any(u => u.UserId == userId && 
                                                    !u.IsDeleted && 
                                                    !u.IsUpdated);
        }

        public void AddUser(User user)
        {
            this.GetDbSet<User>().Add(user);
            UnitOfWork.SaveChanges();
        }

        public void UpdateUser(User user)
        {
            User currentUser = this.GetDbSet<User>()
                .Where(u => u.UserId == user.UserId &&
                        !u.IsUpdated &&
                        !u.IsDeleted)
                .FirstOrDefault();

            if (currentUser != null)
            {
                currentUser.IsUpdated = true;
                currentUser.UpdatedBy = user.UpdatedBy;
                currentUser.UpdatedTime = user.UpdatedTime;

                User newUserVersion = new()
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    Password = user.Password,
                    Role = user.Role,
                    CreatedBy = currentUser.CreatedBy,
                    CreatedTime = currentUser.CreatedTime,
                    IsUpdated = false,
                    UpdatedBy = null,
                    UpdatedTime = null,
                    IsDeleted = false
                };

                this.GetDbSet<User>().Update(currentUser);
                this.GetDbSet<User>().Add(newUserVersion);
                UnitOfWork.SaveChanges();
            }
        }

        public void DeleteUser(string userId)
        {
            User user = this.GetDbSet<User>()
                .Where(u => u.UserId == userId &&
                        !u.IsUpdated &&
                        !u.IsDeleted)
                .FirstOrDefault();

            if (user != null)
            {
                user.IsDeleted = true;
                user.UpdatedBy = System.Environment.UserName;
                user.UpdatedTime = DateTime.UtcNow;
                this.GetDbSet<User>().Update(user);
                UnitOfWork.SaveChanges();
            }
        }

        //User List Queries
        public IQueryable<User> GetUsersByUsername(string username)
        {
            return GetUsersByUsername(username, UserSearchType.UsernameAscending);
        }
        public IQueryable<User> GetUsersByUsername(string username, UserSearchType searchType)
        {
            var queryReturn = this.GetDbSet<User>()
                .Where(u => u.Username.ToLower().Contains(username.ToLower()) &&
                        !u.IsUpdated &&
                        !u.IsDeleted);

            return searchType switch
            {
                UserSearchType.UsernameAscending => queryReturn.OrderBy(u => u.Username),
                UserSearchType.UsernameDescending => queryReturn.OrderByDescending(u => u.Username),
                UserSearchType.IDAscending => queryReturn.OrderBy(u => u.UserId),
                UserSearchType.IDDescending => queryReturn.OrderByDescending(u => u.UserId),
                _ => queryReturn,
            };
        }
        public IQueryable<User> GetUsersByRole(RoleType role)
        {
            return GetUsersByRoleAndUsername(role, string.Empty);
        }
        public IQueryable<User> GetUsersByRoleAndUsername(RoleType role, string username)
        {
            return GetUsersByRoleAndUsername(role, username, UserSearchType.UsernameAscending);
        }
        public IQueryable<User> GetUsersByRoleAndUsername(RoleType role, string username, UserSearchType searchType)
        {
            var queryReturn = this.GetDbSet<User>()
                .Where(u => u.Role == role &&
                        u.Username.ToLower().Contains(username.ToLower()) &&
                        !u.IsUpdated &&
                        !u.IsDeleted);

            return searchType switch
            {
                UserSearchType.UsernameAscending => queryReturn.OrderBy(u => u.Username),
                UserSearchType.UsernameDescending => queryReturn.OrderByDescending(u => u.Username),
                UserSearchType.IDAscending => queryReturn.OrderBy(u => u.UserId),
                UserSearchType.IDDescending => queryReturn.OrderByDescending(u => u.UserId),
                _ => queryReturn,
            };
        }
    }
}
