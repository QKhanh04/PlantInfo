using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PlantManagement.Common.Results;
using PlantManagement.DTOs;
using PlantManagement.Models;
using PlantManagement.Services.Interfaces;
using PlantManagement.ViewModel;

namespace PlantManagement.Pages
{
    public class LookUpPlantModel : PageModel
    {
        private readonly ILogger<LookUpPlantModel> _logger;
        private readonly IPlantService _plantService;
        private readonly ICategoryService _categoryService;
        private readonly IUseService _useService;
        private readonly ISpeciesService _speciesService;
        private readonly IFavoriteService _favoriteService;



        public LookUpPlantModel(ILogger<LookUpPlantModel> logger,
        IPlantService plantService,
        ICategoryService categoryService,
        ISpeciesService speciesService,
        IFavoriteService favoriteService,
        IUseService useService)
        {
            _logger = logger;
            _plantService = plantService;
            _categoryService = categoryService;
            _speciesService = speciesService;
            _favoriteService = favoriteService;
            _useService = useService;
        }

        public PagedResult<PlantListDTO>? Plants { get; set; }
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        [BindProperty(SupportsGet = true)]
        public int TotalPages { get; set; }
        public const int pageSize = 9;


        [BindProperty(SupportsGet = true)]
        public FilterViewModel FilterVM { get; set; } = new();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public IEnumerable<Use> Uses { get; set; } = new List<Use>();

        public List<string> OrderList { get; set; } = new();

        public PlantDetailDTO? PlantExport { get; set; }
        // public bool IsFavorited { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await _plantService.GetPagedAsync(FilterVM?.Keyword, CurrentPage, pageSize, FilterVM?.CategoryIds, FilterVM?.UseIds, FilterVM?.DiseaseIds, FilterVM?.OrderName);
            var categories = await _categoryService.GetAllCategoryAsync();
            var use = await _useService.GetAllUsesAsync();
            OrderList = await _speciesService.GetDistinctOrderNameAsync();

            if (!result.Success || result.Data == null)
            {
                _logger.LogWarning("Lỗi khi lấy danh sách cây: {Message}", result.Message);
                TempData["Error"] = result.Message;
                Plants = new PagedResult<PlantListDTO>
                {
                    Items = new List<PlantListDTO>(),
                    CurrentPage = CurrentPage,
                    PageSize = pageSize,
                    TotalItems = 0,
                    TotalPages = 0
                };
                Categories = categories.Data ?? new List<Category>();
                Uses = use.Data ?? new List<Use>();

                return Page();
            }

            Plants = result.Data;
            TotalPages = result.Data.TotalPages;
            Categories = categories.Data ?? new List<Category>();
            Uses = use.Data ?? new List<Use>();
            _logger.LogInformation("Tìm thấy {Count} cây.", result.Data.Items?.Count() ?? 0);

            return Page();
        }

        public async Task<IActionResult> OnGetPartialAsync(string? keyword,
        List<int>? categoryIds,
            List<int>? useIds,
            string? orderName,
            int currentPage = 1)
        {
            FilterVM = new FilterViewModel
            {
                Keyword = keyword,
                CategoryIds = categoryIds,
                UseIds = useIds,
                OrderName = orderName
            };
            CurrentPage = currentPage;

            await OnGetAsync();
            return Partial("_PlantListPartial", this);
        }


        public async Task<JsonResult> OnPostToggleFavoriteAsync(int plantId)
        {
            // Lấy userId từ claim hoặc session
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return new JsonResult(new { success = false, message = "Bạn cần đăng nhập để yêu thích!" });

            var isFavorite = await _favoriteService.IsFavoriteAsync(userId, plantId);

            if (isFavorite)
            {
                // Bỏ yêu thích
                var success = await _favoriteService.RemoveFavoriteAsync(userId, plantId);
                return new JsonResult(new { success = success, favorite = false });
            }
            else
            {
                // Thêm yêu thích
                var success = await _favoriteService.AddFavoriteAsync(userId, plantId);
                return new JsonResult(new { success = success, favorite = true });
            }

        }
    }
}