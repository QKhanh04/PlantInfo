using System;
using System.Collections.Generic;

namespace PlantManagement.Models;

public partial class Favorite
{
    public int UserId { get; set; }

    public int PlantId { get; set; }

    public DateTime? CreateAt { get; set; }

    public virtual Plant Plant { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
