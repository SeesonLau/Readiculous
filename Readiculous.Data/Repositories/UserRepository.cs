﻿using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Readiculous.Data.Interfaces;
using Readiculous.Data.Models;
using System;
using System.Linq;
using ZXing;
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
        public (IQueryable<User>, int) GetPaginatedUsersByUsername(string username, int pageNumber, int pageSize, UserSortType sortType)
        {
            // Handle null username case
            if (string.IsNullOrEmpty(username))
            {
                // Return all users or empty set based on your requirements
                var allData = this.GetDbSet<User>()
                    .Where(u => u.DeletedTime == null);

                var count = allData.Count();
                var data = allData
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Include(u => u.CreatedByUser)
                    .Include(u => u.UpdatedByUser)
                    .AsNoTracking();

                return (data, count);
            }

            // Original logic for non-null username
            var filteredData = this.GetDbSet<User>()
                .Where(u => u.DeletedTime == null &&
                           u.Username.ToLower().Contains(username.ToLower()));

            var filteredCount = filteredData.Count();
            var pagedData = filteredData
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(u => u.CreatedByUser)
                .Include(u => u.UpdatedByUser)
                .AsNoTracking();

            return (pagedData, filteredCount);
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

        public (IQueryable<User>, int) GetPaginatedUsersByRoleAndUsername(RoleType role, string username, int pageNumber, int pageSize = 10, UserSortType sortType = UserSortType.Latest)
        {
            var data = this.GetDbSet<User>()
                .Where(u => u.DeletedTime == null &&
                            u.Username.ToLower().Contains(username.ToLower()) &&
                            u.Role == role);
            var dataCount = data.Count();
            data = SortUsers(data, sortType)
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
                .Count(u => u.DeletedTime == null);
        }
        public IQueryable<User> GetTopReviewers(int numberOfUsers)
        {
            return this.GetDbSet<User>()
                 .Where(u => u.DeletedTime == null)
                 .Include(u => u.UserReviews)  // Include reviews for counting
                 .OrderByDescending(u => u.UserReviews.Count)  // Changed to Descending
                 .Take(numberOfUsers)
                 .Include(u => u.UserFavoriteBooks)  // Include after ordering to avoid performance impact
                 .Include(u => u.CreatedByUser)
                 .Include(u => u.UpdatedByUser)
                 .AsNoTracking();
        }

        private IQueryable<User> SortUsers(IQueryable<User> users, UserSortType sortType)
        {
            return sortType switch
            {
                UserSortType.UsernameAscending => users.OrderBy(u => u.Username),
                UserSortType.UsernameDescending => users.OrderByDescending(u => u.Username),
                UserSortType.Oldest => users.OrderBy(u => u.UpdatedTime),
                UserSortType.Latest => users.OrderByDescending(u => u.UpdatedTime),
                _ => users.OrderByDescending(u => u.UpdatedTime),
            };
        }
    }
}
