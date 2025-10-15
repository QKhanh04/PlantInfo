using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;
using PlantManagement.DTOs;
using PlantManagement.Models;
using PlantManagement.Repositories.Interfaces;

namespace PlantManagement.Services.Interfaces
{
    public interface IAuthService
    {
        public Task<ServiceResult<User>> Login(String userNameOrEmail, string password);

        public Task<ServiceResult<User>> Register(string email, string userName, string password, string ConfirmedPassword);

        // public Task<ServiceResult<User>> ForgotPassword(string email, string userName, string otp, string password, string ConfirmedPassword);
        public Task<ServiceResult<bool>> SendOtpAsync(HttpContext httpContext, string email);
        public Task<ServiceResult<bool>> ChangePassword(string email, string newPassword, string confirmedNewPassword);
        public Task<bool> VerifyOtp(HttpContext httpContext, string otp);
        public void ClearOtpSession(HttpContext httpContext);
        Task<ServiceResult<bool>> UpdateUserAsync(UpdateUserDTO dto);

    }
}