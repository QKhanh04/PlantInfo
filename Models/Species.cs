using System;
using System.Collections.Generic;

namespace PlantManagement.Models;

public partial class Species
{
    public int SpeciesId { get; set; }

    public string ScientificName { get; set; } = null!;

    public string? Genus { get; set; }

    public string? Family { get; set; }

    public string? OrderName { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();
}
