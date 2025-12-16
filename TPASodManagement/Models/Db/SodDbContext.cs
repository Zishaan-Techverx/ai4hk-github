using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Areas.Identity.Data;

namespace TpaSodManagement.Models.Db;

public partial class SodDbContext : DbContext
{
    public SodDbContext()
    {
    }

    // Use a different DbContextOptions type to avoid migration conflicts
    public SodDbContext(DbContextOptions<SodDbContext> options)
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
    // TpaSodManagementUser is in ApplicationDbContext, not here
    
    public virtual DbSet<Waste> Wastes { get; set; }
    public virtual DbSet<WasteCertificate> WasteCertificates { get; set; }
    public virtual DbSet<WasteReason> WasteReasons { get; set; }
    public virtual DbSet<Website> Websites { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = Environment.GetEnvironmentVariable("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = "Your Local Dev Connection From AppSettings";
            }
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TpaUser and UserRole have been removed - they are replaced by TpaSodManagementUser and IdentityRole
        
        modelBuilder.Entity<TpaSodManagementUser>(entity =>
        {
            entity.ToTable("AspNetUsers");
            entity.HasKey(e => e.Id);
        });

        
        modelBuilder.Entity<Sale>(entity =>
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

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Certificate_TpaSodManagementUser");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasOne(d => d.CreatedByUser)
                .WithMany()
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Product_TpaSodManagementUser_CreatedBy");
        });

        modelBuilder.Entity<Seeding>(entity =>
        {
            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Seeding_TpaSodManagementUser");
        });

        modelBuilder.Entity<Testimonial>(entity =>
        {
            entity.HasOne(d => d.ApprovedByUser)
                .WithMany()
                .HasForeignKey(d => d.ApprovedByUserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Testimonial_TpaSodManagementUser_ApprovedBy");
        });

        modelBuilder.Entity<Waste>(entity =>
        {
            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Waste_TpaSodManagementUser");
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasOne(d => d.TpaUser)
                .WithOne()
                .HasForeignKey<TpaSodManagementUser>(u => u.AddressId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Farm>(entity =>
        {
            entity.HasMany(d => d.TpaUsers)
                .WithOne()
                .HasForeignKey(u => u.FarmId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasOne(d => d.TpaUser)
                .WithOne()
                .HasForeignKey<TpaSodManagementUser>(u => u.PersonId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Website>(entity =>
        {
            entity.HasMany(d => d.TpaUsers)
                .WithOne()
                .HasForeignKey(u => u.WebsiteId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<WasteCertificate>(entity =>
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

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasOne(d => d.RelatedWaste)
                .WithMany()
                .HasForeignKey(d => d.RelatedWasteId)
                .OnDelete(DeleteBehavior.NoAction) 
                .HasConstraintName("FK_Certificate_Waste");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}