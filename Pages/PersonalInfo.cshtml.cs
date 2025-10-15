using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PlantManagement.DTOs;
using PlantManagement.Repositories.Interfaces;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Pages
{
    public class PersonalInfoModel : PageModel
    {
        private readonly IAuthService _userService;
        private readonly IAuthRepository _userRepository;


        public PersonalInfoModel(IAuthService userService, IAuthRepository userRepository)
        {
            _userService = userService;
            _userRepository = userRepository;
        }

        [BindProperty]
        public UpdateUserDTO UserDTO { get; set; } = new();

        public string? UserName { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                TempData["ToastMessage"] = "Không tìm thấy người dùng.";
                TempData["ToastType"] = "danger";
                return RedirectToPage("/Index");
            }

            UserDTO.UserId = user.UserId;
            UserDTO.Email = user.Email;
            UserName = user.Username; // hiển thị nhưng không sửa

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await _userService.UpdateUserAsync(UserDTO);

            if (result.Success)
            {
                TempData["ToastMessage"] = result.Message;
                TempData["ToastType"] = "success";
                return RedirectToPage("/Index", new { id = UserDTO.UserId });
            }
            else
            {

                ModelState.AddModelError("UserDTO.Email", result.Message);
                return Page();
            }
        }
    }
}