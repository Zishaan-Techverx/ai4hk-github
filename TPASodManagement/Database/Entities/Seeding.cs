using System;
using System.Collections.Generic;
using TpaSodManagement.Areas.Identity.Data;

namespace TpaSodManagement.Database.Entities;

public partial class Seeding
{
    public long SeedingId { get; set; }

    public long UserId { get; set; }

    public long FarmId { get; set; }

    public long? FieldId { get; set; }

    public long TagRangeId { get; set; }

    public decimal AreaAmount { get; set; }

    public int AreaTypeId { get; set; }

    public DateOnly SeedingDate { get; set; }

    public string? SeedingMethod { get; set; }

    public decimal? SeedRatePerUnit { get; set; }

    public string? WeatherConditions { get; set; }

    public decimal? SoilTemperature { get; set; }

    public string? SoilMoisture { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public long? DeletedByUserId { get; set; }

    public DateTimeOffset? DeletedDate { get; set; }

    public virtual AreaType AreaType { get; set; } = null!;

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual Farm Farm { get; set; } = null!;

    public virtual Field? Field { get; set; }

    public virtual ICollection<SaleLineItem> SaleLineItems { get; set; } = new List<SaleLineItem>();

    public virtual TagRange TagRange { get; set; } = null!;

    public virtual TpaSodManagementUser User { get; set; } = null!;

    public virtual ICollection<Waste> Wastes { get; set; } = new List<Waste>();
}
