using Readiculous.Data.Interfaces;
using Readiculous.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Linq;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Data.Repositories
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
                                                u.DeletedTime == null);
        }

        public bool EmailExists(string email)
        {
            return this.GetDbSet<User>().Any(u => u.Email.ToLower() == email.ToLower() &&
                                                u.DeletedTime == null);
        }

        public void AddUser(User user, string creatorId)
        {
            this.GetDbSet<User>().Add(user);
            UnitOfWork.SaveChanges();

            user.CreatedBy = user.UpdatedBy = creatorId;
            UnitOfWork.SaveChanges();
        }

        public void UpdateUser(User user)
        {
            this.GetDbSet<User>().Update(user);
            UnitOfWork.SaveChanges();
        }

        public void DeleteUser(string userId, string deleterId)
        {
            User user = this.GetDbSet<User>()
                .Where(u => u.UserId == userId &&
                        u.DeletedTime == null)
                .FirstOrDefault();

            if (user != null)
            {
                user.DeletedBy = deleterId;
                user.DeletedTime = DateTime.UtcNow;
                this.GetDbSet<User>().Update(user);
                UnitOfWork.SaveChanges();
            }
        }

        //User List Queries
        public IQueryable<User> GetUsersByUsername(string username, UserSortType searchType = UserSortType.UsernameAscending)
        {
            var queryReturn = this.GetDbSet<User>()
                .Where(u => u.Username.ToLower().Contains(username.ToLower()) &&
                        u.DeletedTime == null);

            return searchType switch
            {
                UserSortType.UsernameAscending => queryReturn.OrderBy(u => u.Username),
                UserSortType.UsernameDescending => queryReturn.OrderByDescending(u => u.Username),
                UserSortType.CreatedTimeAscending => queryReturn.OrderBy(u => u.UserId),
                UserSortType.CreatedTimeDescending => queryReturn.OrderByDescending(u => u.UserId),
                _ => queryReturn,
            };
        }
        public IQueryable<User> GetUsersByRoleAndUsername(RoleType role, string username, UserSortType searchType = UserSortType.CreatedTimeAscending)
        {
            var queryReturn = this.GetDbSet<User>()
                .Where(u => u.Role == role &&
                        u.Username.ToLower().Contains(username.ToLower()) &&
                        u.DeletedTime == null);

            return searchType switch
            {
                UserSortType.UsernameAscending => queryReturn.OrderBy(u => u.Username),
                UserSortType.UsernameDescending => queryReturn.OrderByDescending(u => u.Username),
                UserSortType.CreatedTimeAscending => queryReturn.OrderBy(u => u.CreatedTime),
                UserSortType.CreatedTimeDescending => queryReturn.OrderByDescending(u => u.CreatedTime),
                _ => queryReturn,
            };
        }
        public User GetUserById(string id)
        {
            return this.GetDbSet<User>().FirstOrDefault(u => u.UserId == id
                                            && u.DeletedTime == null);
        }
        public User GetUserByEmail(string email)
        {
            return this.GetDbSet<User>().Where(u => u.Email.ToLower() == email.ToLower()
                                            && u.DeletedTime == null)
                .ToList()
                .FirstOrDefault();
        }
    }
}
