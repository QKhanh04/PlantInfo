using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantManagement.DTOs
{
    public class PlantDetailDTO
    {
        public int PlantId { get; set; }
        public string CommonName { get; set; }
        public string Origin { get; set; }
        public string Description { get; set; }
        public SpeciesDTO Species { get; set; }
        public List<string> CategoryNames { get; set; }
        public List<string> ImageUrls { get; set; }
        public List<UsesDTO> Uses { get; set; }
        public List<DiseasesDTO> Diseases { get; set; }
        public GrowthConditionDTO GrowthCondition { get; set; }
       
    }
}