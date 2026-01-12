using System;
using System.Collections.Generic;
using TpaSodManagement.Areas.Identity.Data;

namespace TpaSodManagement.Database.Entities;

public partial class Farm
{
    public long FarmId { get; set; }

    public long OrganizationId { get; set; }

    public decimal? TotalArea { get; set; }

    public int? AreaTypeId { get; set; }

    public bool OrganicCertified { get; set; }

    public string? LicenseNumber { get; set; }

    public string? CertificationDetails { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public decimal? ElevationMeters { get; set; }

    public string? SoilType { get; set; }

    public string? IrrigationType { get; set; }

    public string? ClimateZone { get; set; }

    public virtual AreaType? AreaType { get; set; }

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual ICollection<Field> Fields { get; set; } = new List<Field>();

    public virtual Organization Organization { get; set; } = null!;

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public virtual ICollection<Seeding> Seedings { get; set; } = new List<Seeding>();

    public virtual ICollection<Testimonial> Testimonials { get; set; } = new List<Testimonial>();

    public virtual ICollection<TpaSodManagementUser> TpaUsers { get; set; } = new List<TpaSodManagementUser>();

    public virtual ICollection<Waste> Wastes { get; set; } = new List<Waste>();
}
