using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PlantInformation.DTOs;
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
        private readonly IPlantReviewService _plantReviewService;
 
 
        public DetailModel(
         ILogger<DetailModel> logger,
         IPlantService plantService,
         IViewLogService viewLogService,
         IPlantReviewService plantReviewService)
        {
            _logger = logger;
            _plantService = plantService;
            _viewLogService = viewLogService;
            _plantReviewService = plantReviewService;
        }
 
        public PlantDetailDTO? Plants { get; set; } = new();
 
        public RatingSummaryDTO? RatingSummary { get; set; }
        public List<ReviewDTO>? Reviews { get; set; }
 
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }
 
        [BindProperty]
        public CreateReviewDTO NewReview { get; set; } = new();
 
        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Ghi nhận lượt xem
            await _viewLogService.AddPlantViewLogAsync(id, User.Identity.IsAuthenticated ? GetUserId() : (int?)null);
 
            // Lấy thông tin chi tiết cây
            var result = await _plantService.GetDetailPlantAsync(id);
 
            if (!result.Success || result.Data == null)
            {
                TempData["ToastMessage"] = result.Message;
                TempData["ToastType"] = "danger";
 
                return Redirect("/Index");
            }
 
            Plants = result.Data;
 
            var ratingResult = await _plantReviewService.GetRatingSummaryAsync(id);
            RatingSummary = ratingResult.Data;
 
            var reviewsResult = await _plantReviewService.GetVisibleReviewsByPlantIdAsync(id);
            Reviews = reviewsResult.Data ?? new List<ReviewDTO>();
            return Page();
        }
 
        public async Task<IActionResult> OnPostReviewAsync()
        {
            // if (!User.Identity.IsAuthenticated)
            // {
            //     ReviewMessage = "Bạn cần đăng nhập để bình luận!";
            //     return await OnGetAsync(Id);
            // }
 
            NewReview.PlantId = Id;
            int userId = GetUserId() ?? 0;
            var result = await _plantReviewService.AddOrUpdateReviewAsync(userId, NewReview);

            if (result.Success)
            {
                TempData["ToastMessage"] = "Cảm ơn bạn đã đánh giá!";
                TempData["ToastType"] = "success";
                return RedirectToPage(new { id = Id });
            }
            TempData["ToastMessage"] = result.Message;
            TempData["ToastType"] = "danger";
            return await OnGetAsync(Id);
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