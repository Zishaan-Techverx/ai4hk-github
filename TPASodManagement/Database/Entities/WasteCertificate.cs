using System;
using System.Collections.Generic;

namespace TpaSodManagement.Database.Entities;

public partial class WasteCertificate
{
    public long WasteCertificateId { get; set; }

    public long CertificateId { get; set; }

    public long WasteId { get; set; }

    public long CertificateNumber { get; set; }

    public string? Notes { get; set; }

    public virtual Certificate Certificate { get; set; } = null!;

    public virtual Waste Waste { get; set; } = null!;
}
