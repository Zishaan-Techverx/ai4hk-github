using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TpaSodManagement.Models.Db;

public partial class Organization
{
    public long OrganizationId { get; set; }

    public string OrganizationType { get; set; } = null!;

    public string OrganizationName { get; set; } = null!;

    public string? OrganizationCode { get; set; }

    public string? TaxIdentificationNumber { get; set; }

    public string? RegistrationNumber { get; set; }

    public DateOnly? EstablishedDate { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset UpdatedDate { get; set; }
    public string? LogoPath { get; set; }

    [NotMapped]
    public IFormFile? LogoFile { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<Farm> Farms { get; set; } = new List<Farm>();
}
