using System;
using System.Collections.Generic;

namespace PlantManagement.Models;

public partial class Plant
{
    public int PlantId { get; set; }

    public string ScientificName { get; set; } = null!;

    public string? CommonName { get; set; }

    public string? Family { get; set; }

    public string? Genus { get; set; }

    public string? Description { get; set; }

    public string? Origin { get; set; }

    public virtual ICollection<Disease> Diseases { get; set; } = new List<Disease>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<GrowthCondition> GrowthConditions { get; set; } = new List<GrowthCondition>();

    public virtual ICollection<PlantImage> PlantImages { get; set; } = new List<PlantImage>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<Use> Uses { get; set; } = new List<Use>();
}
