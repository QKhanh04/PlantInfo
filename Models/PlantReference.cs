using System;
using System.Collections.Generic;

namespace PlantManagement.Models;

public partial class PlantReference
{
    public int ReferenceId { get; set; }

    public int? PlantId { get; set; }

    public string? SourceName { get; set; }

    public string? Url { get; set; }

    public string? Author { get; set; }

    public int? PublishedYear { get; set; }

    public virtual Plant? Plant { get; set; }
}
