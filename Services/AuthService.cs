using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<User?> Register(string username, string email, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return null;
            }
            var existingUser = await _authRepository.GetUserByUsernameOrEmail(username);
            if (existingUser != null)
            {
                return null;
            }
            var hashedPassword = PasswordHelper.HashPassword(password);
            var newUser = new User
            {
                Username = username,
                Email = email,
                PasswordHash = hashedPassword
            };
            await _authRepository.AddAsync(newUser);
            await _authRepository.SaveChangesAsync();
            return newUser;
        }
    }
}