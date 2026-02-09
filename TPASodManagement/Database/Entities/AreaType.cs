using System;
using System.Collections.Generic;
using TpaSodManagement.Database.Base_Entities;

namespace TpaSodManagement.Database.Entities;

public partial class AreaType : AuditBaseEntity
{
    public int AreaTypeId { get; set; }

    public string AreaTypeName { get; set; } = null!;

    public string UnitAbbreviation { get; set; } = null!;

    public string UnitSystem { get; set; } = null!;

    public decimal ConversionToSquareMeters { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Farm> Farms { get; set; } = new List<Farm>();

    public virtual ICollection<Field> Fields { get; set; } = new List<Field>();

    public virtual ICollection<SaleLineItem> SaleLineItems { get; set; } = new List<SaleLineItem>();

    public virtual ICollection<Seeding> Seedings { get; set; } = new List<Seeding>();

    public virtual ICollection<Waste> Wastes { get; set; } = new List<Waste>();
}
