using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantManagement.DTOs
{
    public class SpeciesDTO
    {
        public string ScientificName { get; set; }
        public string Genus { get; set; }
        public string Family { get; set; }
        public string OrderName { get; set; }
    }
}