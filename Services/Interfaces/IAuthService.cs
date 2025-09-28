using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;
using PlantManagement.Models;

namespace PlantManagement.Services.Interfaces
{
    public interface IAuthService
    {
        public Task<ServiceResult<User>> Register(string username, string email, string password, string confirmPassword);
        public Task<ServiceResult<User?>> Login(string usernameOrEmail, string password);
    }
}