using System;
using System.Collections.Generic;
using TpaSodManagement.Database.Base_Entities;

namespace TpaSodManagement.Database.Entities;

public partial class Customer : AuditBaseEntity
{
    public long CustomerId { get; set; }

    public string CustomerType { get; set; } = null!;

    public long? PersonId { get; set; }

    public long? OrganizationId { get; set; }

    public string? CustomerCode { get; set; }

    public decimal? CreditLimit { get; set; }

    public int? PaymentTermsDays { get; set; }

    public bool TaxExempt { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; }

    public virtual Organization? Organization { get; set; }

    public virtual Person? Person { get; set; }

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public virtual ICollection<Testimonial> Testimonials { get; set; } = new List<Testimonial>();
}
