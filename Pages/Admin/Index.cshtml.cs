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
using PlantManagement.Models;
using PlantManagement.Services;
using PlantManagement.Services.Interfaces;
using PlantManagement.ViewModel;

namespace PlantManagement.Pages.Admin
{
    [Authorize(Roles = "Admin")]
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

        public PagedResult<PlantListDTO>? Plants { get; set; }  = new();
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        [BindProperty(SupportsGet = true)]
        public int TotalPages { get; set; }
        public const int pageSize = 12;


        [BindProperty(SupportsGet = true)]
        public FilterViewModel? FilterVM { get; set; }
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public List<string> OrderList { get; set; } = new();

        public PlantDetailDTO? PlantExport { get; set; }


        public async Task<IActionResult> OnGetAsync()
        {
            var result = await _plantService.GetPagedAsync(FilterVM?.Keyword, CurrentPage, pageSize, FilterVM?.CategoryId, FilterVM?.OrderName);
            var categories = await _categoryService.GetAllCategoryAsync();
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
                return Page();
            }

            Plants = result.Data;
            TotalPages = result.Data.TotalPages;
            Categories = categories.Data ?? new List<Category>();

            _logger.LogInformation("Tìm thấy {Count} cây.", result.Data.Items?.Count() ?? 0);

            return Page();
        }

        public async Task<IActionResult> OnGetPartialAsync(string? keyword, int? categoryId, string? orderName, int currentPage = 1)
        {
            FilterVM = new FilterViewModel
            {
                Keyword = keyword,
                CategoryId = categoryId,
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
        
    }
}