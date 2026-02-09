using System;
using System.Collections.Generic;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Database.Base_Entities;

namespace TpaSodManagement.Database.Entities;

public partial class Website : AuditBaseEntity
{
    public long WebsiteId { get; set; }

    // public string EntityType { get; set; } = null!;

    // public long EntityId { get; set; }

    public string WebsiteUrl { get; set; } = null!;

    public string? WebsiteType { get; set; }

    public bool IsPrimary { get; set; }

    public virtual ICollection<TpaSodManagementUser> TpaUsers { get; set; } = new List<TpaSodManagementUser>();
}
