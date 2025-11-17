using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TpaSodManagement.Models.Db;

public partial class UserRole : IdentityRole<long>
{
    public long UserRoleId
    {
        get => base.Id; 
        set => base.Id = value;
    }

    public string RoleCode { get; set; } = null!;

    public string RoleName 
    {
        get => base.Name; 
        set => base.Name = value;
    }

    public int RoleLevel { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public virtual ICollection<TpaUser> TpaUsers { get; set; } = new List<TpaUser>();
}
