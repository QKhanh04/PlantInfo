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
    public class DiseasesManagementModel : PageModel
    {
        private readonly ILogger<DiseasesManagementModel> _logger;
        private readonly IDiseaseService _diseaseService;

        public DiseasesManagementModel(ILogger<DiseasesManagementModel> logger, IDiseaseService diseaseService)
        {
            _logger = logger;
            _diseaseService = diseaseService;
        }

        public PagedResult<DiseaseDTO> Diseases { get; set; } = new();
        public List<PlantDTO> Plants { get; set; } = new();
        
        [BindProperty(SupportsGet = true)]
        public string Keyword { get; set; } = string.Empty;
        
        [BindProperty(SupportsGet = true)]
        public int? PlantId { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        
        public int PageSize { get; set; } = 5;
        
        [BindProperty(SupportsGet = true)]
        public int TotalPages { get; set; }

        private async Task<PagedResult<DiseaseDTO>> LoadPagedDiseasesAsync(string keyword, int? plantId, int currentPage)
        {
            var result = await _diseaseService.GetPagedDiseasesAsync(keyword, plantId, currentPage, PageSize);

            if (!result.Success || result.Data == null)
            {
                result.Data = new PagedResult<DiseaseDTO>
                {
                    Items = new List<DiseaseDTO>(),
                    CurrentPage = currentPage,
                    PageSize = PageSize,
                    TotalItems = 0
                };
            }

            return result.Data;
        }

        public async Task OnGetAsync()
        {
            Diseases = await LoadPagedDiseasesAsync(Keyword, PlantId, CurrentPage);
            TotalPages = Diseases.TotalPages;
            
            // Load danh sách plants cho dropdown
            var plantsResult = await _diseaseService.GetAllPlantsForDropdownAsync();
            Plants = plantsResult.Data?.ToList() ?? new List<PlantDTO>();
        }

        public async Task<IActionResult> OnGetDetailAsync(int id)
        {
            var result = await _diseaseService.GetByIdAsync(id);
            var diseases = await _diseaseService.GetPagedDiseasesAsync("", null, 1, PageSize);
            int totalPages = diseases.Data?.TotalPages ?? 1;
            
            if (result.Success)
            {
                return new JsonResult(new
                {
                    success = true,
                    disease = new
                    {
                        diseaseId = result.Data.DiseaseId,
                        plantId = result.Data.PlantId,
                        diseaseName = result.Data.DiseaseName,
                        symptoms = result.Data.Symptoms,
                        treatment = result.Data.Treatment
                    },
                    totalPages
                });
            }

            return new JsonResult(new { success = false });
        }

        public async Task<IActionResult> OnPostEditAsync([FromForm] EditDiseaseRequest req)
        {
            ModelState.Remove("Keyword");

            if (!ModelState.IsValid)
            {
                _logger.LogError("ModelState không hợp lệ: {@ModelState}", ModelState);
                return new JsonResult(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var dto = new DiseaseDTO
            {
                DiseaseId = req.DiseaseId,
                PlantId = req.PlantId,
                DiseaseName = req.DiseaseName,
                Symptoms = req.Symptoms,
                Treatment = req.Treatment
            };

            var result = await _diseaseService.UpdateDiseaseAsync(dto);
            
            if (result.Success)
                return new JsonResult(new { success = true, message = result.Message });
            
            return new JsonResult(new { success = false, message = result.Message });
        }

        public async Task<IActionResult> OnPostAddAsync([FromForm] AddDiseaseRequest req)
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

            var dto = new DiseaseDTO
            {
                PlantId = req.PlantId,
                DiseaseName = req.DiseaseName,
                Symptoms = req.Symptoms,
                Treatment = req.Treatment
            };

            var result = await _diseaseService.CreateDiseaseAsync(dto);
            
            if (result.Success)
            {
                return new JsonResult(new
                {
                    success = true,
                    disease = new
                    {
                        diseaseId = result.Data.DiseaseId,
                        plantId = result.Data.PlantId,
                        diseaseName = result.Data.DiseaseName,
                        symptoms = result.Data.Symptoms,
                        treatment = result.Data.Treatment
                    },
                    message = result.Message
                });
            }
            
            return new JsonResult(new { success = false, message = result.Message });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var result = await _diseaseService.DeleteDiseaseAsync(id);

            var diseases = await _diseaseService.GetPagedDiseasesAsync("", null, 1, PageSize);
            int totalPages = diseases.Data?.TotalPages ?? 1;

            return new JsonResult(new { success = result.Success, message = result.Message, totalPages });
        }

        public async Task<PartialViewResult> OnGetListAsync(string keyword, int? plantId, int currentPage = 1)
        {
            var data = await LoadPagedDiseasesAsync(keyword, plantId, currentPage);
            return Partial("Shared/_DiseasesTableBody", data.Items);
        }

        public async Task<IActionResult> OnGetPlantsAsync()
        {
            var result = await _diseaseService.GetAllPlantsForDropdownAsync();
            if (result.Success)
            {
                return new JsonResult(new { success = true, plants = result.Data });
            }
            return new JsonResult(new { success = false });
        }
    }

    public class AddDiseaseRequest
    {
        public int PlantId { get; set; }
        public string DiseaseName { get; set; }
        public string? Symptoms { get; set; }
        public string? Treatment { get; set; }
    }

    public class EditDiseaseRequest
    {
        public int DiseaseId { get; set; }
        public int PlantId { get; set; }
        public string DiseaseName { get; set; }
        public string? Symptoms { get; set; }
        public string? Treatment { get; set; }
    }
}