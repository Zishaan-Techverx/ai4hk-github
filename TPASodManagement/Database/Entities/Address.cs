using System;
using System.Collections.Generic;
using TpaSodManagement.Areas.Identity.Data;

namespace TpaSodManagement.Database.Entities;

public partial class Address
{
    public long AddressId { get; set; }

    // public string EntityType { get; set; } = null!;

    // public long EntityId { get; set; }

    public int AddressTypeId { get; set; }

    public string AddressLine1 { get; set; } = null!;

    public string? AddressLine2 { get; set; }

    public string City { get; set; } = null!;

    public int? StateProvinceId { get; set; } // Changed to nullable

    public string PostalCode { get; set; } = null!;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool IsPrimary { get; set; }

    public bool IsVerified { get; set; }

    public DateTimeOffset? VerificationDate { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset UpdatedDate { get; set; }

    public virtual AddressType AddressType { get; set; } = null!;

    public virtual StateProvince? StateProvince { get; set; } // Changed to nullable
    
    public virtual TpaSodManagementUser? TpaUser { get; set; }
}
