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

    public class SpeciesManagementModel : PageModel
    {
        private readonly ISpeciesService _speciesService;
        public SpeciesManagementModel(ISpeciesService service)
        {
            _speciesService = service;
        }

        public PagedResult<SpeciesDTO> Species { get; set; } = new();
        [BindProperty(SupportsGet = true)]
        public string Keyword { get; set; } = string.Empty;
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        [BindProperty(SupportsGet = true)]
        public int TotalPages { get; set; }

        private async Task<PagedResult<SpeciesDTO>> LoadPagedSpeciesAsync(string keyword, int currentPage)
        {
            var result = await _speciesService.GetPagedSpeciesAsync(keyword, currentPage, PageSize);

            if (!result.Success || result.Data == null)
            {
                result.Data = new PagedResult<SpeciesDTO>
                {
                    Items = new List<SpeciesDTO>(),
                    CurrentPage = currentPage,
                    PageSize = PageSize,
                    TotalItems = 0
                };
            }

            return result.Data;
        }

        public async Task OnGetAsync()
        {
            Species = await LoadPagedSpeciesAsync(Keyword, CurrentPage);
            TotalPages = Species.TotalPages;
        }

        public async Task<IActionResult> OnGetDetailAsync(int id)
        {
            var result = await _speciesService.GetByIdAsync(id);
            var species = await _speciesService.GetPagedSpeciesAsync("", 1, PageSize);
            int totalPages = species.Data?.TotalPages ?? 1;

            if (result.Success)
            {
                return new JsonResult(new
                {
                    success = true,
                    species = new
                    {
                        speciesId = result.Data.SpeciesId,
                        scientificName = result.Data.ScientificName,
                        genus = result.Data.Genus,
                        family = result.Data.Family,
                        orderName = result.Data.OrderName,
                        description = result.Data.Description
                    },
                    totalPages
                });
            }

            return new JsonResult(new { success = false });
        }


        public async Task<IActionResult> OnPostEditAsync([FromForm] EditSpeciesRequest req)
        {
            ModelState.Remove("Keyword");
            if (!ModelState.IsValid)
            {
                return new JsonResult(new { success = false, message = "Dữ liệu không hợp lệ" });
            }
            var oldResult = await _speciesService.GetByIdAsync(req.SpeciesId);
            if (!oldResult.Success || oldResult.Data == null)
                return new JsonResult(new { success = false, message = "Không tìm thấy loài!" });

            var dto = new SpeciesDTO
            {
                SpeciesId = req.SpeciesId,
                ScientificName = oldResult.Data.ScientificName, // Nếu không cho sửa tên khoa học
                Genus = req.Genus,
                Family = req.Family,
                OrderName = req.OrderName,
                Description = req.Description
            };
            var result = await _speciesService.UpdateSpeciesAsync(dto);
            if (result.Success)
                return new JsonResult(new { success = true });
            return new JsonResult(new { success = false, message = result.Message });
        }
        public async Task<IActionResult> OnPostAddAsync([FromForm] AddSpeciesRequest req)
        {
            ModelState.Remove("Keyword");

            if (!ModelState.IsValid)
            {
                return new JsonResult(new { success = false, message = "Dữ liệu không hợp lệ" });
            }
            var dto = new SpeciesDTO
            {
                ScientificName = req.ScientificName,
                Genus = req.Genus,
                Family = req.Family,
                OrderName = req.OrderName,
                Description = req.Description
            };
            var result = await _speciesService.CreateSpeciesAsync(dto);
            if (result.Success)
                return new JsonResult(new
                {
                    success = true,
                    species = new
                    {
                        speciesId = result.Data.SpeciesId,
                        scientificName = result.Data.ScientificName,
                        genus = result.Data.Genus,
                        family = result.Data.Family,
                        orderName = result.Data.OrderName,
                        description = result.Data.Description
                    }
                });
            return new JsonResult(new { success = false, message = result.Message });
        }

        public async Task<IActionResult> OnPostCheckBeforeDeleteAsync([FromBody] CheckSpeciesDeleteRequest req)
        {
            int id = req.Id;
            var plants = await _speciesService.GetPlantsBySpeciesIdAsync(id);

            if (plants == null || !plants.Any())
            {
                return new JsonResult(new
                {
                    success = true,
                    message = "Không có cây nào thuộc loài này. Bạn có thể xóa an toàn."
                });
            }

            string plantListHtml = string.Join("", plants.Select(p =>
                $"<li>{p.CommonName ?? "Không rõ"}</li>"
            ));

            string html = $@"
                <p>Loài này đang được sử dụng bởi <strong>{plants.Count}</strong> cây:</p>
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
            var result = await _speciesService.DeleteSpeciesAsync(id);

            var species = await _speciesService.GetPagedSpeciesAsync("", 1, PageSize);
            int totalPages = species.Data?.TotalPages ?? 1;

            return new JsonResult(new { success = result.Success, message = result.Message, totalPages });
        }

        public async Task<PartialViewResult> OnGetListAsync(string keyword, int currentPage = 1)
        {
            var data = await LoadPagedSpeciesAsync(keyword, currentPage);
            return Partial("Shared/_SpeciesTableBody", data.Items);
        }
    }

    public class EditSpeciesRequest
    {
        public int SpeciesId { get; set; }
        public string? Genus { get; set; }
        public string? Family { get; set; }
        public string? OrderName { get; set; }
        public string? Description { get; set; }
    }
    public class AddSpeciesRequest
    {
        public string? ScientificName { get; set; }
        public string? Genus { get; set; }
        public string? Family { get; set; }
        public string? OrderName { get; set; }
        public string? Description { get; set; }
    }


    public class CheckSpeciesDeleteRequest
    {
        public int Id { get; set; }
    }

}