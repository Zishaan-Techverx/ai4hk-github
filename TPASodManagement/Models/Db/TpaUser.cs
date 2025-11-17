using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TpaSodManagement.Models.Db;

public partial class TpaUser : IdentityUser<long>
{
    // Map UserId to Identity's Id property
    public long UserId
    {
        get => base.Id; // Use Identity's Id property
        set => base.Id = value; // Set Identity's Id property
    }

    public long PersonId { get; set; }

    public long FarmId { get; set; }

    public long UserRoleId { get; set; }
    
    public long AddressId { get; set; }
    
    public long WebsiteId { get; set; }

    public string Username
    {
        get => base.UserName; 
        set => base.UserName = value;
    }

    // These properties are already handled by IdentityUser, so you can remove or map them:
    // public string PasswordHash { get; set; } = null!;

    public string PasswordSalt { get; set; } = null!;

    // These properties are already handled by IdentityUser, so you can remove or map them:
    // public string Email { get; set; } = null!;

    public bool EmailVerified { get; set; }

    public string? EmailVerificationToken { get; set; }

    public string? PasswordResetToken { get; set; }

    public DateTimeOffset? PasswordResetExpires { get; set; }

    public DateTimeOffset? LastLoginDate { get; set; }

    public int FailedLoginAttempts { get; set; }

    public DateTimeOffset? AccountLockedUntil { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset UpdatedDate { get; set; }

    public int PdfErrorCount { get; set; }

    public bool SimpleModeEnabled { get; set; }

    public string? SimpleLogin { get; set; }

    public string? SimplePassword { get; set; }

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual Farm Farm { get; set; } = null!;
    
    public virtual Address Address { get; set; } = null!;

    public virtual ICollection<Field> Fields { get; set; } = new List<Field>();

    public virtual Person Person { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<Sale> SaleUpdatedByUsers { get; set; } = new List<Sale>();

    public virtual ICollection<Sale> SaleUsers { get; set; } = new List<Sale>();

    public virtual ICollection<Seeding> Seedings { get; set; } = new List<Seeding>();

    public virtual ICollection<Testimonial> Testimonials { get; set; } = new List<Testimonial>();

    public virtual UserRole UserRole { get; set; } = null!;

    public virtual ICollection<Waste> Wastes { get; set; } = new List<Waste>();
    
    public virtual Website Website { get; set; } = null!;
}
