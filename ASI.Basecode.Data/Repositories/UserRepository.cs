using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(IUnitOfWork unitOfWork) : base(unitOfWork) 
        {

        }

        public IQueryable<UserTest> GetUsers()
        {
            return this.GetDbSet<UserTest>();
        }

        public bool UserExists(string userId)
        {
            return this.GetDbSet<UserTest>().Any(x => x.UserId == userId);
        }

        public void AddUser(UserTest user)
        {
            this.GetDbSet<UserTest>().Add(user);
            UnitOfWork.SaveChanges();
        }

    }
}
