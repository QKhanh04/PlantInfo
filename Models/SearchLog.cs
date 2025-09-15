using System;
using System.Collections.Generic;

namespace PlantManagement.Models;

public partial class SearchLog
{
    public int SearchId { get; set; }

    public int? UserId { get; set; }

    public string Keyword { get; set; } = null!;

    public DateTime? SearchDate { get; set; }

    public virtual User? User { get; set; }
}
