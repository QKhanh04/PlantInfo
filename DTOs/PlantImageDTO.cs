using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantManagement.DTOs
{
    public class PlantImageDTO
    {
        public int? ImageId { get; set; }
        public string? ImageUrl { get; set; } = string.Empty;

        public string? Caption { get; set; }

        public bool IsPrimary { get; set; }
    }
}