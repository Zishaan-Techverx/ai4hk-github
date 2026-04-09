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

    public long FarmId { get; set; }

    public TagSeedType SeedType { get; set; }

    public int TotalTags { get; set; }

    public virtual Farm Farm { get; set; } = null!;

    public virtual ICollection<SaleLineItem> SaleLineItems { get; set; } = new List<SaleLineItem>();
}
