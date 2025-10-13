using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;
using PlantManagement.Helper;
using PlantManagement.Models;
using PlantManagement.Repositories;
using PlantManagement.Repositories.Interfaces;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _userRepo;
        private readonly IEmailSender _emailSender;
        private const string OtpSessionKey = "OTP";
        private const string EmailSessionKey = "OTP_EMAIL";
        public AuthService(IAuthRepository userRepo, IEmailSender emailSender)
        {
            _userRepo = userRepo;
            _emailSender = emailSender;
        }



        public async Task<ServiceResult<bool>> SendOtpAsync(HttpContext httpContext, string email)
        {
            if (await _userRepo.GetUserByUsernameOrEmail(email) == null)
                return ServiceResult<bool>.Fail("Email is not existed");

            var otp = new Random().Next(100000, 999999).ToString();
            httpContext.Session.SetString("OTP", otp);
            httpContext.Session.SetString("OTP_EMAIL", email);

            var subject = "Your OTP Code";
            var body = $"Your OTP code is: {otp}";
            return ServiceResult<bool>.Ok(_emailSender.SendEmail(email, subject, body), "OTP has been sent to your email.");
        }

        public async Task<bool> VerifyOtp(HttpContext httpContext, string otp)
        {
            var storedOtp = httpContext.Session.GetString(OtpSessionKey);
            return storedOtp == otp;
        }

        public async Task<ServiceResult<bool>> ChangePassword(string email, string newPassword, string confirmedNewPassword)
        {
            var user = await _userRepo.GetUserByUsernameOrEmail(email);

            if (user == null)
            {
                return ServiceResult<bool>.Fail("Email is not existed");
            }
            else if (newPassword != confirmedNewPassword)
            {
                return ServiceResult<bool>.Fail("Passwords do not match.");
            }

            user.Password = PasswordHelper.HashPassword(newPassword);
            await _userRepo.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true, "Password changed successfully!");
        }

        public void ClearOtpSession(HttpContext httpContext)
        {
            httpContext.Session.Remove(OtpSessionKey);
            httpContext.Session.Remove(EmailSessionKey);
        }

        public async Task<ServiceResult<User>> Login(String userNameOrEmail, string password)
        {
            var user = await _userRepo.GetUserByUsernameOrEmail(userNameOrEmail);
            if (user == null)
            {
                return ServiceResult<User>.Fail("UserName or Email is not existed");
            }
            bool verify = PasswordHelper.VerifyPassword(password, user.Password);
            if (verify) return ServiceResult<User>.Ok(user);
            return ServiceResult<User>.Fail("Password is not correct");
        }

        public async Task<ServiceResult<User>> Register(string email, string userName, string password, string ConfirmedPassword)
        {
            var existingUserName = await _userRepo.GetUserByUsernameOrEmail(userName);
            var existingEmail = await _userRepo.GetUserByUsernameOrEmail(email);

            if (existingUserName != null)
            {
                return ServiceResult<User>.Fail("User name is existed");
            }
            else if (existingEmail != null)
            {
                return ServiceResult<User>.Fail("Email is existed");
            }
            else if (password != ConfirmedPassword)
            {
                return ServiceResult<User>.Fail("Confirm password is not correct");
            }
            var user = new User
            {
                Username = userName,
                Email = email,
                Password = PasswordHelper.HashPassword(password),
                Role = "User"
            };
            await _userRepo.AddAsync(user);
            await _userRepo.SaveChangesAsync();
            return ServiceResult<User>.Ok(user);
        }


    }
}
