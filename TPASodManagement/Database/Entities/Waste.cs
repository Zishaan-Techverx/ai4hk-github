using System;
using System.Collections.Generic;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Database.Base_Entities;

namespace TpaSodManagement.Database.Entities;

public partial class Waste : AuditBaseEntity
{
    public long WasteId { get; set; }

    public long UserId { get; set; }

    public long FarmId { get; set; }

    public long? FieldId { get; set; }

    public long? SeedingId { get; set; }

    public decimal AreaAmount { get; set; }

    public int AreaTypeId { get; set; }

    public DateOnly WasteDate { get; set; }

    public int WasteReasonId { get; set; }

    public string? DisposalMethod { get; set; }

    public string? DisposalLocation { get; set; }

    public bool EnvironmentalImpactAssessed { get; set; }

    public bool RegulatoryReported { get; set; }

    public decimal? EstimatedLossValue { get; set; }

    public int? CurrencyId { get; set; }

    public string? Notes { get; set; }

    public virtual AreaType AreaType { get; set; } = null!;

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual Currency? Currency { get; set; }

    public virtual Farm Farm { get; set; } = null!;

    public virtual Field? Field { get; set; }

    public virtual Seeding? Seeding { get; set; }

    public virtual TpaSodManagementUser User { get; set; } = null!;

    public virtual ICollection<WasteCertificate> WasteCertificates { get; set; } = new List<WasteCertificate>();

    public virtual WasteReason WasteReason { get; set; } = null!;
}
