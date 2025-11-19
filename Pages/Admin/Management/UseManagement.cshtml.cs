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
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Pages.Admin.Management
{
    [IgnoreAntiforgeryToken]
    [Authorize(Roles = "Admin")]
    public class UseManagementModel : PageModel
    {
        private readonly ILogger<UseManagementModel> _logger;
        private readonly IUseService _useService;

        public UseManagementModel(ILogger<UseManagementModel> logger, IUseService useService)
        {
            _logger = logger;
            _useService = useService;
        }

        public PagedResult<UseDTO> Uses { get; set; } = new();
        
        [BindProperty(SupportsGet = true)]
        public string Keyword { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        
        public int PageSize { get; set; } = 5;
        
        [BindProperty(SupportsGet = true)]
        public int TotalPages { get; set; }

        private async Task<PagedResult<UseDTO>> LoadPagedUsesAsync(string keyword, int currentPage)
        {
            var result = await _useService.GetPagedUsesAsync(keyword, currentPage, PageSize);

            if (!result.Success || result.Data == null)
            {
                result.Data = new PagedResult<UseDTO>
                {
                    Items = new List<UseDTO>(),
                    CurrentPage = currentPage,
                    PageSize = PageSize,
                    TotalItems = 0
                };
            }

            return result.Data;
        }

        public async Task OnGetAsync()
        {
            Uses = await LoadPagedUsesAsync(Keyword, CurrentPage);
            TotalPages = Uses.TotalPages;
        }

        public async Task<IActionResult> OnGetDetailAsync(int id)
        {
            var result = await _useService.GetByIdAsync(id);
            var uses = await _useService.GetPagedUsesAsync("", 1, PageSize);
            int totalPages = uses.Data?.TotalPages ?? 1;
            
            if (result.Success)
            {
                return new JsonResult(new
                {
                    success = true,
                    use = new
                    {
                        useId = result.Data.UseId,
                        useName = result.Data.UseName,
                        description = result.Data.Description
                    },
                    totalPages
                });
            }

            return new JsonResult(new { success = false });
        }

        public async Task<IActionResult> OnPostEditAsync([FromForm] EditUseRequest req)
        {
            ModelState.Remove("Keyword");

            if (!ModelState.IsValid)
            {
                _logger.LogError("ModelState không hợp lệ: {@ModelState}", ModelState);
                return new JsonResult(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var dto = new UseDTO
            {
                UseId = req.UseId,
                UseName = req.UseName,
                Description = req.Description
            };

            var result = await _useService.UpdateUseAsync(dto);
            
            if (result.Success)
                return new JsonResult(new { success = true, message = result.Message });
            
            return new JsonResult(new { success = false, message = result.Message });
        }

        public async Task<IActionResult> OnPostAddAsync([FromForm] AddUseRequest req)
        {
            ModelState.Remove("Keyword");

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
                return new JsonResult(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var dto = new UseDTO
            {
                UseName = req.UseName,
                Description = req.Description
            };

            var result = await _useService.CreateUseAsync(dto);
            
            if (result.Success)
            {
                return new JsonResult(new
                {
                    success = true,
                    use = new
                    {
                        useId = result.Data.UseId,
                        useName = result.Data.UseName,
                        description = result.Data.Description
                    },
                    message = result.Message
                });
            }
            
            return new JsonResult(new { success = false, message = result.Message });
        }

        public async Task<IActionResult> OnPostCheckBeforeDeleteAsync([FromBody] CheckDeleteRequest req)
        {
            int id = req.Id;
            var plants = await _useService.GetPlantsByUseIdAsync(id);

            if (plants == null || !plants.Any())
            {
                return new JsonResult(new
                {
                    success = true,
                    message = "Không có cây nào sử dụng công dụng này. Bạn có thể xóa an toàn."
                });
            }

            string plantListHtml = string.Join("", plants.Select(p =>
                $"<li>{p.CommonName ?? p.Species?.ScientificName ?? "Không rõ"}</li>"
            ));

            string html = $@"
                <p>Công dụng này đang được sử dụng bởi <strong>{plants.Count}</strong> cây:</p>
                <ul style='max-height: 200px; overflow-y: auto; padding-left: 20px;'>
                    {plantListHtml}
                </ul>";
            
            return new JsonResult(new
            {
                success = false,
                message = html
            });
        }

        public async Task<IActionResult> OnPostDeleteConfirmedAsync(int id)
        {
            var result = await _useService.DeleteUseAsync(id);

            var uses = await _useService.GetPagedUsesAsync("", 1, PageSize);
            int totalPages = uses.Data?.TotalPages ?? 1;

            return new JsonResult(new { success = result.Success, message = result.Message, totalPages });
        }

        public async Task<PartialViewResult> OnGetListAsync(string keyword, int currentPage = 1)
        {
            var data = await LoadPagedUsesAsync(keyword, currentPage);
            return Partial("Shared/_UseTableBody", data.Items);
        }
    }

    public class AddUseRequest
    {
        public string UseName { get; set; }
        public string? Description { get; set; }
    }

    public class EditUseRequest
    {
        public int UseId { get; set; }
        public string UseName { get; set; }
        public string? Description { get; set; }
    }

    public class CheckDeleteRequest
    {
        public int Id { get; set; }
    }
}