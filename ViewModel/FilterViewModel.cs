using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantManagement.ViewModel
{
    public class FilterViewModel
    {
        public string? Keyword { get; set; }
        public int? CategoryId { get; set; }

        public int? UseId { get; set; }

        public int? DiseaseId { get; set; }
    }
}