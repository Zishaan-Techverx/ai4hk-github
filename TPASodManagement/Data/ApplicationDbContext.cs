using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Models;
using TpaSodManagement.Models.Db;

namespace TpaSodManagement.Data;

public class ApplicationDbContext : IdentityDbContext<TpaSodManagementUser, IdentityRole<long>, long>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Organization> Organizations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Permission");

            entity.ToTable("Permission");

            entity.HasIndex(e => e.Name, "UK_Permission_Name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("PermissionId");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("PermissionName");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("Description");
            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .HasColumnName("Category");
        });

        builder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermission");

            entity.HasKey(e => e.Id)
                  .HasName("PK_RolePermission");
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }, "UK_RolePermission_RolePermission")
                  .IsUnique();
            entity.Property(e => e.Id)
                  .HasColumnName("RolePermissionId");
            entity.HasOne(d => d.Role)
                .WithMany()
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RolePermission_Role");
            entity.HasOne(d => d.Permission)
                .WithMany()
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RolePermission_Permission");
        });
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new IdentityTestUserEntityConfiguration());
    }
}

public class IdentityTestUserEntityConfiguration : IEntityTypeConfiguration<TpaSodManagementUser>
{
    public void Configure(EntityTypeBuilder<TpaSodManagementUser> builder)
    {
        builder.Property(u => u.PhoneNumber).HasDefaultValue(string.Empty);
        builder.Property(u => u.PrimaryContact).HasMaxLength(20);
        builder.Property(u => u.IsActive).HasDefaultValue(false);
        
        // OrganizationId foreign key relationship
        builder.Property(u => u.OrganizationId);
        builder.HasOne(u => u.Organization)
            .WithMany(o => o.Users)
            .HasForeignKey(u => u.OrganizationId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_AspNetUsers_Organizations");
        
        builder.Property(u => u.AddressId);
        builder.Property(u => u.PersonId);
        builder.Property(u => u.WebsiteId);
        builder.Property(u => u.FarmId);
        
        builder.Property(u => u.EmailConfirmed).HasDefaultValue(false);
        builder.Property(u => u.PhoneNumberConfirmed).HasDefaultValue(false);
        builder.Property(u => u.TwoFactorEnabled).HasDefaultValue(false);
        builder.Property(u => u.LockoutEnabled).HasDefaultValue(false);
        builder.Property(u => u.AccessFailedCount).HasDefaultValue(0);
    }
}
