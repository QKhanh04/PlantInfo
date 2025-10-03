using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlantManagement.Common.Results;
using PlantManagement.DTOs;
using PlantManagement.Models;
using PlantManagement.Services.Interfaces;
using PlantManagement.ViewModel;

namespace PlantManagement.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
        private readonly IPlantService _plantService;
        private readonly ICategoryService _categoryService;
        private readonly ISpeciesService _speciesService;

        public IndexModel(ILogger<IndexModel> logger, IPlantService plantService, ICategoryService categoryService, ISpeciesService speciesService)
        {
            _logger = logger;
            _plantService = plantService;
            _categoryService = categoryService;
            _speciesService = speciesService;
        }

        public PagedResult<PlantListDTO>? Plants { get; set; }
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        [BindProperty(SupportsGet = true)]
        public int TotalPages { get; set; }
        public const int pageSize = 9;

        [BindProperty(SupportsGet = true)]
        public FilterViewModel FilterVM { get; set; }
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public List<string> OrderList { get; set; } = new List<string>();


        public async Task<IActionResult> OnGetAsync()
        {
            
            var result = await _plantService.GetPagedAsync(FilterVM.Keyword, CurrentPage, pageSize,
            FilterVM.CategoryId, FilterVM.OrderName);
            var categories = await _categoryService.GetAllCategories();
            OrderList = await _speciesService.GetDistinctOrderNameAsync();
            _logger.LogWarning("Lỗi khi lấy danh sách cây: {Message}", result.Data.Items);
            if (!result.Success)
            {

                TempData["Error"] = result.Message;
                return Page();
            }
            Plants = result.Data;
            TotalPages = result.Data.TotalPages;

            Categories = categories.Data;
            return Page();
        }

        public async Task<PartialViewResult> OnGetPartialAsync(string keyword, int? categoryId, string orderName, int currentPage = 1)
        {
            var result = await _plantService.GetPagedAsync(keyword, currentPage, pageSize, categoryId, orderName);

            if (!result.Success)
            {
                Plants = new PagedResult<PlantListDTO>
                {
                    Items = new List<PlantListDTO>(),
                    CurrentPage = 1,
                    TotalPages = 0
                };
            }
            else
            {
                Plants = result.Data;
                TotalPages = result.Data.TotalPages;
                CurrentPage = result.Data.CurrentPage;
            }

            return Partial("_PlantListPartial", this);
        }


}
