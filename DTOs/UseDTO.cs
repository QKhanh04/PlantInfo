using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantManagement.DTOs
{
    public class UseDTO
    {
        public int UseId { get; set; }
        public string UseName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}