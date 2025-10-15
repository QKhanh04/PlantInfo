using System;
using System.Collections.Generic;

namespace PlantManagement.Models;

public partial class PlantReview
{
    public int ReviewId { get; set; }

    public int PlantId { get; set; }

    public int UserId { get; set; }

    public string? Comment { get; set; }

    public int Rating { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual Plant Plant { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
