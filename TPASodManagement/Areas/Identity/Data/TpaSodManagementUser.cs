using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TpaSodManagement.Areas.Identity.Data;

public class TpaSodManagementUser : IdentityUser<long>
{
    public string? PrimaryContact { get; set; }
    public bool IsActive { get; set; }

    [StringLength(100)]
    public string? OrganizationName { get; set; }
    
    public long? AddressId { get; set; }
    public long? PersonId { get; set; }
    public long? WebsiteId { get; set; }
    public long? FarmId { get; set; }
}

