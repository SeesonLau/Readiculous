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
            return this.GetDbSet<User>().Any(u => u.DeletedTime == null &&
                                                  u.UserId == userId);
        }

        public bool EmailExists(string email, string userId)
        {
            return this.GetDbSet<User>().Any(u => u.DeletedTime == null&&
                                                  u.UserId != userId &&
                                                  u.Email.ToLower() == email.ToLower());
        }
        public bool UsernameExists(string username, string userId)
        {
            return this.GetDbSet<User>()
                .Any(u => u.DeletedTime == null &&
                          u.UserId != userId &&
                          u.Username == username);
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
                .Where(u => u.DeletedTime == null &&
                            u.Username.ToLower().Contains(username.ToLower()));

            return users;
        }
        public (IQueryable<User>, int) GetPaginatedUsersByUsername(string username, int pageNumber, int pageSize)
        {
            var data = this.GetDbSet<User>()
                .Where(u => u.DeletedTime == null &&
                            u.Username.ToLower().Contains(username.ToLower()));
            var dataCount = data.Count();
            data = data
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(u => u.CreatedByUser)
                .Include(u => u.UpdatedByUser)
                .AsNoTracking();

            return (data, dataCount);
        }

        public IQueryable<User> GetUsersByRoleAndUsername(RoleType role, string username)
        {
            var users = this.GetDbSet<User>()
                .Include(u => u.CreatedByUser)
                .Include(u => u.UpdatedByUser)
                .Where(u => u.DeletedTime == null &&
                            u.Username.ToLower().Contains(username.ToLower()) &&
                            u.Role == role);

            return users;
        }

        public (IQueryable<User>, int) GetPaginatedUsersByRoleAndUsername(RoleType role, string username, int pageNumber, int pageSize)
        {
            var data = this.GetDbSet<User>()
                .Where(u => u.DeletedTime == null &&
                            u.Username.ToLower().Contains(username.ToLower()) &&
                            u.Role == role);
            var dataCount = data.Count();
            data = data
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(u => u.CreatedByUser)
                .Include(u => u.UpdatedByUser)
                .AsNoTracking();

            return (data, dataCount);
        }
        public User GetUserById(string id)
        {
            return this.GetDbSet<User>()
                .Include(u => u.CreatedByUser)
                .Include(u => u.UpdatedByUser)
                .FirstOrDefault(u => u.DeletedTime == null &&
                                     u.UserId == id);
        }
        public User GetUserWithNavigationPropertiesById(string id)
        {
            return this.GetDbSet<User>()
                .Where(u => u.DeletedTime == null &&
                            u.UserId == id)
                .Include(u => u.UserFavoriteBooks)
                .Include(u => u.UserReviews)
                .Include(u => u.CreatedByUser)
                .Include(u => u.UpdatedByUser)
                .FirstOrDefault();
        }
        public User GetUserByEmailAndPassword(string email, string password)
        {
            return this.GetDbSet<User>()
                .FirstOrDefault(u => u.DeletedTime == null && 
                                     u.Password == password && 
                                     u.Email.ToLower() == email.ToLower());
        }

        public User GetUserByEmail(string email)
        {
            return this.GetDbSet<User>()
                .FirstOrDefault(u => u.DeletedTime == null &&
                                     u.Email.ToLower() == email.ToLower());
        }
        
        public int GetActiveUserCount()
        {
            return this.GetDbSet<User>()
                .Count(u => u.DeletedTime != null);
        }
        public IQueryable<User> GetTopReviewers(int numberOfUsers)
        {
            return this.GetDbSet<User>()
                .Where(u => u.DeletedTime == null)
                .OrderBy(u => u.UserReviews.Count())
                .Take(numberOfUsers)
                .AsNoTracking();
        }
    }
}
