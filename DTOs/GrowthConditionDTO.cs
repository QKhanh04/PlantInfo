using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantManagement.DTOs
{
    public class GrowthConditionDTO
    {
        public string? SoilType { get; set; }
        public string? Climate { get; set; }
        public string? TemperatureRange { get; set; }
        public string? WaterRequirement { get; set; }
        public string? Sunlight { get; set; }
    }
}