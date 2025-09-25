using System;
using System.Collections.Generic;

namespace PlantManagement.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Email { get; set; }

    public string? Role { get; set; }

    public DateTime? CreateAt { get; set; }

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<SearchLog> SearchLogs { get; set; } = new List<SearchLog>();
}
