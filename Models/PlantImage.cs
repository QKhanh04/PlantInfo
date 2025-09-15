using System;
using System.Collections.Generic;

namespace PlantManagement.Models;

public partial class PlantImage
{
    public int ImageId { get; set; }

    public int PlantId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? Caption { get; set; }

    public bool? IsPrimary { get; set; }

    public virtual Plant Plant { get; set; } = null!;
}
