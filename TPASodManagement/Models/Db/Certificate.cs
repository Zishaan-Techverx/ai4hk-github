using System;
using System.Collections.Generic;
using TpaSodManagement.Areas.Identity.Data;

namespace TpaSodManagement.Models.Db;

public partial class Certificate
{
    public long CertificateId { get; set; }

    public int CertificateTypeId { get; set; }

    public long UserId { get; set; }

    public long FarmId { get; set; }

    public string CertificateNumber { get; set; } = null!;

    public string? CertificateRangeStart { get; set; }

    public string? CertificateRangeEnd { get; set; }

    public DateOnly IssueDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public string IssuingAuthority { get; set; } = null!;

    public string? IssuingOfficer { get; set; }

    public int StatusId { get; set; }

    public string? VerificationCode { get; set; }

    public string? DigitalSignature { get; set; }

    public long? RelatedSaleId { get; set; }

    public long? RelatedSeedingId { get; set; }

    public long? RelatedWasteId { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public virtual CertificateType CertificateType { get; set; } = null!;

    public virtual Farm Farm { get; set; } = null!;

    public virtual Sale? RelatedSale { get; set; }

    public virtual Seeding? RelatedSeeding { get; set; }

    public virtual Waste? RelatedWaste { get; set; }

    public virtual Status Status { get; set; } = null!;

    public virtual TpaSodManagementUser User { get; set; } = null!;

    public virtual ICollection<WasteCertificate> WasteCertificates { get; set; } = new List<WasteCertificate>();
}
