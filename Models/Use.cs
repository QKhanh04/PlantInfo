using System;
using System.Collections.Generic;

namespace PlantManagement.Models;

public partial class Use
{
    public int UseId { get; set; }

    public string UseName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();
}
