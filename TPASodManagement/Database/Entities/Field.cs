using System;
using System.Collections.Generic;
using TpaSodManagement.Database.Base_Entities;

namespace TpaSodManagement.Database.Entities;

public partial class Field : AuditBaseEntity
{
    public long FieldId { get; set; }

    public long FarmId { get; set; }

    public string FieldName { get; set; } = null!;

    public string? FieldCode { get; set; }

    public decimal AreaAmount { get; set; }

    public int AreaTypeId { get; set; }

    public string? SoilType { get; set; }

    public decimal? SlopePercentage { get; set; }

    public bool IrrigationAvailable { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string? BoundaryCoordinates { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; }

    public virtual AreaType AreaType { get; set; } = null!;

    public virtual Farm Farm { get; set; } = null!;

    public virtual ICollection<SaleLineItem> SaleLineItems { get; set; } = new List<SaleLineItem>();

    public virtual ICollection<Seeding> Seedings { get; set; } = new List<Seeding>();

    public virtual ICollection<Waste> Wastes { get; set; } = new List<Waste>();
}
