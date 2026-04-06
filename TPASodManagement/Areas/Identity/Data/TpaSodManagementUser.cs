using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TpaSodManagement.Database.Entities;

namespace TpaSodManagement.Areas.Identity.Data;

public class TpaSodManagementUser : IdentityUser<long>
{
    public string? PrimaryContact { get; set; }
    public bool IsActive { get; set; }

    public long? OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    
    public long? AddressId { get; set; }
    public long? PersonId { get; set; }
    public long? WebsiteId { get; set; }
    public long? FarmId { get; set; }
    public Farm? Farm { get; set; }

    // Audit properties (same as AuditBaseEntity - cannot inherit due to IdentityUser<long> inheritance)
    public DateTimeOffset CreatedDate { get; set; }
    public long? CreatedByUserId { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }
    public long? UpdatedByUserId { get; set; }
    public DateTimeOffset? DeletedDate { get; set; }
    public long? DeletedByUserId { get; set; }
}

