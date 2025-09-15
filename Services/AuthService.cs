using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;
using PlantManagement.Helper;
using PlantManagement.Models;
using PlantManagement.Repositories;

namespace PlantManagement.Services
{
    public class AuthService
    {
        private readonly AuthRepository _authRepository;
        public AuthService(AuthRepository authRepository)
        {
            _authRepository = authRepository;
        }
        public async Task<User?> Login(string usernameOrEmail, string password)
        {
            if (string.IsNullOrEmpty(usernameOrEmail) || string.IsNullOrEmpty(password))
            {
                return null;
            }
            var user = _authRepository.GetUserByUsernameOrEmail(usernameOrEmail);
            if (user == null || !PasswordHelper.VerifyPassword(password, user.Result.PasswordHash))
            {
                return null;
            }
            return user.Result;
        }

        public async Task<ServiceResult<User>> Register(string username, string email, string password)
        {
            // if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            // {
            //     return new ServiceResult<User> { Success = false, Message = "Invalid input." };
            // }
            var existingUserName = await _authRepository.GetUserByUsernameOrEmail(username);
            var existingEmail = await _authRepository.GetUserByUsernameOrEmail(email);

            if (existingUserName != null)
            {
                return new ServiceResult<User> { Success = false, Message = "Username already exists." };
            }
            if (existingEmail != null)
            {
                return new ServiceResult<User> { Success = false, Message = "Email already exists." };
            }

            var newUser = new User
            {
                Username = username,
                Email = email,
                PasswordHash = PasswordHelper.HashPassword(password)
            };
            await _authRepository.AddAsync(newUser);
            await _authRepository.SaveChangesAsync();
            return new ServiceResult<User> { Success = true, Message = "User registered successfully.", Data = newUser };
        }
    }
}