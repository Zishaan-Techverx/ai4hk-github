using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;
using System.Reflection;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Database.Entities;

namespace TpaSodManagement.Database;

public class ApplicationDbContext : IdentityDbContext<TpaSodManagementUser, IdentityRole<long>, long>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }
    public virtual DbSet<AddressType> AddressTypes { get; set; }
    public virtual DbSet<AreaType> AreaTypes { get; set; }
    public virtual DbSet<Certificate> Certificates { get; set; }
    public virtual DbSet<CertificateType> CertificateTypes { get; set; }
    public virtual DbSet<Country> Countries { get; set; }
    public virtual DbSet<Currency> Currencies { get; set; }
    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<Farm> Farms { get; set; }
    public virtual DbSet<Field> Fields { get; set; }
    public virtual DbSet<Organization> Organizations { get; set; }
    public virtual DbSet<Person> People { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<ProductCategory> ProductCategories { get; set; }
    public virtual DbSet<Sale> Sales { get; set; }
    public virtual DbSet<SaleLineItem> SaleLineItems { get; set; }
    public virtual DbSet<SaleType> SaleTypes { get; set; }
    public virtual DbSet<Seeding> Seedings { get; set; }
    public virtual DbSet<StateProvince> StateProvinces { get; set; }
    public virtual DbSet<Status> Statuses { get; set; }
    public virtual DbSet<TagRange> TagRanges { get; set; }
    public virtual DbSet<Testimonial> Testimonials { get; set; }
    public virtual DbSet<Waste> Wastes { get; set; }
    public virtual DbSet<WasteCertificate> WasteCertificates { get; set; }
    public virtual DbSet<WasteReason> WasteReasons { get; set; }
    public virtual DbSet<Website> Websites { get; set; }
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

        // Sale entity relationships
        builder.Entity<Sale>(entity =>
        {
            entity.HasOne(d => d.UpdatedByUser)
                .WithMany()
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Sale_TpaSodManagementUser_UpdatedBy");
            
            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Sale_TpaSodManagementUser");
        });

        // Certificate entity relationships
        builder.Entity<Certificate>(entity =>
        {
            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Certificate_TpaSodManagementUser");
            
            entity.HasOne(d => d.RelatedWaste)
                .WithMany()
                .HasForeignKey(d => d.RelatedWasteId)
                .OnDelete(DeleteBehavior.NoAction) 
                .HasConstraintName("FK_Certificate_Waste");
        });

        // Product entity relationships
        builder.Entity<Product>(entity =>
        {
            entity.HasOne(d => d.CreatedByUser)
                .WithMany()
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Product_TpaSodManagementUser_CreatedBy");
        });

        // Seeding entity relationships
        builder.Entity<Seeding>(entity =>
        {
            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Seeding_TpaSodManagementUser");
        });

        // Testimonial entity relationships
        builder.Entity<Testimonial>(entity =>
        {
            entity.HasOne(d => d.ApprovedByUser)
                .WithMany()
                .HasForeignKey(d => d.ApprovedByUserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Testimonial_TpaSodManagementUser_ApprovedBy");
        });

        // Waste entity relationships
        builder.Entity<Waste>(entity =>
        {
            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Waste_TpaSodManagementUser");
        });

        // Address entity relationships
        builder.Entity<Address>(entity =>
        {
            entity.HasOne(d => d.TpaUser)
                .WithOne()
                .HasForeignKey<TpaSodManagementUser>(u => u.AddressId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Farm entity relationships
        builder.Entity<Farm>(entity =>
        {
            entity.HasMany(d => d.TpaUsers)
                .WithOne()
                .HasForeignKey(u => u.FarmId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Person entity relationships
        builder.Entity<Person>(entity =>
        {
            entity.HasOne(d => d.TpaUser)
                .WithOne()
                .HasForeignKey<TpaSodManagementUser>(u => u.PersonId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Website entity relationships
        builder.Entity<Website>(entity =>
        {
            entity.HasMany(d => d.TpaUsers)
                .WithOne()
                .HasForeignKey(u => u.WebsiteId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // WasteCertificate entity relationships
        builder.Entity<WasteCertificate>(entity =>
        {
            entity.HasKey(e => e.WasteCertificateId).HasName("PK_WasteCertificate");

            entity.ToTable("WasteCertificate");

            entity.Property(e => e.WasteCertificateId).HasColumnName("WasteCertificateId");
            entity.Property(e => e.CertificateId).HasColumnName("CertificateId");
            entity.Property(e => e.WasteId).HasColumnName("WasteId");
            entity.Property(e => e.CertificateNumber).HasColumnName("CertificateNumber");
            entity.Property(e => e.Notes).HasColumnName("Notes");

            entity.HasOne(d => d.Certificate)
                .WithMany(p => p.WasteCertificates)
                .HasForeignKey(d => d.CertificateId)
                .OnDelete(DeleteBehavior.NoAction) 
                .HasConstraintName("FK_WasteCertificate_Certificate");

            entity.HasOne(d => d.Waste)
                .WithMany(p => p.WasteCertificates)
                .HasForeignKey(d => d.WasteId)
                .OnDelete(DeleteBehavior.NoAction) 
                .HasConstraintName("FK_WasteCertificate_waste");
        });

        // Configure soft delete properties for all entities
        builder.Entity<Address>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<AddressType>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<AreaType>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<Certificate>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<CertificateType>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<Country>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<Currency>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<Customer>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<Farm>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<Field>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<Organization>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<Person>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<Product>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<ProductCategory>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<Sale>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<SaleLineItem>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<SaleType>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<Seeding>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<StateProvince>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<Status>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<TagRange>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<Testimonial>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<Waste>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<WasteCertificate>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<WasteReason>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<Website>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<Permission>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        builder.Entity<RolePermission>(entity =>
        {
            entity.Property(e => e.DeletedByUserId);
            entity.Property(e => e.DeletedDate);
        });

        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new IdentityTestUserEntityConfiguration());

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var deletedDateProperty = entityType.ClrType.GetProperty("DeletedDate");
            if (deletedDateProperty != null && deletedDateProperty.PropertyType == typeof(DateTimeOffset?))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(SetGlobalQueryFilter), BindingFlags.NonPublic | BindingFlags.Static)
                    ?.MakeGenericMethod(entityType.ClrType);
                method?.Invoke(null, new object[] { builder });
            }
        }
    }

    private static void SetGlobalQueryFilter<TEntity>(ModelBuilder builder) where TEntity : class
    {
        builder.Entity<TEntity>().HasQueryFilter(e => EF.Property<DateTimeOffset?>(e, "DeletedDate") == null);
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
        
        // Audit properties (same as AuditBaseEntity)
        builder.Property(u => u.CreatedDate);
        builder.Property(u => u.CreatedByUserId);
        builder.Property(u => u.UpdatedDate);
        builder.Property(u => u.UpdatedByUserId);
        builder.Property(u => u.DeletedDate);
        builder.Property(u => u.DeletedByUserId);
        // Note: Soft delete query filter is applied globally in OnModelCreating
        
        builder.Property(u => u.EmailConfirmed).HasDefaultValue(false);
        builder.Property(u => u.PhoneNumberConfirmed).HasDefaultValue(false);
        builder.Property(u => u.TwoFactorEnabled).HasDefaultValue(false);
        builder.Property(u => u.LockoutEnabled).HasDefaultValue(false);
        builder.Property(u => u.AccessFailedCount).HasDefaultValue(0);
    }
}
