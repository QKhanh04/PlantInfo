using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PlantManagement.Data;
using PlantManagement.Services;
using PlantManagement.Services.Implementations;
using PlantManagement.Services.Interfaces;
using PlantManagement.ViewModel;

namespace PlantManagement.Pages
{
    public class AuthenticationModel : PageModel
    {
        private readonly IAuthService _userService;
        public AuthenticationModel(IAuthService user)
        {
            _userService = user;
        }
        [BindProperty]
        public LoginViewModel? LoginVM { get; set; }
        [BindProperty]
        public RegisterViewModel? RegisterVM { get; set; }
        public string ActiveTab { get; set; } = "login";
        public void OnGet() { }

        public async Task<IActionResult> OnPostLogin(string returnUrl = null)
        {
            ModelState.Clear();
            TryValidateModel(LoginVM, nameof(LoginVM));

            ActiveTab = "login";
            // LoginVM = loginVM;
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var user = await _userService.Login(LoginVM.UserNameOrEmail, LoginVM.Password);

            if (!user.Success)
            {
                if (user.Message.Contains("UserName"))
                    ModelState.AddModelError("LoginVM.UserNameOrEmail", user.Message);
                else if (user.Message.Contains("Password"))
                    ModelState.AddModelError("LoginVM.Password", user.Message);
                else
                {
                    TempData["ToastMessage"] = user.Message;
                    TempData["ToastType"] = "info";
                }

                return Page();
            }

            // Tạo claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Data.Username),
                new Claim(ClaimTypes.Role, user.Data.Role),
                new Claim(ClaimTypes.NameIdentifier, user.Data.UserId.ToString()),
                new Claim("UserId", user.Data.UserId.ToString())
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
                    });
            TempData["ToastMessage"] = "Đăng nhập thành công!";
            TempData["ToastType"] = "success";

            // Kiểm tra returnUrl có hợp lệ không
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // if (user.Data.Role == "Admin")
            //     return RedirectToPage("/Admin/Index");
            // else
            return RedirectToPage("/Index");
        }


        public async Task<IActionResult> OnPostRegister()
        {
            ModelState.Clear();
            TryValidateModel(RegisterVM, nameof(RegisterVM));

            ActiveTab = "register";
            // RegisterVM = registerViewModel;
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        // Xử lý hoặc log
                        Console.WriteLine($"Field: {key}, Error: {error.ErrorMessage}");
                    }
                }
                return Page();
            }

            var result = await _userService.Register(RegisterVM.Email, RegisterVM.Username, RegisterVM.Password, RegisterVM.ConfirmPassword);


            if (!result.Success)
            {
                ModelState.AddModelError("", RegisterVM.ConfirmPassword);

                if (result.Message.Contains("User"))
                    ModelState.AddModelError("RegisterVM.Username", result.Message);
                else if (result.Message.Contains("Email"))
                    ModelState.AddModelError("RegisterVM.Email", result.Message);
                else if (result.Message.Contains("password"))
                    ModelState.AddModelError("RegisterVM.ConfirmPassword", result.Message);
                else
                    ModelState.AddModelError(string.Empty, result.Message);

                return Page();
            }

            TempData["ToastMessage"] = "Đăng ký thành công!";
            TempData["ToastType"] = "success";
            return RedirectToPage("/Auth/Authentication");
        }

        public async Task<IActionResult> OnPostLogout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Auth/Authentication");
        }

    }
}