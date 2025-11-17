using System;
using System.Collections.Generic;

namespace TpaSodManagement.Models.Db;

public partial class Website
{
    public long WebsiteId { get; set; }

    // public string EntityType { get; set; } = null!;

    // public long EntityId { get; set; }

    public string WebsiteUrl { get; set; } = null!;

    public string? WebsiteType { get; set; }

    public bool IsPrimary { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset UpdatedDate { get; set; }
    
    public virtual ICollection<TpaUser> TpaUsers { get; set; } = new List<TpaUser>();
}
