using System;
using System.Collections.Generic;

namespace PlantManagement.Models;

public partial class ViewLog
{
    public int ViewId { get; set; }

    public int? PlantId { get; set; }

    public int? UserId { get; set; }

    public DateTime? ViewDate { get; set; }

    public virtual Plant? Plant { get; set; }

    public virtual User? User { get; set; }
}
