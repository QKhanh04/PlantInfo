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
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Pages.Admin.Management
{
    [IgnoreAntiforgeryToken]
    [Authorize(Roles = "Admin")]
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
        private async Task<PagedResult<CategoryDTO>> LoadPagedCategoriesAsync(string keyword, int currentPage)
        {
            var result = await _categoryService.GetPagedCategoriesAsync(keyword, currentPage, PageSize);

            if (!result.Success || result.Data == null)
            {
                result.Data = new PagedResult<CategoryDTO>
                {
                    Items = new List<CategoryDTO>(),
                    CurrentPage = currentPage,
                    PageSize = PageSize,
                    TotalItems = 0
                };
            }

            return result.Data;
        }
        public async Task OnGetAsync()
        {
            Categories = await LoadPagedCategoriesAsync(Keyword, CurrentPage);
            TotalPages = Categories.TotalPages;
        }

        public async Task<IActionResult> OnGetDetailAsync(int id)
        {
            var result = await _categoryService.GetByIdAsync(id);
            var categories = await _categoryService.GetPagedCategoriesAsync("", 1, PageSize);
            int totalPages = categories.Data?.TotalPages ?? 1;
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
                    },
                    totalPages
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
                return new JsonResult(new { success = true, message = result.Message });
            return new JsonResult(new { success = false, message = result.Message });
        }

        public async Task<IActionResult> OnPostAddAsync([FromForm] AddCategoryRequest req)
        {
            ModelState.Remove("Keyword");

            // Kiểm tra dữ liệu đầu vào
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        _logger.LogError("Field: {Field}, Error: {Error}", key, error.ErrorMessage);
                    }
                }
                // Luôn trả về JSON! (tránh lỗi JS như ảnh)
                return new JsonResult(new { success = false, message = "Invalid input data." });
            }

            var dto = new CategoryDTO
            {
                CategoryName = req.CategoryName,
                Description = req.Description
            };

            var result = await _categoryService.CreateCategoryAsync(dto);
            if (result.Success)
            {
                // Trả về thông tin danh mục vừa tạo (bạn có thể trả về nhiều trường hơn nếu muốn)
                return new JsonResult(new
                {
                    success = true,
                    category = new
                    {
                        categoryId = result.Data.CategoryId,
                        categoryName = result.Data.CategoryName,
                        description = result.Data.Description
                    },
                    message = result.Message
                });
            }
            // Nếu lỗi (ví dụ trùng tên), trả về thông báo chi tiết
            return new JsonResult(new { success = false, message = result.Message });
        }


        public async Task<IActionResult> OnPostCheckBeforeDeleteAsync([FromBody] CheckCategoryDeleteRequest req)
        {
            int id = req.Id;
            var plants = await _categoryService.GetPlantsByCategoryIdAsync(id);

            if (plants == null || !plants.Any())
            {
                return new JsonResult(new
                {
                    success = true,
                    message = "Không có cây nào sử dụng danh mục này. Bạn có thể xóa an toàn."
                });
            }

            // Ghép tên cây (ưu tiên CommonName, fallback ScientificName)
            // Ghép danh sách cây thành <li>
            string plantListHtml = string.Join("", plants.Select(p =>
                $"<li>{p.CommonName ?? p.Species?.ScientificName ?? "Không rõ"}</li>"
            ));

            string html = $@"
    <p>Danh mục này đang được sử dụng bởi <strong>{plants.Count}</strong> cây:</p>
    <ul style='max-height: 200px; overflow-y: auto; padding-left: 20px;'>
        {plantListHtml}
    </ul>
";
            return new JsonResult(new
            {
                success = false,
                message = html
            });
        }
        public async Task<IActionResult> OnPostDeleteConfirmedAsync(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);

            // Sau khi xóa, lấy lại tổng số trang
            var categories = await _categoryService.GetPagedCategoriesAsync("", 1, PageSize);
            int totalPages = categories.Data?.TotalPages ?? 1;

            return new JsonResult(new { success = result.Success, message = result.Message, totalPages });
        }
        public async Task<PartialViewResult> OnGetListAsync(string keyword, int currentPage = 1)
        {
            var data = await LoadPagedCategoriesAsync(keyword, currentPage);
            return Partial("Shared/_CategoryTableBody", data.Items);
        }

    }
    public class AddCategoryRequest
    {
        public string CategoryName { get; set; }
        public string Description { get; set; }
    }
    public class EditCategoryRequest
    {
        public int CategoryId { get; set; }
        public string Description { get; set; }
    }
    public class CheckCategoryDeleteRequest
    {
        public int Id { get; set; }
    }
}