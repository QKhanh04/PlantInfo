using System;
using System.Collections.Generic;

namespace PlantManagement.Models;

public partial class GrowthCondition
{
    public int ConditionId { get; set; }

    public int PlantId { get; set; }

    public string? SoilType { get; set; }

    public string? Climate { get; set; }

    public string? TemperatureRange { get; set; }

    public string? WaterRequirement { get; set; }

    public string? Sunlight { get; set; }

    public virtual Plant Plant { get; set; } = null!;
}
