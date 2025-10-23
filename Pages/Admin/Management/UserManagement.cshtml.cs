using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PlantManagement.Common.Results;
using PlantManagement.DTOs;
using PlantManagement.Services.Implementations;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Pages.Admin.Management
{
    [Authorize(Roles = "Admin")]
    public class UserManagementModel : PageModel
    {
        private readonly IAuthService _userService;

        public UserManagementModel(IAuthService userService)
        {
            _userService = userService;
        }

        public PagedResult<UserDTO> Users { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 3;
        [BindProperty(SupportsGet = true)]
        public int TotalPages { get; set; }


        [BindProperty(SupportsGet = true)]
        public string Status { get; set; }

        public int? CurrentUserId { get; set; }

        public async Task OnGetAsync()
        {
            CurrentUserId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var userId)
                ? userId : (int?)null;

            bool? isLocked = null;
            if (Status == "locked") isLocked = true;
            else if (Status == "active") isLocked = false;

            var result = await _userService.SearchUsersAsync(SearchTerm, CurrentPage, PageSize, null, isLocked);

            if (!result.Success || result.Data == null)
            {
                Users = new PagedResult<UserDTO>
                {
                    Items = new List<UserDTO>(),
                    CurrentPage = CurrentPage,
                    PageSize = PageSize,
                    TotalItems = 0,
                };
            }
            else
            {
                Users = result.Data;
            }

            TotalPages = Users.TotalPages;
        }

        // Xử lý khóa/mở khóa (toggle)
        public async Task<IActionResult> OnPostToggleLockAsync(int id)
        {
            var result = await _userService.ToggleLockUserAsync(id);

            TempData["ToastMessage"] = result.Message;
            TempData["ToastType"] = "success";

            // Quay về cùng tham số tìm kiếm/phân trang hiện tại
            return RedirectToPage(new { SearchTerm, CurrentPage, PageSize, Status });
        }
    }
}