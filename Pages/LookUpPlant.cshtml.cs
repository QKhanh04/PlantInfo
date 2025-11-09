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
    [ValidateAntiForgeryToken]
    public class LookUpPlantModel : PageModel
    {
        private readonly ILogger<LookUpPlantModel> _logger;
        private readonly IPlantService _plantService;
        private readonly ICategoryService _categoryService;
        private readonly IUseService _useService;
        private readonly ISpeciesService _speciesService;
        private readonly IFavoriteService _favoriteService;
        private readonly ISearchLogService _searchLogService;



        public LookUpPlantModel(ILogger<LookUpPlantModel> logger,
        IPlantService plantService,
        ICategoryService categoryService,
        ISpeciesService speciesService,
        IFavoriteService favoriteService,
        IUseService useService,
        ISearchLogService searchLogService)
        {
            _logger = logger;
            _plantService = plantService;
            _categoryService = categoryService;
            _speciesService = speciesService;
            _favoriteService = favoriteService;
            _useService = useService;
            _searchLogService = searchLogService;
        }

        public PagedResult<PlantListDTO>? Plants { get; set; }
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        [BindProperty(SupportsGet = true)]
        public int TotalPages { get; set; }
        public const int pageSize = 12;


        [BindProperty(SupportsGet = true)]
        public FilterViewModel FilterVM { get; set; } = new();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public IEnumerable<Use> Uses { get; set; } = new List<Use>();

        public List<string> OrderList { get; set; } = new();

        public PlantDetailDTO? PlantExport { get; set; }
        // public bool IsFavorited { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Lấy userId (nếu đăng nhập)
            int? userId = null;
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int id)) userId = id;

            // Lấy danh sách cây đã yêu thích
            var favoritePlantIds = userId.HasValue
                ? await _favoriteService.GetFavoritePlantsAsync(userId.Value)
                : new List<PlantListDTO>();


            // Khi tạo PlantListDTO, thêm thuộc tính IsFavorited

            if (FilterVM.Keyword != null)
            {
                await _searchLogService.AddSearchLogAsync(FilterVM.Keyword, userId);
            }

            var result = await _plantService.GetPagedAsync(FilterVM?.Keyword, CurrentPage, pageSize, FilterVM?.CategoryIds, FilterVM?.UseIds, FilterVM?.DiseaseIds, FilterVM?.OrderName, null, null);
            var categories = await _categoryService.GetAllCategoryAsync();
            var use = await _useService.GetAllUsesAsync();
            OrderList = await _speciesService.GetDistinctOrderNameAsync();

            if (!result.Success || result.Data == null)
            {
                _logger.LogWarning("Lỗi khi lấy danh sách cây: {Message}", result.Message);
                TempData["ToastMessage"] = result.Message;
                TempData["ToastType"] = "danger";

                Plants = new PagedResult<PlantListDTO>
                {
                    Items = new List<PlantListDTO>(),
                    CurrentPage = CurrentPage,
                    PageSize = pageSize,
                    TotalItems = 0,
                };

                Categories = categories.Data ?? new List<Category>();
                Uses = use.Data ?? new List<Use>();
                foreach (var plant in Plants.Items)
                {
                    plant.IsFavorited = favoritePlantIds.Any(p => p.PlantId == plant.PlantId);
                }
                return Page();
            }

            Plants = result.Data;
            foreach (var plant in Plants.Items)
            {
                plant.IsFavorited = favoritePlantIds.Any(p => p.PlantId == plant.PlantId);
            }
            TotalPages = result.Data.TotalPages;
            Categories = categories.Data ?? new List<Category>();
            Uses = use.Data ?? new List<Use>();
            _logger.LogInformation("Tìm thấy {Count} cây.", result.Data.Items?.Count() ?? 0);

            return Page();
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