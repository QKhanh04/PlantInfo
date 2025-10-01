using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantManagement.DTOs
{
    public class PlantDTO
    {
        public int PlantId { get; set; }
        public string CommonName { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
        // Tham chiếu
        public SpeciesDTO? Species { get; set; }
        public GrowthConditionDTO? GrowthCondition { get; set; }
        public List<DiseaseDTO>? Diseases { get; set; }
        public List<CategoryDTO>? Categories { get; set; }
        public List<UseDTO>? Uses { get; set; }
        public List<PlantImageDTO>? Images { get; set; }
        public List<PlantReferenceDTO>? References { get; set; }
    }
}