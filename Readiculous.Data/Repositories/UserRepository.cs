using Readiculous.Data.Interfaces;
using Readiculous.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Linq;
using static Readiculous.Resources.Constants.Enums;
using Microsoft.EntityFrameworkCore;

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

        //User List Queries
        public IQueryable<User> GetUsersByUsername(string username)
        {
            var users = this.GetDbSet<User>()
                .Include(u => u.CreatedByUser)
                .Include(u => u.UpdatedByUser)
                .Where(u => u.Username.ToLower().Contains(username.ToLower()) &&
                        u.DeletedTime == null);

            return users;
        }

        public IQueryable<User> GetUsersByRoleAndUsername(RoleType role, string username)
        {
            var users = this.GetDbSet<User>()
                .Include(u => u.CreatedByUser)
                .Include(u => u.UpdatedByUser)
                .Where(u => u.Role == role &&
                        u.Username.ToLower().Contains(username.ToLower()) &&
                        u.DeletedTime == null);

            return users;
        }
        public User GetUserById(string id)
        {
            return this.GetDbSet<User>()
                .Include(u => u.CreatedByUser)
                .Include(u => u.UpdatedByUser)
                .FirstOrDefault(u => u.UserId == id
                                    && u.DeletedTime == null);
        }
        public User GetUserWithNavigationPropertiesById(string id)
        {
            return this.GetDbSet<User>()
                .Include(u => u.CreatedByUser)
                .Include(u => u.UpdatedByUser)
                .FirstOrDefault(u => u.UserId == id
                                    && u.DeletedTime == null);
        }
        public User GetUserByEmailAndPassword(string email, string password)
        {
            return this.GetDbSet<User>()
                .FirstOrDefault(u => u.Email.ToLower() == email.ToLower()
                                    && u.Password == password
                                    && u.DeletedTime == null);
        }

        public User GetUserByEmail(string email)
        {
            return this.GetDbSet<User>()
                .FirstOrDefault(u => u.Email.ToLower() == email.ToLower()
                                    && u.DeletedTime == null);
        }
    }
}
