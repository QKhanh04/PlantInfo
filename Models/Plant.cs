using System;
using System.Collections.Generic;

namespace PlantManagement.Models;

public partial class Plant
{
    public int PlantId { get; set; }

    public string? CommonName { get; set; }

    public string? Origin { get; set; }

    public int? SpeciesId { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual ICollection<Disease> Diseases { get; set; } = new List<Disease>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual GrowthCondition? GrowthCondition { get; set; }

    public virtual ICollection<PlantImage> PlantImages { get; set; } = new List<PlantImage>();

    public virtual ICollection<PlantReference> PlantReferences { get; set; } = new List<PlantReference>();

    public virtual ICollection<PlantReview> PlantReviews { get; set; } = new List<PlantReview>();

    public virtual Species? Species { get; set; }

    public virtual ICollection<ViewLog> ViewLogs { get; set; } = new List<ViewLog>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<Use> Uses { get; set; } = new List<Use>();
}
