using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PlantManagement.Common.Results;
using PlantManagement.Models;
using PlantManagement.Services;
using PlantManagement.ViewModel;
 
namespace PlantManagement.Pages.Admin
{
    // [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IPlantService _plantService;
        private readonly ICategoryService _categoryService;
 
        public IndexModel(ILogger<IndexModel> logger, IPlantService plantService, ICategoryService categoryService)
        {
            _logger = logger;
            _plantService = plantService;
            _categoryService = categoryService;
        }
 
        public PagedResult<Plant>? Plants { get; set; }
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        [BindProperty(SupportsGet = true)]
        public int TotalPages { get; set; }
        public const int pageSize = 9;
 
        [BindProperty(SupportsGet = true)]
        public FilterViewModel FilterVM { get; set; }
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public List<Use> Uses { get; set; } = new();
        public List<Disease> Diseases { get; set; } = new();
 
 
        public async Task<IActionResult> OnGetAsync()
        {
            var result = await _plantService.GetPagedAsync(FilterVM.Keyword, CurrentPage, pageSize,
            FilterVM.CategoryId, FilterVM.UseId, FilterVM.DiseaseId);
            var categories = await _categoryService.GetAllCategories();
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
    }
}