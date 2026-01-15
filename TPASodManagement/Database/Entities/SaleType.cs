using System;
using System.Collections.Generic;
using TpaSodManagement.Database.Base_Entities;

namespace TpaSodManagement.Database.Entities;

public partial class SaleType : AuditBaseEntity
{
    public int SaleTypeId { get; set; }

    public string SaleTypeCode { get; set; } = null!;

    public string SaleTypeName { get; set; } = null!;

    public bool RequiresCertificate { get; set; }

    public int? CertificateTypeId { get; set; }

    public bool TaxApplicable { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public virtual CertificateType? CertificateType { get; set; }

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
