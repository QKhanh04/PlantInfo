using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantManagement.DTOs
{
    public class ReferenceDTO
    {
        public int ReferenceId { get; set; }
        public string SourceName { get; set; }
        public string Url { get; set; }
        public string Author { get; set; }
        public int? PublishedYear { get; set; }
    }
}