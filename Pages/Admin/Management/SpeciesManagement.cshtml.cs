using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PlantManagement.Common.Results;
using PlantManagement.DTOs;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Pages.Admin.Management
{
    [IgnoreAntiforgeryToken]
    public class SpeciesManagementModel : PageModel
    {
        private readonly ISpeciesService _speciesService;
        public SpeciesManagementModel(ISpeciesService service)
        {
            _speciesService = service;
        }

        public PagedResult<SpeciesDTO> SpeciesList { get; set; } = new();
        [BindProperty(SupportsGet = true)]
        public string Keyword { get; set; }
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        [BindProperty(SupportsGet = true)]
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            var result = await _speciesService.GetPagedSpeciesAsync(Keyword, CurrentPage, PageSize);
            SpeciesList = result.Data ?? new PagedResult<SpeciesDTO>
            {
                Items = new List<SpeciesDTO>(),
                CurrentPage = CurrentPage,
                PageSize = PageSize,
                TotalItems = 0,
            };
            TotalPages = SpeciesList.TotalPages;
        }

        public async Task<IActionResult> OnGetDetailAsync(int id)
        {
            var result = await _speciesService.GetByIdAsync(id);
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
                    }
                });
            }
            return new JsonResult(new { success = false, message = "Không tìm thấy loài!" });
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
    }
}