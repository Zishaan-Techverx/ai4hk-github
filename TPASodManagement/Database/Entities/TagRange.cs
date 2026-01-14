using System;
using System.Collections.Generic;
using TpaSodManagement.Database.Base_Entities;

namespace TpaSodManagement.Database.Entities;

public partial class TagRange : AuditBaseEntity
{
    public long TagRangeId { get; set; }

    public string TagRangeCode { get; set; } = null!;

    public long TagStartNumber { get; set; }

    public long TagEndNumber { get; set; }

    public string? TagPrefix { get; set; }

    public string? TagSuffix { get; set; }

    public int TotalTags { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<SaleLineItem> SaleLineItems { get; set; } = new List<SaleLineItem>();

    public virtual ICollection<Seeding> Seedings { get; set; } = new List<Seeding>();
}
