using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PlantManagement.Services.Implementations;
using PlantManagement.Services.Interfaces;
using PlantManagement.ViewModel;

namespace PlantManagement.Pages.Auth
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IAuthService _userService;
        private readonly ILogger<ForgotPasswordModel> _logger;

        public ForgotPasswordModel(ILogger<ForgotPasswordModel> logger, IAuthService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [BindProperty]
        public SendOtpViewModel SendOtpVM { get; set; }
        [BindProperty]
        public VerifyOtpViewModel VerifyOtpVM { get; set; }
        [BindProperty]
        public ChangePasswordViewModel ChangePasswordVM { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Step { get; set; } = 1;

        public void OnGet()
        {
            // Đảm bảo luôn về bước 1 khi load lại trang
            Step = 1;
            // Có thể reset ViewModel ở đây nếu muốn
        }

        public async Task<IActionResult> OnPostSendOtp()
        {
            ModelState.Remove("VerifyOtpVM.Otp");
            ModelState.Remove("ChangePasswordVM.NewPassword");
            ModelState.Remove("ChangePasswordVM.ConfirmPassword");
            if (!ModelState.IsValid)
            {
                Step = 1;
                
                return Page();
            }

            var result = await _userService.SendOtpAsync(HttpContext, SendOtpVM.Email);
            if (result.Data)
            {
                TempData["ToastMessage"] = result.Message;
                TempData["ToastType"] = "success";
                Step = 2;
            }
            else
            {
                ModelState.AddModelError("SendOtpVM.Email", result.Message);
                Step = 1;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostVerifyOtp()
        {
            ModelState.Remove("SendOtpVM.Email");
            ModelState.Remove("ChangePasswordVM.NewPassword");
            ModelState.Remove("ChangePasswordVM.ConfirmPassword");
            if (!ModelState.IsValid)
            {
                Step = 2;
                return Page();
            }

            var isValid = await _userService.VerifyOtp(HttpContext, VerifyOtpVM.Otp);
            if (isValid)
            {
                TempData["ToastMessage"] = "OTP verified. Please set your new password.";
                TempData["ToastType"] = "success";
                Step = 3;
            }
            else
            {
                ModelState.AddModelError("VerifyOtpVM.Otp", "Incorrect OTP.");
                Step = 2;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostChangePassword()
        {
            ModelState.Remove("VerifyOtpVM.Otp");
            ModelState.Remove("SendOtpVM.Email");

            if (!ModelState.IsValid)
            {
                Step = 3;
                return Page();
            }

            var email = HttpContext.Session.GetString("OTP_EMAIL");
            var result = await _userService.ChangePassword(email, ChangePasswordVM.NewPassword, ChangePasswordVM.ConfirmPassword);
            if (result.Data)
            {
                TempData["ToastMessage"] = result.Message;
                TempData["ToastType"] = "success";
                _userService.ClearOtpSession(HttpContext);
                return RedirectToPage("/Auth/Authentication");
            }
            else
            {
                ModelState.AddModelError("ChangePasswordVM.NewPassword", result.Message);
                Step = 3;
            }
            return Page();
        }
    }
}