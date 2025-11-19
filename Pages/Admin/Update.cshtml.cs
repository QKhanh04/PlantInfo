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
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class UpdateModel : PageModel
    {
        private readonly ILogger<UpdateModel> _logger;
        private readonly IPlantService _plantService;
        private readonly ICategoryService _categoryService;
        private readonly IUseService _useService;
        private readonly ISpeciesService _speciesService;
        private readonly IDiseaseService _diseaseService;
        private readonly IMapper _mapper;

        public UpdateModel(ILogger<UpdateModel> logger, IPlantService plantService,
         ICategoryService categoryService,
        IUseService useService,
        ISpeciesService speciesService,
        IDiseaseService diseaseService, IMapper mapper)
        {
            _logger = logger;
            _plantService = plantService;
            _mapper = mapper;
            _categoryService = categoryService;
            _useService = useService;
            _speciesService = speciesService;
            _diseaseService = diseaseService;
        }
        [BindProperty]
        public PlantUpdateDTO Plant { get; set; } = new PlantUpdateDTO();
        public List<SelectListItem> SpeciesList { get; set; } = new();
        public List<SelectListItem> CategoryList { get; set; } = new();
        public List<SelectListItem> UseList { get; set; } = new();
        public List<SelectListItem> DiseaseList { get; set; } = new();


        // Lấy dữ liệu cây để hiển thị lên form
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var result = await _plantService.GetDetailPlantAsync(id);
            if (!result.Success || result.Data == null)
            {
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
                TempData["ToastMessage"] = "Không tìm thấy cây!";
                return RedirectToPage("/Plants/Index");
            }

            // Map từ PlantDetailDTO sang PlantUpdateDTO (bạn có thể dùng AutoMapper hoặc tự map)
            Plant = _mapper.Map<PlantUpdateDTO>(result.Data);
            var species = await _speciesService.GetAllSpeciesAsync();
            SpeciesList = species.Data.Select(c => new SelectListItem
            {
                Value = c.SpeciesId.ToString(),
                Text = c.ScientificName

            }).ToList();
            var categories = await _categoryService.GetAllCategoryAsync();
            CategoryList = categories.Data.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.CategoryName
            }).ToList();
            var uses = await _useService.GetAllUsesAsync();

            UseList = uses.Data.Select(c => new SelectListItem
            {
                Value = c.UseId.ToString(),
                Text = c.UseName
            }).ToList();

            var disease = await _diseaseService.GetAllDiseasesAsync();
            DiseaseList = disease.Data.Select(s => new SelectListItem
            {
                Value = s.DiseaseId.ToString(),
                Text = s.DiseaseName
            }).ToList();
            Plant.CategoryIds = result.Data.Categories?.Select(c => c.CategoryId).ToList();
            Plant.UseIds = result.Data.Uses?.Select(u => u.UseId).ToList();
            Plant.DiseaseIds = result.Data.Diseases?.Select(d => d.DiseaseId).ToList();
            return Page();


        }

        // Gửi form cập nhật cây
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        _logger.LogWarning($"ModelState error for {key}: {error.ErrorMessage}");
                        TempData["ToastMessage"] = $"{key}: {error.ErrorMessage}";

                    }
                }
                return Page();
            }
            if (Plant.ImageFiles != null && Plant.ImageFiles.Count > 0)
            {
                foreach (var file in Plant.ImageFiles)
                {
                    if (file.Length > 0)
                    {
                        // Đường dẫn lưu file (ví dụ lưu vào wwwroot/uploads)
                        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                        if (!Directory.Exists(uploads))
                            Directory.CreateDirectory(uploads);

                        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploads, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Sau khi upload, thêm ảnh vào Plant.Images để lưu vào DB
                        if (Plant.Images == null) Plant.Images = new List<PlantImageDTO>();
                        Plant.Images.Add(new PlantImageDTO
                        {
                            ImageUrl = "/uploads/" + fileName,
                            Caption = "Ảnh tải lên", // hoặc có thể cho người dùng nhập caption
                            IsPrimary = false
                        });
                    }
                }
            }
            var result = await _plantService.UpdatePlantAsync(Plant);
            if (result.Success)
            {
                TempData["ToastType"] = "success";
                TempData["ToastMessage"] = "Cập nhật thành công!";
                Console.WriteLine("So luong anh trong Plant.Images: " + Plant.Images?.Count);

                return RedirectToPage("/Detail", new { id = Plant.PlantId });
            }
            else
            {
                TempData["ToastMessage"] = result.Message;
                return Page();
            }
        }
    }
}