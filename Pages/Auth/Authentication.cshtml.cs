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

        public LoginViewModel LoginVM { get; set; }
        public RegisterViewModel RegisterVM { get; set; }



        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
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

            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostRegister()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Kiểm tra username trùng
            var existingUser = await _authService.Users
                .FirstOrDefaultAsync(u => u.Username == Input.Username);

            if (existingUser != null)
            {
                ModelState.AddModelError("Input.Username", "Username already exists");
                return Page();
            }

            // Kiểm tra email trùng
            var existingEmail = await _authService.Users
                .FirstOrDefaultAsync(u => u.Email == Input.Email);

            if (existingEmail != null)
            {
                ModelState.AddModelError("Input.Email", "Email already exists");
                return Page();
            }

            // Hash password với BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(Input.Password);

            var newUser = new User
            {
                Username = Input.Username,
                Email = Input.Email,
                PasswordHash = hashedPassword,
                Role = "User", // mặc định role User
                CreatedAt = DateTime.UtcNow
            };

            _authService.Users.Add(newUser);
            await _authService.SaveChangesAsync();

            // Sau khi đăng ký thành công → redirect sang Login
            return RedirectToPage("/Login");
        }
    }
}