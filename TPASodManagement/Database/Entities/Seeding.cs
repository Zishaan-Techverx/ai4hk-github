using System;
using System.Collections.Generic;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Database.Base_Entities;

namespace TpaSodManagement.Database.Entities;

public partial class Seeding : AuditBaseEntity
{
    public long SeedingId { get; set; }

    public long UserId { get; set; }

    public long FarmId { get; set; }

    public long? FieldId { get; set; }

    public int TagStartNumber { get; set; }

    public int TagEndNumber { get; set; }

    public decimal? AreaAmount { get; set; }

    public int AreaTypeId { get; set; }

    public DateOnly SeedingDate { get; set; }

    public string? SeedingMethod { get; set; }

    public decimal? SeedRatePerUnit { get; set; }

    public string? WeatherConditions { get; set; }

    public decimal? SoilTemperature { get; set; }

    public string? SoilMoisture { get; set; }

    public string? Notes { get; set; }

    public virtual AreaType AreaType { get; set; } = null!;

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual Farm Farm { get; set; } = null!;

    public virtual Field? Field { get; set; }

    public virtual ICollection<SaleLineItem> SaleLineItems { get; set; } = new List<SaleLineItem>();

    public virtual TpaSodManagementUser User { get; set; } = null!;

    public virtual ICollection<Waste> Wastes { get; set; } = new List<Waste>();
}
