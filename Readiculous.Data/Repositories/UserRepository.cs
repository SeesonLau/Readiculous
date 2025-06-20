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
            return this.GetDbSet<User>().Any(u => email == u.Email &&
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
        public IQueryable<User> GetUsersByUsername(string username, UserSearchType searchType = UserSearchType.UsernameAscending)
        {
            var queryReturn = this.GetDbSet<User>()
                .Where(u => u.Username.ToLower().Contains(username.ToLower()) &&
                        u.DeletedTime == null);

            return searchType switch
            {
                UserSearchType.UsernameAscending => queryReturn.OrderBy(u => u.Username),
                UserSearchType.UsernameDescending => queryReturn.OrderByDescending(u => u.Username),
                UserSearchType.IDAscending => queryReturn.OrderBy(u => u.UserId),
                UserSearchType.IDDescending => queryReturn.OrderByDescending(u => u.UserId),
                _ => queryReturn,
            };
        }
        public IQueryable<User> GetUsersByRoleAndUsername(RoleType role, string username, UserSearchType searchType = UserSearchType.UsernameAscending)
        {
            var queryReturn = this.GetDbSet<User>()
                .Where(u => u.Role == role &&
                        u.Username.ToLower().Contains(username.ToLower()) &&
                        u.DeletedTime == null);

            return searchType switch
            {
                UserSearchType.UsernameAscending => queryReturn.OrderBy(u => u.Username),
                UserSearchType.UsernameDescending => queryReturn.OrderByDescending(u => u.Username),
                UserSearchType.IDAscending => queryReturn.OrderBy(u => u.UserId),
                UserSearchType.IDDescending => queryReturn.OrderByDescending(u => u.UserId),
                _ => queryReturn,
            };
        }
        public User GetUserById(string id)
        {
            return this.GetDbSet<User>().Where(u => u.UserId == id
                                            && u.DeletedTime == null)
                .ToList()
                .FirstOrDefault();
        }
        public User GetUserByEmail(string email)
        {
            return this.GetDbSet<User>().Where(u => u.Email == email
                                            && u.DeletedTime == null)
                .ToList()
                .FirstOrDefault();
        }
    }
}
