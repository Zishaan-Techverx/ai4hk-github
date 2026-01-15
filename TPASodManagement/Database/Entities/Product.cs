using System;
using System.Collections.Generic;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Database.Base_Entities;

namespace TpaSodManagement.Database.Entities;

public partial class Product : AuditBaseEntity
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

    public virtual CertificateType? CertificateType { get; set; }

    public virtual TpaSodManagementUser? CreatedByUser { get; set; }

    public virtual Currency? Currency { get; set; }

    public virtual ProductCategory ProductCategory { get; set; } = null!;

    public virtual ICollection<SaleLineItem> SaleLineItems { get; set; } = new List<SaleLineItem>();
}
