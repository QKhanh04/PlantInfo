using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantManagement.DTOs
{
    public class PlantListDTO
    {
        public int PlantId { get; set; }
        public string CommonName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? SpeciesName { get; set; }
        public List<string>? CategoryNames { get; set; }
        public List<string>? ImageUrls { get; set; }
        public bool? IsActive { get; set; }
        public bool IsFavorited { get; set; }
    }
}