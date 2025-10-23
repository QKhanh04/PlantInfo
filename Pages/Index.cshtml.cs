using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlantManagement.Common.Results;
using PlantManagement.DTOs;
using PlantManagement.Models;
using PlantManagement.Services.Interfaces;
using PlantManagement.ViewModel;

namespace PlantManagement.Pages
{

    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IPlantService _plantService;
        private readonly ICategoryService _categoryService;
        private readonly ISpeciesService _speciesService;
        private readonly IFavoriteService _favoriteService;



        public IndexModel(ILogger<IndexModel> logger,
        IPlantService plantService,
        ICategoryService categoryService,
        ISpeciesService speciesService,
        IFavoriteService favoriteService)
        {
            _logger = logger;
            _plantService = plantService;
            _categoryService = categoryService;
            _speciesService = speciesService;
            _favoriteService = favoriteService;
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
        public List<string> OrderList { get; set; } = new();

        public PlantDetailDTO? PlantExport { get; set; }
        // public bool IsFavorited { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // int? userId = null;
            // var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            // if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int id)) userId = id;
            // var favoritePlant = await _favoriteService.GetFavoritePlantsAsync(userId.Value);
            // if (userId.HasValue)
            // {
            //     ViewData["FavoriteCount"] = favoritePlant.Count;
            // }
            // else
            // {
            //     ViewData["FavoriteCount"] = 0;
            // }
            return Page();
        }

        public async Task<IActionResult> OnGetPartialAsync(string? keyword, List<int>? categoryIds, string? orderName, int currentPage = 1)
        {
            FilterVM = new FilterViewModel
            {
                Keyword = keyword,
                CategoryIds = categoryIds,
                OrderName = orderName
            };
            CurrentPage = currentPage;

            await OnGetAsync();
            return Partial("_PlantListPartial", this);
        }

        public async Task<ActionResult> OnPostToggleActive(int id)
        {
            var result = await _plantService.DeletePlantAsync(id);
            if (result.Success)
            {
                TempData["ToastType"] = "success";
                TempData["ToastMessage"] = result.Message;

            }
            else
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = result.Message;
            }
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
