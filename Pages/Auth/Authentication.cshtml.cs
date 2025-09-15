using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlantManagement.Services;
using PlantManagement.ViewModel;

namespace PlantManagement.Pages.Auth
{
    public class AuthenticationModel : PageModel
    {
        private readonly AuthService _authService;
        public AuthenticationModel(AuthService authService)
        {
            _authService = authService;
        }
        [BindProperty]
        public LoginViewModel LoginVM { get; set; }
        [BindProperty]
        public RegisterViewModel RegisterVM { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync([FromForm] LoginViewModel LoginVM)
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _authService.Login(LoginVM.UsernameOrEmail, LoginVM.Password);

            if (user == null) // TODO: hash password
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            // Tạo claims
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
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

            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostRegister([FromForm] RegisterViewModel RegisterVM)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _authService.Register(RegisterVM.Username, RegisterVM.Email, RegisterVM.Password);


            if (!result.Success)
            {
                if (result.Message.Contains("Username"))
                    ModelState.AddModelError("Input.Username", result.Message);
                else if (result.Message.Contains("Email"))
                    ModelState.AddModelError("Input.Email", result.Message);
                else
                    ModelState.AddModelError(string.Empty, result.Message);

                return Page();
            }

            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToPage("/Login");
        }
    }
}