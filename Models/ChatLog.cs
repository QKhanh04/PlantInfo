using System;
using System.Collections.Generic;

namespace PlantManagement.Models;

public partial class ChatLog
{
    public int ChatId { get; set; }

    public int? UserId { get; set; }

    public string Message { get; set; } = null!;

    public string? Response { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
