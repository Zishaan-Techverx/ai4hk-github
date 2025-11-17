using System;
using System.Collections.Generic;

namespace TpaSodManagement.Models.Db;

public partial class Status
{
    public int StatusId { get; set; }

    public string StatusCategory { get; set; } = null!;

    public string StatusCode { get; set; } = null!;

    public string StatusName { get; set; } = null!;

    public int StatusOrder { get; set; }

    public bool IsFinalStatus { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public virtual ICollection<Testimonial> Testimonials { get; set; } = new List<Testimonial>();
}
