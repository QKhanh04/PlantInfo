using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PlantManagement.ViewModel
{
    public class SendOtpViewModel
    {
        // [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }
    }

    public class VerifyOtpViewModel
    {
        // [Required(ErrorMessage = "Please enter the OTP.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be numbers only.")]
        public string? Otp { get; set; }
    }

    public class ChangePasswordViewModel
    {
        // [Required(ErrorMessage = "Please enter new password.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        // [Required(ErrorMessage = "Please confirm password.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        // [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}