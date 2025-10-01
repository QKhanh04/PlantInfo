using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantManagement.DTOs
{
    public class DiseasesDTO
    {
        //disease_name VARCHAR(255),
        //symptoms TEXT,
        // treatment TEXT
        public int DiseaseId { get; set; }

        public string DiseaseName { get; set; }
        public string Symptoms { get; set; }
        public string Treatment { get; set; }
    }
}