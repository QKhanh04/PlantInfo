using System;
using System.Collections.Generic;

namespace PlantManagement.Models;

public partial class Disease
{
    public int DiseaseId { get; set; }

    public int? PlantId { get; set; }

    public string? DiseaseName { get; set; }

    public string? Symptoms { get; set; }

    public string? Treatment { get; set; }

    public virtual Plant? Plant { get; set; }
}
