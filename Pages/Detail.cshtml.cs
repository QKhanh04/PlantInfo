using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PlantManagement.DTOs;
using PlantManagement.Services;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Pages
{

    public class DetailModel : PageModel
    {
        private readonly ILogger<DetailModel> _logger;
        private readonly IPlantService _plantService;
        private readonly IViewLogService _viewLogService;

        public DetailModel(ILogger<DetailModel> logger, IPlantService plantService, IViewLogService viewLogService)
        {
            _logger = logger;
            _plantService = plantService;
            _viewLogService = viewLogService;
        }

        public PlantDetailDTO? Plants { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Ghi nhận lượt xem
            await _viewLogService.AddPlantViewLogAsync(id, User.Identity.IsAuthenticated ? GetUserId() : (int?)null);

            // Lấy thông tin chi tiết cây
            var result = await _plantService.GetDetailPlantAsync(id);

            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Message;
                return Redirect("/Index");
            }

            Plants = result.Data;
            return Page();
        }

        // Hàm lấy userId, bạn cần tự cài đặt cho phù hợp với hệ thống
        private int? GetUserId()
        {
            // Ví dụ lấy userId từ claim, tuỳ hệ thống bạn có thể sửa lại
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                return userId;
            return null;
        }
    }
}
