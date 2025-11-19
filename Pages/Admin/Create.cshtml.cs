using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using PlantManagement.DTOs;
using PlantManagement.Models;
using PlantManagement.Repositories.Interfaces;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Pages.Admin
{

    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ILogger<CreateModel> _logger;
        private readonly IPlantService _plantService;
        private readonly ICategoryService _categoryService;
        private readonly IUseService _useService;
        private readonly ISpeciesService _speciesService;
        private readonly IDiseaseService _diseaseService;

        private readonly IMapper _mapper;

        public CreateModel(ILogger<CreateModel> logger,
        IPlantService plantService,
        ICategoryService categoryService,
        IUseService useService,
        ISpeciesService speciesService,
        IDiseaseService diseaseService,


        IMapper mapper)
        {
            _logger = logger;
            _plantService = plantService;
            _categoryService = categoryService;
            _speciesService = speciesService;
            _useService = useService;
            _diseaseService = diseaseService;

            _mapper = mapper;
        }

        [BindProperty]
        public PlantCreateDTO Plant { get; set; }
        public List<SelectListItem> CategoryList { get; set; } = new();
        public List<SelectListItem> UseList { get; set; } = new();
        public List<SelectListItem> SpeciesList { get; set; } = new();
        public List<SelectListItem> DiseaseList { get; set; } = new();

        public async Task OnGetAsync()
        {
            var categories = await _categoryService.GetAllCategoryAsync();
            CategoryList = categories.Data.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.CategoryName
            }).ToList();
            var uses = await _useService.GetAllUsesAsync();
            UseList = uses.Data.Select(u => new SelectListItem
            {
                Value = u.UseId.ToString(),
                Text = u.UseName
            }).ToList();
            var species = await _speciesService.GetAllSpeciesAsync();
            SpeciesList = species.Data.Select(s => new SelectListItem
            {
                Value = s.SpeciesId.ToString(),
                Text = s.ScientificName
            }).ToList();

            var disease = await _diseaseService.GetAllDiseasesAsync();
            DiseaseList = disease.Data.Select(s => new SelectListItem
            {
                Value = s.DiseaseId.ToString(),
                Text = s.DiseaseName
            }).ToList();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await OnGetAsync(); // reload select list
                    TempData["ToastMessage"] = "Dữ liệu nhập không hợp lệ. Vui lòng kiểm tra lại!";
                    TempData["ToastType"] = "danger";
                    foreach (var key in ModelState.Keys)
                    {
                        var errors = ModelState[key].Errors;
                        foreach (var error in errors)
                        {
                            _logger.LogWarning($"ModelState error for {key}: {error.ErrorMessage}");
                            TempData["ToastMessage"] = $"D{key}: {error.ErrorMessage}";
                            TempData["ToastType"] = "danger";
                        }
                    }
                    return Page();
                }

                if (Plant.ImageFiles != null && Plant.ImageFiles.Count > 0)
                {
                    var saveDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/plants");
                    if (!Directory.Exists(saveDir)) Directory.CreateDirectory(saveDir);

                    foreach (var file in Plant.ImageFiles)
                    {
                        if (file.Length > 0)
                        {
                            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                            var filePath = Path.Combine(saveDir, fileName);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                            if (Plant.Images == null) Plant.Images = new List<PlantImageDTO>();
                            Plant.Images.Add(new PlantImageDTO
                            {
                                ImageUrl = "/images/plants/" + fileName,
                                Caption = "", // lấy theo form nếu cần
                                IsPrimary = false // lấy theo form nếu cần
                            });
                        }
                    }
                }

                var result = await _plantService.CreatePlantAsync(Plant);

                if (result.Success)
                {
                    TempData["ToastMessage"] = result.Message ?? "Thêm cây thành công!";
                    TempData["ToastType"] = "success";
                    return RedirectToPage("/Admin/Dashboard");
                }

                TempData["ToastMessage"] = result.Message ?? "Thêm cây thất bại!";
                TempData["ToastType"] = "danger";
                await OnGetAsync();
                return Page();
            }
            catch (Exception ex)
            {
                // Nếu có logger, ghi lại chi tiết InnerException
                var inner = ex.InnerException;
                _logger?.LogError(ex, "Lỗi khi thêm cây trồng: {Message}. Inner: {Inner}", ex.Message, inner?.Message);

                TempData["ToastMessage"] = "Đã xảy ra lỗi khi lưu dữ liệu: " + (inner?.Message ?? ex.Message);
                TempData["ToastType"] = "danger";
                await OnGetAsync();
                return Page();
            }
        }
    }
}