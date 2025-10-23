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

namespace PlantManagement.Pages.Admin.Management
{
    [IgnoreAntiforgeryToken]

    public class CategoryManagementModel : PageModel
    {
        private readonly ILogger<CategoryManagementModel> _logger;
        private readonly ICategoryService _categoryService;

        public CategoryManagementModel(ILogger<CategoryManagementModel> logger, ICategoryService categoryService)
        {
            _logger = logger;
            _categoryService = categoryService;
        }

        public PagedResult<CategoryDTO> Categories { get; set; } = new();
        [BindProperty(SupportsGet = true)]
        public string Keyword { get; set; }
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        [BindProperty(SupportsGet = true)]
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            var result = await _categoryService.GetPagedCategoriesAsync(Keyword, CurrentPage, PageSize);
            if (!result.Success || result.Data == null)
            {
                Categories = new PagedResult<CategoryDTO>
                {
                    Items = new List<CategoryDTO>(),
                    CurrentPage = CurrentPage,
                    PageSize = PageSize,
                    TotalItems = 0,
                };
            }
            else
            {
                Categories = result.Data;
            }

            TotalPages = Categories.TotalPages;
        }

        public async Task<IActionResult> OnGetDetailAsync(int id)
        {
            var result = await _categoryService.GetByIdAsync(id);
            if (result.Success)
            {
                return new JsonResult(new
                {
                    success = true,
                    category = new
                    {
                        categoryId = result.Data.CategoryId,
                        categoryName = result.Data.CategoryName,
                        description = result.Data.Description
                    }
                });
            }
            return new JsonResult(new { success = false });
        }

        public async Task<IActionResult> OnPostEditAsync([FromForm] EditCategoryRequest req)
        {
            ModelState.Remove("Keyword");

            if (!ModelState.IsValid)
            {
                _logger.LogError("ModelState không hợp lệ: {@ModelState}", ModelState);
                return new JsonResult(new { success = false, message = "Dữ liệu không hợp lệ" });
            }
            _logger.LogInformation("Đã vào handler OnPostEditAsync");
            var oldResult = await _categoryService.GetByIdAsync(req.CategoryId);
            if (!oldResult.Success || oldResult.Data == null)
                return new JsonResult(new { success = false, message = "Không tìm thấy danh mục!" });

            var dto = new CategoryDTO
            {
                CategoryId = req.CategoryId,
                CategoryName = oldResult.Data.CategoryName,
                Description = req.Description
            };
            var result = await _categoryService.UpdateCategoryAsync(dto);
            _logger.LogInformation("CategoryId: {CategoryId}, Description: {Description}", req?.CategoryId, req?.Description);
            if (result.Success)
                return new JsonResult(new { success = true });
            return new JsonResult(new { success = false, message = result.Message });
        }

    }

    public class EditCategoryRequest
    {
        public int CategoryId { get; set; }
        public string Description { get; set; }
    }
}