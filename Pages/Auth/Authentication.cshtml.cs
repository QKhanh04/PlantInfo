using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PlantManagement.Services;
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
 
        public async Task<IActionResult> OnPostLogin()
        {
            ModelState.Clear();
            TryValidateModel(LoginVM, nameof(LoginVM));
 
            ActiveTab = "login";
            // LoginVM = loginVM;
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var user = await _userService.Login(LoginVM.UsernameOrEmail, LoginVM.Password);
 
            if (!user.Success) // TODO: hash password
            {
                if (user.Message.Contains("UserName"))
                    ModelState.AddModelError("LoginVM.UserNameOrEmail", user.Message);
                else
                    ModelState.AddModelError("LoginVM.Password", user.Message);
                return Page();
            }
 
            // Tạo claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Data.Username),
                new Claim(ClaimTypes.Role, user.Data.Role)
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
            TempData["SuccessMessage"] = "Đăng nhập thành công!";
            if (user.Data.Role == "Admin")
                return RedirectToPage("/Admin/Index");
            else
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
                ModelState.AddModelError(string.Empty, "Vui lòng kiểm tra lại thông tin.");
                return Page();
            }
 
            var result = await _userService.Register(RegisterVM.Username, RegisterVM.Email, RegisterVM.Password, RegisterVM.ConfirmPassword);
 
 
            if (!result.Success)
            {
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
 
            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToPage("/Auth/Authentication");
        }
 
        public async Task<IActionResult> OnPostLogout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Auth/Authentication");
        }
 
    }
}