using System;
using System.Collections.Generic;

namespace TpaSodManagement.Database.Entities;

public partial class CertificateType
{
    public int CertificateTypeId { get; set; }

    public string CertificateTypeCode { get; set; } = null!;

    public string CertificateTypeName { get; set; } = null!;

    public int? ValidityPeriodDays { get; set; }

    public bool RequiresRenewal { get; set; }

    public string? IssuingAuthority { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<SaleType> SaleTypes { get; set; } = new List<SaleType>();
}
