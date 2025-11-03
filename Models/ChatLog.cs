using System;
using System.Collections.Generic;

namespace PlantManagement.Models;

public partial class ChatLog
{
    public int ChatId { get; set; }

    public int? UserId { get; set; }

    public string? SessionId { get; set; }

    public string Sender { get; set; } = null!;

    public string Message { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
