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
        private readonly IFavoriteService _favoritePlantService;

        public DetailModel(
            ILogger<DetailModel> logger,
            IPlantService plantService,
            IViewLogService viewLogService,
            IPlantReviewService plantReviewService,
            IFavoriteService favoritePlantService)
        {
            _logger = logger;
            _plantService = plantService;
            _viewLogService = viewLogService;
            _plantReviewService = plantReviewService;
            _favoritePlantService = favoritePlantService;
        }

        public PlantDetailDTO? Plants { get; set; } = new();

        public RatingSummaryDTO? RatingSummary { get; set; }
        public List<ReviewDTO>? Reviews { get; set; }

        public ReviewDTO? UserReview { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public CreateReviewDTO AddReview { get; set; } = new();

        [BindProperty]
        public UpdateReviewDTO UpdateReview { get; set; } = new();

        public bool IsFavorited { get; set; } = false;

        public int? CurrentUserId { get; set; }


        public async Task<IActionResult> OnGetAsync(int id)
        {
            await _viewLogService.AddPlantViewLogAsync(id, User.Identity.IsAuthenticated ? GetUserId() : (int?)null);
            CurrentUserId = GetUserId();
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

            // Lấy review của user (nếu đã đánh giá)
            if (User.Identity.IsAuthenticated)
            {
                int userId = GetUserId() ?? 0;
                var userReviewResult = await _plantReviewService.GetUserReviewAsync(id, userId);
                UserReview = userReviewResult.Data;

                var favoriteResult = await _favoritePlantService.IsFavoriteAsync(userId, id);
                IsFavorited = favoriteResult;
            }
            return Page();
        }
        public async Task<IActionResult> OnPostToggleFavoriteAsync()
        {
            int userId = GetUserId() ?? 0;

            var checkResult = await _favoritePlantService.IsFavoriteAsync(userId, Id);

            bool isFavorited;
            if (checkResult)
            {
                await _favoritePlantService.RemoveFavoriteAsync(userId, Id);
                isFavorited = false;
            }
            else
            {
                await _favoritePlantService.AddFavoriteAsync(userId, Id);
                isFavorited = true;
            }

            return new JsonResult(new { isFavorited });
        }

        public async Task<IActionResult> OnPostAddReviewAsync()
        {
            AddReview.PlantId = Id;
            int userId = GetUserId() ?? 0;
            var result = await _plantReviewService.AddReviewAsync(userId, AddReview);
            _logger.LogInformation("User {UserId} added review for Plant {PlantId}", userId, Id);
            _logger.LogInformation("AddReviewAsync result: {Result}", AddReview.Rating);
            _logger.LogError("AddReviewAsync result: {Result}", result.Data);
            // TempData["ToastMessage"] = result.Message;
            // TempData["ToastType"] = result.Success ? "success" : "danger";
            // return RedirectToPage(new { id = Id });
            return new JsonResult(new
            {
                success = result.Success,
                message = result.Message,
                toastType = result.Success ? "success" : "danger"
            }); 
        }


        public async Task<IActionResult> OnPostUpdateReviewAsync()
        {
            int userId = GetUserId() ?? 0;
            UpdateReview.PlantId = Id;
            var result = await _plantReviewService.UpdateReviewAsync(userId, UpdateReview);

            // Có thể bỏ TempData nếu không dùng cho AJAX
            // TempData["ToastMessage"] = result.Message;
            // TempData["ToastType"] = result.Success ? "success" : "danger";

            // Trả về dữ liệu JSON cho client
            return new JsonResult(new
            {
                success = result.Success,
                message = result.Message,
                toastType = result.Success ? "success" : "danger"
            });
        }


        private int? GetUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                return userId;
            return null;
        }
    }
}