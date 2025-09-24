using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantManagement.DTOs
{
    public class PlantDTO
    {
        public int PlantId { get; set; }
        public string CommonName { get; set; }
        public string Description { get; set; }
        public string SpeciesName { get; set; }
        public List<string> CategoryNames { get; set; }
        public string ImageUrls { get; set; }
 
    }
}