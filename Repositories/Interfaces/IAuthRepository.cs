using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Models;

namespace PlantManagement.Repositories.Interfaces
{
    public interface IAuthRepository : IGenericRepository<User>
    {
        public Task<User?> GetUserByUsernameOrEmail(string usernameOrEmail);
    }
}