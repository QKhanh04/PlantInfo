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
        public string? OrderName { get; set; }
        public List<int>? CategoryIds { get; set; }
        public List<int>? UseIds { get; set; }
        public List<int>? DiseaseIds { get; set; }

    }
}