using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Database.Base_Entities;

namespace TpaSodManagement.Database.Entities;

public partial class Organization : AuditBaseEntity
{
    public long OrganizationId { get; set; }

    public string? OrganizationTypeName { get; set; }

    public string OrganizationName { get; set; } = null!;

    public long? OrganizationTypeId { get; set; }
    public virtual OrganizationType? OrganizationType { get; set; }

    public string? OrganizationCode { get; set; }

    public string? TaxIdentificationNumber { get; set; }

    public string? RegistrationNumber { get; set; }

    public DateOnly? EstablishedDate { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public byte[]? LogoBytes { get; set; }

    [NotMapped]
    public IFormFile? LogoFile { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<Farm> Farms { get; set; } = new List<Farm>();

    public virtual ICollection<TpaSodManagementUser> Users { get; set; } = new List<TpaSodManagementUser>();
}