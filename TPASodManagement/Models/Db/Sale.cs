using System;
using System.Collections.Generic;
using TpaSodManagement.Areas.Identity.Data;

namespace TpaSodManagement.Models.Db;

public partial class Sale
{
    public long SaleId { get; set; }

    public long UserId { get; set; }

    public long FarmId { get; set; }

    public long CustomerId { get; set; }

    public int SaleTypeId { get; set; }

    public string SaleNumber { get; set; } = null!;

    public string? InvoiceNumber { get; set; }

    public string? PurchaseOrderNumber { get; set; }

    public DateOnly SaleDate { get; set; }

    public DateOnly? DueDate { get; set; }

    public decimal SubtotalAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public int CurrencyId { get; set; }

    public int? PaymentTermsDays { get; set; }

    public int StatusId { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset UpdatedDate { get; set; }

    public long UpdatedByUserId { get; set; }

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual Currency Currency { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual Farm Farm { get; set; } = null!;

    public virtual ICollection<SaleLineItem> SaleLineItems { get; set; } = new List<SaleLineItem>();

    public virtual SaleType SaleType { get; set; } = null!;

    public virtual Status Status { get; set; } = null!;

    public virtual TpaSodManagementUser UpdatedByUser { get; set; } = null!;

    public virtual TpaSodManagementUser User { get; set; } = null!;
}
