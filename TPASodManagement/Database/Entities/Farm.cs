using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Database.Base_Entities;

namespace TpaSodManagement.Database.Entities;

public partial class Farm : AuditBaseEntity
{
    public long FarmId { get; set; }

    [Required(ErrorMessage = "Farm Name is required")]
    public string FarmName { get; set; } = null!;

    public long OrganizationId { get; set; }

    public long? AddressId { get; set; }

    public decimal? TotalArea { get; set; }

    public virtual Address? Address { get; set; }

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

    public byte[]? LogoBytes { get; set; }

    /// <summary>
    /// Relative path under wwwroot, e.g. "uploads/logos/farm_1.png".
    /// </summary>
    public string? LogoFilePath { get; set; }

    [NotMapped]
    public IFormFile? LogoFile { get; set; }

    public virtual AreaType? AreaType { get; set; }

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual ICollection<Field> Fields { get; set; } = new List<Field>();

    public virtual Organization Organization { get; set; } = null!;

    public virtual ICollection<Seeding> Seedings { get; set; } = new List<Seeding>();

    public virtual ICollection<Testimonial> Testimonials { get; set; } = new List<Testimonial>();

    public virtual ICollection<TpaSodManagementUser> TpaUsers { get; set; } = new List<TpaSodManagementUser>();

    public virtual ICollection<Waste> Wastes { get; set; } = new List<Waste>();
}
