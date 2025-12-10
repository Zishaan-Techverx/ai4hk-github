using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Models;

namespace TpaSodManagement.Data;

public class ApplicationDbContext : IdentityDbContext<TpaSodManagementUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

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
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
        builder.ApplyConfiguration(new IdentityTestUserEntityConfiguration());
    }
}

public class IdentityTestUserEntityConfiguration : IEntityTypeConfiguration<TpaSodManagementUser>
{
    public void Configure(EntityTypeBuilder<TpaSodManagementUser> builder)
    {
        builder.Property(u => u.PhoneNumber).HasMaxLength(20);
        builder.Property(u => u.PrimaryContact).HasMaxLength(20);
        builder.Property(u => u.IsActive).HasDefaultValue(false);
        builder.Property(u => u.OrganizationName).HasMaxLength(200);
        builder.Property(u => u.AddressId);
        builder.Property(u => u.PersonId);
        builder.Property(u => u.WebsiteId);
        builder.Property(u => u.FarmId);
    }
}
