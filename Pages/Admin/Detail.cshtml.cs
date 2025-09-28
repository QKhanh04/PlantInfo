using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PlantManagement.DTOs;
using PlantManagement.Services;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Pages.Admin
{

    [Authorize(Roles = "Admin")]
    public class DetailModel : PageModel
    {
        private readonly ILogger<DetailModel> _logger;
        private readonly IPlantService _plantService;

        public DetailModel(ILogger<DetailModel> logger, IPlantService plantService)
        {
            _logger = logger;
            _plantService = plantService;
        }

        public PlantDetailDTO? Plants { get; set; }
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var result = await _plantService.GetDetailAsync(id);
            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Message;
                return Redirect("/Admin/Index");
            }
            Plants = result.Data;
            return Page();


        }
    }
}
