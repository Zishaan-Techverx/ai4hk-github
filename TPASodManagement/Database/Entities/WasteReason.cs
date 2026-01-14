using System;
using System.Collections.Generic;
using TpaSodManagement.Database.Base_Entities;

namespace TpaSodManagement.Database.Entities;

public partial class WasteReason : AuditBaseEntity
{
    public int WasteReasonId { get; set; }

    public string ReasonCode { get; set; } = null!;

    public string ReasonName { get; set; } = null!;

    public string ReasonCategory { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Waste> Wastes { get; set; } = new List<Waste>();
}
