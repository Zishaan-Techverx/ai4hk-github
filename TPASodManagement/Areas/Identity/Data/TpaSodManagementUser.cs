using Microsoft.AspNetCore.Identity;
using NuGet.ProjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TpaSodManagement.Areas.Identity.Data;

public class TpaSodManagementUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int PhoneNumber { get; set; }
    public string? PostalCode { get; set; }
    public string? Address { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public int? PrimaryContact { get; set; }
    public bool IsActive { get; set; }

    [StringLength(100)]
    public string OrganizationName { get; set; }
}

