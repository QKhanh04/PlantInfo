using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace PlantManagement.Pages.Admin
{
    public class Report : PageModel
    {
        private readonly ILogger<Report> _logger;

        public Report(ILogger<Report> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}