using System;
using System.Collections.Generic;

namespace TpaSodManagement.Database.Entities;

public partial class SaleLineItem
{
    public long SaleLineItemId { get; set; }

    public long SaleId { get; set; }

    public int LineNumber { get; set; }

    public long ProductId { get; set; }

    public long? SeedingId { get; set; }

    public long? FieldId { get; set; }

    public long? TagRangeId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }

    public string? LotNumber { get; set; }

    public decimal? AreaAmount { get; set; }

    public int? AreaTypeId { get; set; }

    public string? VerificationCode { get; set; }

    public long? CertificateSequenceNumber { get; set; }

    public string? RoyaltyInvoiceNumber { get; set; }

    public string? Notes { get; set; }

    public long? DeletedByUserId { get; set; }

    public DateTimeOffset? DeletedDate { get; set; }

    public virtual AreaType? AreaType { get; set; }

    public virtual Field? Field { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Sale Sale { get; set; } = null!;

    public virtual Seeding? Seeding { get; set; }

    public virtual TagRange? TagRange { get; set; }
}
