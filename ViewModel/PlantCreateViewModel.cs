using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.DTOs;

namespace PlantManagement.ViewModel
{
    public class PlantCreateViewModel
    {
        public PlantCreateDTO Plant { get; set; } = new PlantCreateDTO();

        // dữ liệu hiển thị để chọn
        public List<SpeciesDTO> AvailableSpecies { get; set; } = new();
        public List<CategoryDTO> AvailableCategories { get; set; } = new();
        public List<UseDTO> AvailableUses { get; set; } = new();
    }
}