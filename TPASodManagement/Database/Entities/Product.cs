using System;
using System.Collections.Generic;
using TpaSodManagement.Areas.Identity.Data;

namespace TpaSodManagement.Database.Entities;

public partial class Product
{
    public long ProductId { get; set; }

    public string ProductCode { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public int ProductCategoryId { get; set; }

    public string UnitOfMeasure { get; set; } = null!;

    public decimal? StandardPrice { get; set; }

    public int? CurrencyId { get; set; }

    public bool RequiresCertificate { get; set; }

    public int? CertificateTypeId { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public long CreatedByUserId { get; set; }

    public long? DeletedByUserId { get; set; }

    public DateTimeOffset? DeletedDate { get; set; }

    public virtual CertificateType? CertificateType { get; set; }

    public virtual TpaSodManagementUser CreatedByUser { get; set; } = null!;

    public virtual Currency? Currency { get; set; }

    public virtual ProductCategory ProductCategory { get; set; } = null!;

    public virtual ICollection<SaleLineItem> SaleLineItems { get; set; } = new List<SaleLineItem>();
}
