// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.RazorPages;
// using Microsoft.Extensions.Logging;
// using PlantManagement.Models;
// using PlantManagement.Repositories.Interfaces;
// using PlantManagement.Services.Interfaces;

// namespace PlantManagement.Pages.Admin
// {
//     public class CreateModel : PageModel
//     {
//         private readonly ILogger<CreateModel> _logger;
//         private readonly IPlantService _plantService;
//         private readonly ICategoryRepository _categoryRepo;
//         private readonly IUseRepository _useRepo;
//         private readonly ISpeciesRepository _speciesRepo;
//         private readonly IDiseaseRepository _diseaseRepo;
//         private readonly IGrowthConditionRepository _growthRepo;
//         private readonly IMapper _mapper;

//         public CreateModel(ILogger<CreateModel> logger, IPlantService plantService, ICategoryRepository categoryRepo,
//         IUseRepository useRepo,
//         ISpeciesRepository speciesRepo,
//         IDiseaseRepository diseaseRepo,
//         IGrowthConditionRepository growthRepo,
//         IMapper mapper)
//         {
//             _logger = logger;
//             _plantService = plantService;
//             _categoryRepo = categoryRepo;
//             _useRepo = useRepo;
//             _speciesRepo = speciesRepo;
//             _diseaseRepo = diseaseRepo;
//             _growthRepo = growthRepo;
//             _mapper = mapper;
//         }

//         [BindProperty]
//         public CreatePlantViewModel Plant { get; set; }

//         public List<Category> AllCategories { get; set; }
//         public List<Use> AllUses { get; set; }
//         public List<Species> AllSpecies { get; set; }
//         public List<Disease> AllDiseases { get; set; }
//         public List<GrowthCondition> AllGrowthConditions { get; set; }

//         public async Task OnGetAsync()
//         {
//             AllCategories = (List<Category>)await _categoryRepo.GetAllAsync();
//             AllUses = (List<Use>)await _useRepo.GetAllAsync();
//             AllSpecies = (List<Species>)await _speciesRepo.GetAllAsync();
//             AllDiseases = (List<Disease>)await _diseaseRepo.GetAllAsync();
//             AllGrowthConditions = (List<GrowthCondition>)await _growthRepo.GetAllAsync();
//         }



//         public async Task<IActionResult> OnPostAsync()
//         {

//             AllCategories = (List<Category>)await _categoryRepo.GetAllAsync();
//             AllUses = (List<Use>)await _useRepo.GetAllAsync();
//             AllSpecies = (List<Species>)await _speciesRepo.GetAllAsync();
//             AllDiseases = (List<Disease>)await _diseaseRepo.GetAllAsync();
//             AllGrowthConditions = (List<GrowthCondition>)await _growthRepo.GetAllAsync();

//             if (!ModelState.IsValid)
//             {
//                 foreach (var state in ModelState)
//                 {
//                     var key = state.Key;
//                     var errors = state.Value.Errors;
//                     foreach (var error in errors)
//                     {
//                         _logger.LogError($"Field {key} error: {error.ErrorMessage}");
//                         TempData["Error"] = $"Field {key} error: {error.ErrorMessage}";
//                     }
//                 }

//                 return Page();
//             }

//             // Map ViewModel sang DTO bằng AutoMapper
//             var dto = _mapper.Map<CreatePlantDTO>(Plant);
//             var result = await _plantService.CreateAsync(dto);

//             if (result.Success)
//             {
//                 TempData["Success"] = result.Message;
//                 return RedirectToPage("Index");
//             }
//             else
//             {
//                 ModelState.AddModelError("", result.Message);
//                 TempData["Error"] = result.Message;

//                 return Page();
//             }
//         }
//     }
// }