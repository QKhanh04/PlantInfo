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
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        public AuthService(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }
        public async Task<ServiceResult<User?>> Login(string usernameOrEmail, string password)
        {
            if (string.IsNullOrEmpty(usernameOrEmail) || string.IsNullOrEmpty(password))
            {
                return ServiceResult<User?>.Fail("Invalid input");
            }
            var user = _authRepository.GetUserByUsernameOrEmail(usernameOrEmail);
            if (user == null)
            {
                return ServiceResult<User?>.Fail("User not found");
            }
            if (!PasswordHelper.VerifyPassword(password, user.Result.Password))
            {
                return ServiceResult<User?>.Fail("Incorrect password");
            }

            return ServiceResult<User?>.Ok(user.Result, "Login successful");
        }

        public async Task<ServiceResult<User>> Register(string username, string email, string password, string confirmPassword)
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
            if (password != confirmPassword)
            {
                return new ServiceResult<User> { Success = false, Message = "Passwords do not match." };
            }

            var newUser = new User
            {
                Username = username,
                Email = email,
                Password = PasswordHelper.HashPassword(password)
            };
            await _authRepository.AddAsync(newUser);
            await _authRepository.SaveChangesAsync();
            return new ServiceResult<User> { Success = true, Message = "User registered successfully.", Data = newUser };
        }
    }
}