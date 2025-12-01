using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TpaSodManagement.Models.Db;

public partial class SodDbContext : IdentityDbContext<TpaUser, UserRole, long>
{
    public SodDbContext()
    {
    }

    public SodDbContext(DbContextOptions<SodDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<AddressType> AddressTypes { get; set; }

    public virtual DbSet<AreaType> AreaTypes { get; set; }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<CertificateType> CertificateTypes { get; set; }

    // public virtual DbSet<Contact> Contacts { get; set; }

    // public virtual DbSet<ContactType> ContactTypes { get; set; }

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

    public virtual DbSet<TpaUser> TpaUsers { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<Waste> Wastes { get; set; }

    public virtual DbSet<WasteCertificate> WasteCertificates { get; set; }

    public virtual DbSet<WasteReason> WasteReasons { get; set; }

    public virtual DbSet<Website> Websites { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Read from environment variable (Production safe)
            var connectionString = Environment.GetEnvironmentVariable("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                // Fallback to local config if needed
                connectionString = "Your Local Dev Connection From AppSettings";
            }

            optionsBuilder.UseSqlServer(connectionString);
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK_Address");

            entity.ToTable("Address");

            // entity.HasIndex(e => new { e.EntityType, e.EntityId }, "IX_Address_Entity");

            // entity.HasIndex(e => new { e.EntityType, e.EntityId, e.IsPrimary }, "IX_Address_Primary").HasFilter("([IsPrimary]=(1))");

            entity.Property(e => e.AddressId).HasColumnName("AddressId");
            entity.Property(e => e.AddressLine1)
                .HasMaxLength(100)
                .HasColumnName("AddressLine1");
            entity.Property(e => e.AddressLine2)
                .HasMaxLength(100)
                .HasColumnName("AddressLine2");
            entity.Property(e => e.AddressTypeId).HasColumnName("AddressTypeId");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .HasColumnName("City");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            // entity.Property(e => e.EntityId).HasColumnName("EntityId");
            /* entity.Property(e => e.EntityType)
                .HasMaxLength(20)
                .HasColumnName("EntityType");
                */
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive");
            entity.Property(e => e.IsPrimary).HasColumnName("IsPrimary");
            entity.Property(e => e.IsVerified).HasColumnName("IsVerified");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(10, 7)")
                .HasColumnName("Latitude");
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(10, 7)")
                .HasColumnName("Longitude");
            entity.Property(e => e.PostalCode)
                .HasMaxLength(20)
                .HasColumnName("PostalCode");
            entity.Property(e => e.StateProvinceId).HasColumnName("StateProvinceId");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("UpdatedDate");
            entity.Property(e => e.VerificationDate).HasColumnName("VerificationDate");

            entity.HasOne(d => d.AddressType).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.AddressTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Address_AddressType");

            entity.HasOne(d => d.StateProvince).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.StateProvinceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Address_StateProvince");
        });

        modelBuilder.Entity<AddressType>(entity =>
        {
            entity.HasKey(e => e.AddressTypeId).HasName("PK_AddressType");

            entity.ToTable("AddressType");

            entity.HasIndex(e => e.AddressTypeCode, "UK_AddressType_Code").IsUnique();

            entity.Property(e => e.AddressTypeId).HasColumnName("AddressTypeId");
            entity.Property(e => e.AddressTypeCode)
                .HasMaxLength(20)
                .HasColumnName("AddressTypeCode");
            entity.Property(e => e.AddressTypeName)
                .HasMaxLength(50)
                .HasColumnName("AddressTypeName");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("Description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive");
        });

        modelBuilder.Entity<AreaType>(entity =>
        {
            entity.HasKey(e => e.AreaTypeId).HasName("PK_AreaType");

            entity.ToTable("AreaType");

            entity.HasIndex(e => e.AreaTypeName, "UK_AreaType_Name").IsUnique();

            entity.Property(e => e.AreaTypeId).HasColumnName("AreaTypeId");
            entity.Property(e => e.AreaTypeName)
                .HasMaxLength(25)
                .HasColumnName("AreaTypeName");
            entity.Property(e => e.ConversionToSquareMeters)
                .HasColumnType("decimal(18, 8)")
                .HasColumnName("ConversionToSquareMeters");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("Description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive");
            entity.Property(e => e.UnitAbbreviation)
                .HasMaxLength(10)
                .HasColumnName("UnitAbbreviation");
            entity.Property(e => e.UnitSystem)
                .HasMaxLength(10)
                .HasColumnName("UnitSystem");
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.CertificateId).HasName("PK_Certificate");

            entity.ToTable("Certificate");

            entity.HasIndex(e => e.FarmId, "IX_Certificate_Farm");

            entity.HasIndex(e => e.CertificateNumber, "IX_Certificate_Number");

            entity.HasIndex(e => e.StatusId, "IX_Certificate_Status");

            entity.HasIndex(e => e.UserId, "IX_Certificate_User");

            entity.HasIndex(e => e.CertificateNumber, "UK_Certificate_Number").IsUnique();

            entity.Property(e => e.CertificateId).HasColumnName("CertificateId");
            entity.Property(e => e.CertificateNumber)
                .HasMaxLength(100)
                .HasColumnName("CertificateNumber");
            entity.Property(e => e.CertificateRangeEnd)
                .HasMaxLength(100)
                .HasColumnName("CertificateRangeEnd");
            entity.Property(e => e.CertificateRangeStart)
                .HasMaxLength(100)
                .HasColumnName("CertificateRangeStart");
            entity.Property(e => e.CertificateTypeId).HasColumnName("CertificateTypeId");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.DigitalSignature).HasColumnName("DigitalSignature");
            entity.Property(e => e.ExpiryDate).HasColumnName("ExpiryDate");
            entity.Property(e => e.FarmId).HasColumnName("FarmId");
            entity.Property(e => e.IssueDate).HasColumnName("IssueDate");
            entity.Property(e => e.IssuingAuthority)
                .HasMaxLength(100)
                .HasColumnName("IssuingAuthority");
            entity.Property(e => e.IssuingOfficer)
                .HasMaxLength(100)
                .HasColumnName("IssuingOfficer");
            entity.Property(e => e.Notes).HasColumnName("Notes");
            entity.Property(e => e.RelatedSaleId).HasColumnName("RelatedSaleId");
            entity.Property(e => e.RelatedSeedingId).HasColumnName("RelatedSeedingId");
            entity.Property(e => e.RelatedWasteId).HasColumnName("RelatedWasteId");
            entity.Property(e => e.StatusId).HasColumnName("StatusId");
            entity.Property(e => e.UserId).HasColumnName("UserId");
            entity.Property(e => e.VerificationCode)
                .HasMaxLength(100)
                .HasColumnName("VerificationCode");

            entity.HasOne(d => d.CertificateType).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.CertificateTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Certificate_CertificateType");

            entity.HasOne(d => d.Farm).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.FarmId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Certificate_Farm");

            entity.HasOne(d => d.RelatedSale).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.RelatedSaleId)
                .HasConstraintName("FK_Certificate_Sale");

            entity.HasOne(d => d.RelatedSeeding).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.RelatedSeedingId)
                .HasConstraintName("FK_Certificate_Seeding");

            entity.HasOne(d => d.RelatedWaste).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.RelatedWasteId)
                .HasConstraintName("FK_Certificate_Waste");

            entity.HasOne(d => d.Status).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Certificate_Status");

            entity.HasOne(d => d.User).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Certificate_TpaUser");
        });

        modelBuilder.Entity<CertificateType>(entity =>
        {
            entity.HasKey(e => e.CertificateTypeId).HasName("PK_CertificateType");

            entity.ToTable("CertificateType");

            entity.HasIndex(e => e.CertificateTypeCode, "UK_CertificateType_Code").IsUnique();

            entity.Property(e => e.CertificateTypeId).HasColumnName("CertificateTypeId");
            entity.Property(e => e.CertificateTypeCode)
                .HasMaxLength(20)
                .HasColumnName("CertificateTypeCode");
            entity.Property(e => e.CertificateTypeName)
                .HasMaxLength(100)
                .HasColumnName("CertificateTypeName");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("Description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive");
            entity.Property(e => e.IssuingAuthority)
                .HasMaxLength(100)
                .HasColumnName("IssuingAuthority");
            entity.Property(e => e.RequiresRenewal).HasColumnName("RequiresRenewal");
            entity.Property(e => e.ValidityPeriodDays).HasColumnName("ValidityPeriodDays");
        });
        /*
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.ContactId).HasName("PK_Contact");

            entity.ToTable("Contact");

            // entity.HasIndex(e => new { e.EntityType, e.EntityId }, "IX_Contact_Entity");

            // entity.HasIndex(e => new { e.EntityType, e.EntityId, e.IsPrimary }, "IX_Contact_Primary").HasFilter("([IsPrimary]=(1))");

            entity.Property(e => e.ContactId).HasColumnName("ContactId");
            entity.Property(e => e.ContactTypeId).HasColumnName("ContactTypeId");
            entity.Property(e => e.ContactValue)
                .HasMaxLength(255)
                .HasColumnName("ContactValue");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            // entity.Property(e => e.EntityId).HasColumnName("EntityId");
            // entity.Property(e => e.EntityType)
            //  .HasMaxLength(20)
            //  .HasColumnName("EntityType");
                
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive");
            entity.Property(e => e.IsPrimary).HasColumnName("IsPrimary");
            entity.Property(e => e.IsVerified).HasColumnName("IsVerified");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("UpdatedDate");
            entity.Property(e => e.VerificationDate).HasColumnName("VerificationDate");

            entity.HasOne(d => d.ContactType).WithMany(p => p.Contacts)
                .HasForeignKey(d => d.ContactTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Contact_ContactType");
        });

        modelBuilder.Entity<ContactType>(entity =>
        {
            entity.HasKey(e => e.ContactTypeId).HasName("PK_ContactType");

            entity.ToTable("ContactType");

            entity.HasIndex(e => e.ContactTypeCode, "UK_ContactType_Code").IsUnique();

            entity.Property(e => e.ContactTypeId).HasColumnName("ContactTypeId");
            entity.Property(e => e.ContactTypeCode)
                .HasMaxLength(20)
                .HasColumnName("ContactTypeCode");
            entity.Property(e => e.ContactTypeName)
                .HasMaxLength(50)
                .HasColumnName("ContactTypeName");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive");
            entity.Property(e => e.ValidationPattern)
                .HasMaxLength(255)
                .HasColumnName("ValidationPattern");
        });

        */
        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.CountryId).HasName("PK_Country");

            entity.ToTable("Country");

            entity.HasIndex(e => e.CountryCode, "UK_Country_Code").IsUnique();

            entity.HasIndex(e => e.CountryName, "UK_Country_Name").IsUnique();

            entity.Property(e => e.CountryId).HasColumnName("CountryId");
            entity.Property(e => e.CountryCode)
                .HasMaxLength(3)
                .HasColumnName("CountryCode");
            entity.Property(e => e.CountryName)
                .HasMaxLength(100)
                .HasColumnName("CountryName");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .HasColumnName("CurrencyCode");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive");
            entity.Property(e => e.PhonePrefix)
                .HasMaxLength(10)
                .HasColumnName("PhonePrefix");
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.CurrencyId).HasName("PK_Currency");

            entity.ToTable("Currency");

            entity.HasIndex(e => e.CurrencyCode, "UK_Currency_Code").IsUnique();

            entity.Property(e => e.CurrencyId).HasColumnName("CurrencyId");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .HasColumnName("CurrencyCode");
            entity.Property(e => e.CurrencyName)
                .HasMaxLength(50)
                .HasColumnName("CurrencyName");
            entity.Property(e => e.CurrencySymbol)
                .HasMaxLength(5)
                .HasColumnName("CurrencySymbol");
            entity.Property(e => e.DecimalPlaces)
                .HasDefaultValue((byte)2)
                .HasColumnName("DecimalPlaces");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK_Customer");

            entity.ToTable("Customer");

            entity.HasIndex(e => e.IsActive, "IX_Customer_Active").HasFilter("([IsActive]=(1))");

            entity.HasIndex(e => e.CustomerType, "IX_Customer_Type");

            entity.Property(e => e.CustomerId).HasColumnName("CustomerId");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.CreditLimit)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("CreditLimit");
            entity.Property(e => e.CustomerCode)
                .HasMaxLength(20)
                .HasColumnName("CustomerCode");
            entity.Property(e => e.CustomerType)
                .HasMaxLength(20)
                .HasColumnName("CustomerType");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive");
            entity.Property(e => e.Notes).HasColumnName("Notes");
            entity.Property(e => e.OrganizationId).HasColumnName("OrganizationId");
            entity.Property(e => e.PaymentTermsDays)
                .HasDefaultValue(30)
                .HasColumnName("PaymentTermsDays");
            entity.Property(e => e.PersonId).HasColumnName("PersonId");
            entity.Property(e => e.TaxExempt).HasColumnName("TaxExempt");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("UpdatedDate");

            entity.HasOne(d => d.Organization).WithMany(p => p.Customers)
                .HasForeignKey(d => d.OrganizationId)
                .HasConstraintName("FK_Customer_Organization");

            entity.HasOne(d => d.Person).WithMany(p => p.Customers)
                .HasForeignKey(d => d.PersonId)
                .HasConstraintName("FK_Customer_Person");
        });

        modelBuilder.Entity<Farm>(entity =>
        {
            entity.HasKey(e => e.FarmId).HasName("PK_Farm");

            entity.ToTable("Farm");

            entity.HasIndex(e => e.OrganizationId, "IX_Farm_Organization");

            entity.HasIndex(e => e.OrganizationId, "UK_Farm_Organization").IsUnique();

            entity.Property(e => e.FarmId).HasColumnName("FarmId");
            entity.Property(e => e.AreaTypeId).HasColumnName("AreaTypeId");
            entity.Property(e => e.CertificationDetails).HasColumnName("CertificationDetails");
            entity.Property(e => e.ClimateZone)
                .HasMaxLength(50)
                .HasColumnName("ClimateZone");
            entity.Property(e => e.ElevationMeters)
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("ElevationMeters");
            entity.Property(e => e.IrrigationType)
                .HasMaxLength(100)
                .HasColumnName("IrrigationType");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(10, 7)")
                .HasColumnName("Latitude");
            entity.Property(e => e.LicenseNumber)
                .HasMaxLength(50)
                .HasColumnName("LicenseNumber");
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(10, 7)")
                .HasColumnName("Longitude");
            entity.Property(e => e.OrganicCertified).HasColumnName("OrganicCertified");
            entity.Property(e => e.OrganizationId).HasColumnName("OrganizationId");
            entity.Property(e => e.SoilType)
                .HasMaxLength(100)
                .HasColumnName("SoilType");
            entity.Property(e => e.TotalArea)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("TotalArea");

            entity.HasOne(d => d.AreaType).WithMany(p => p.Farms)
                .HasForeignKey(d => d.AreaTypeId)
                .HasConstraintName("FK_Farm_AreaType");
            
            entity.HasOne(d => d.Organization).WithMany(p => p.Farms)
                .HasForeignKey(d => d.OrganizationId)
                .HasConstraintName("FK_Farm_Organization");

        });

        modelBuilder.Entity<Field>(entity =>
        {
            entity.HasKey(e => e.FieldId).HasName("PK_Field");

            entity.ToTable("Field");

            entity.HasIndex(e => e.FarmId, "IX_Field_Farm");

            entity.HasIndex(e => new { e.FarmId, e.FieldName }, "UK_Field_NameFarm").IsUnique();

            entity.Property(e => e.FieldId).HasColumnName("FieldId");
            entity.Property(e => e.AreaAmount)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("AreaAmount");
            entity.Property(e => e.AreaTypeId).HasColumnName("AreaTypeId");
            entity.Property(e => e.BoundaryCoordinates).HasColumnName("BoundaryCoordinates");
            entity.Property(e => e.CreatedByUserId).HasColumnName("CreatedByUserId");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.FarmId).HasColumnName("FarmId");
            entity.Property(e => e.FieldCode)
                .HasMaxLength(20)
                .HasColumnName("FieldCode");
            entity.Property(e => e.FieldName)
                .HasMaxLength(100)
                .HasColumnName("FieldName");
            entity.Property(e => e.IrrigationAvailable).HasColumnName("IrrigationAvailable");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(10, 7)")
                .HasColumnName("Latitude");
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(10, 7)")
                .HasColumnName("Longitude");
            entity.Property(e => e.Notes).HasColumnName("Notes");
            entity.Property(e => e.SlopePercentage)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("SlopePercentage");
            entity.Property(e => e.SoilType)
                .HasMaxLength(100)
                .HasColumnName("SoilType");

            entity.HasOne(d => d.AreaType).WithMany(p => p.Fields)
                .HasForeignKey(d => d.AreaTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Field_AreaType");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Fields)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Field_TpaUser_CreatedBy");

            entity.HasOne(d => d.Farm).WithMany(p => p.Fields)
                .HasForeignKey(d => d.FarmId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Field_Farm");
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.OrganizationId).HasName("PK_Organization");

            entity.ToTable("Organization");

            entity.HasIndex(e => e.OrganizationName, "IX_Organization_Name");

            entity.HasIndex(e => e.OrganizationType, "IX_Organization_Type");

            entity.HasIndex(e => e.OrganizationName, "UK_Organization_Name").IsUnique();

            entity.Property(e => e.OrganizationId).HasColumnName("OrganizationId");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.Description).HasColumnName("Description");
            entity.Property(e => e.EstablishedDate).HasColumnName("EstablishedDate");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive");
            entity.Property(e => e.OrganizationCode)
                .HasMaxLength(20)
                .HasColumnName("OrganizationCode");
            entity.Property(e => e.OrganizationName)
                .HasMaxLength(100)
                .HasColumnName("OrganizationName");
            entity.Property(e => e.OrganizationType)
                .HasMaxLength(20)
                .HasColumnName("OrganizationType");
            entity.Property(e => e.RegistrationNumber)
                .HasMaxLength(50)
                .HasColumnName("RegistrationNumber");
            entity.Property(e => e.TaxIdentificationNumber)
                .HasMaxLength(50)
                .HasColumnName("TaxIdentificationNumber");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("UpdatedDate");
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.PersonId).HasName("PK_Person");

            entity.ToTable("Person");

            entity.HasIndex(e => new { e.LastName, e.FirstName }, "IX_Person_Name");

            entity.Property(e => e.PersonId).HasColumnName("PersonId");
            entity.Property(e => e.Bio).HasColumnName("Bio");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.DateOfBirth).HasColumnName("DateOfBirth");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("FirstName");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .HasColumnName("Gender");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("LastName");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(50)
                .HasColumnName("MiddleName");
            entity.Property(e => e.Title)
                .HasMaxLength(20)
                .HasColumnName("Title");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("UpdatedDate");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK_Product");

            entity.ToTable("Product");

            entity.HasIndex(e => e.ProductCode, "UK_Product_Code").IsUnique();

            entity.Property(e => e.ProductId).HasColumnName("ProductId");
            entity.Property(e => e.CertificateTypeId).HasColumnName("CertificateTypeId");
            entity.Property(e => e.CreatedByUserId).HasColumnName("CreatedByUserId");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.CurrencyId).HasColumnName("CurrencyId");
            entity.Property(e => e.Description).HasColumnName("Description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive");
            entity.Property(e => e.ProductCategoryId).HasColumnName("ProductCategoryId");
            entity.Property(e => e.ProductCode)
                .HasMaxLength(50)
                .HasColumnName("ProductCode");
            entity.Property(e => e.ProductName)
                .HasMaxLength(100)
                .HasColumnName("ProductName");
            entity.Property(e => e.RequiresCertificate).HasColumnName("RequiresCertificate");
            entity.Property(e => e.StandardPrice)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("StandardPrice");
            entity.Property(e => e.UnitOfMeasure)
                .HasMaxLength(20)
                .HasColumnName("UnitOfMeasure");

            entity.HasOne(d => d.CertificateType).WithMany(p => p.Products)
                .HasForeignKey(d => d.CertificateTypeId)
                .HasConstraintName("FK_Product_CertificateType");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Products)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_TpaUser_CreatedBy");

            entity.HasOne(d => d.Currency).WithMany(p => p.Products)
                .HasForeignKey(d => d.CurrencyId)
                .HasConstraintName("FK_Product_Currency");

            entity.HasOne(d => d.ProductCategory).WithMany(p => p.Products)
                .HasForeignKey(d => d.ProductCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_ProductCategory");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.ProductCategoryId).HasName("PK_ProductCategory");

            entity.ToTable("ProductCategory");

            entity.HasIndex(e => e.CategoryCode, "UK_ProductCategory_Code").IsUnique();

            entity.Property(e => e.ProductCategoryId).HasColumnName("ProductCategoryId");
            entity.Property(e => e.CategoryCode)
                .HasMaxLength(20)
                .HasColumnName("CategoryCode");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .HasColumnName("CategoryName");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("Description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive");
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.SaleId).HasName("PK_Sale");

            entity.ToTable("Sale");

            entity.HasIndex(e => e.CustomerId, "IX_Sale_Customer");

            entity.HasIndex(e => new { e.FarmId, e.SaleDate }, "IX_Sale_FarmDate");

            entity.HasIndex(e => e.StatusId, "IX_Sale_Status");

            entity.HasIndex(e => new { e.UserId, e.SaleDate }, "IX_Sale_UserDate");

            entity.HasIndex(e => e.SaleNumber, "UK_Sale_Number").IsUnique();

            entity.Property(e => e.SaleId).HasColumnName("SaleId");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.CurrencyId).HasColumnName("CurrencyId");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerId");
            entity.Property(e => e.DiscountAmount)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("DiscountAmount");
            entity.Property(e => e.DueDate).HasColumnName("DueDate");
            entity.Property(e => e.FarmId).HasColumnName("FarmId");
            entity.Property(e => e.InvoiceNumber)
                .HasMaxLength(100)
                .HasColumnName("InvoiceNumber");
            entity.Property(e => e.Notes).HasColumnName("Notes");
            entity.Property(e => e.PaymentTermsDays).HasColumnName("PaymentTermsDays");
            entity.Property(e => e.PurchaseOrderNumber)
                .HasMaxLength(100)
                .HasColumnName("PurchaseOrderNumber");
            entity.Property(e => e.SaleDate).HasColumnName("SaleDate");
            entity.Property(e => e.SaleNumber)
                .HasMaxLength(50)
                .HasColumnName("SaleNumber");
            entity.Property(e => e.SaleTypeId).HasColumnName("SaleTypeId");
            entity.Property(e => e.StatusId).HasColumnName("StatusId");
            entity.Property(e => e.SubtotalAmount)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("SubtotalAmount");
            entity.Property(e => e.TaxAmount)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("TaxAmount");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("TotalAmount");
            entity.Property(e => e.UpdatedByUserId).HasColumnName("UpdatedByUserId");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("UpdatedDate");
            entity.Property(e => e.UserId).HasColumnName("UserId");

            entity.HasOne(d => d.Currency).WithMany(p => p.Sales)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sale_Currency");

            entity.HasOne(d => d.Customer).WithMany(p => p.Sales)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sale_Customer");

            entity.HasOne(d => d.Farm).WithMany(p => p.Sales)
                .HasForeignKey(d => d.FarmId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sale_Farm");

            entity.HasOne(d => d.SaleType).WithMany(p => p.Sales)
                .HasForeignKey(d => d.SaleTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sale_SaleType");

            entity.HasOne(d => d.Status).WithMany(p => p.Sales)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sale_Status");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.SaleUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sale_TpaUser_UpdatedBy");

            entity.HasOne(d => d.User).WithMany(p => p.SaleUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sale_TpaUser");
        });

        modelBuilder.Entity<SaleLineItem>(entity =>
        {
            entity.HasKey(e => e.SaleLineItemId).HasName("PK_SaleLineItem");

            entity.ToTable("SaleLineItem");

            entity.HasIndex(e => e.SaleId, "IX_SaleLineItem_Sale");

            entity.HasIndex(e => new { e.SaleId, e.LineNumber }, "UK_SaleLineItem_LineNumber").IsUnique();

            entity.Property(e => e.SaleLineItemId).HasColumnName("SaleLineItemId");
            entity.Property(e => e.AreaAmount)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("AreaAmount");
            entity.Property(e => e.AreaTypeId).HasColumnName("AreaTypeId");
            entity.Property(e => e.CertificateSequenceNumber).HasColumnName("CertificateSequenceNumber");
            entity.Property(e => e.FieldId).HasColumnName("FieldId");
            entity.Property(e => e.LineNumber).HasColumnName("LineNumber");
            entity.Property(e => e.LineTotal)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("LineTotal");
            entity.Property(e => e.LotNumber)
                .HasMaxLength(100)
                .HasColumnName("LotNumber");
            entity.Property(e => e.Notes).HasColumnName("Notes");
            entity.Property(e => e.ProductId).HasColumnName("ProductId");
            entity.Property(e => e.Quantity)
                .HasColumnType("decimal(16, 4)")
                .HasColumnName("Quantity");
            entity.Property(e => e.RoyaltyInvoiceNumber)
                .HasMaxLength(100)
                .HasColumnName("RoyaltyInvoiceNumber");
            entity.Property(e => e.SaleId).HasColumnName("SaleId");
            entity.Property(e => e.SeedingId).HasColumnName("SeedingId");
            entity.Property(e => e.TagRangeId).HasColumnName("TagRangeId");
            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("UnitPrice");
            entity.Property(e => e.VerificationCode)
                .HasMaxLength(100)
                .HasColumnName("VerificationCode");

            entity.HasOne(d => d.AreaType).WithMany(p => p.SaleLineItems)
                .HasForeignKey(d => d.AreaTypeId)
                .HasConstraintName("FK_SaleLineItem_AreaType");

            entity.HasOne(d => d.Field).WithMany(p => p.SaleLineItems)
                .HasForeignKey(d => d.FieldId)
                .HasConstraintName("FK_SaleLineItem_Field");

            entity.HasOne(d => d.Product).WithMany(p => p.SaleLineItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SaleLineItem_Product");

            entity.HasOne(d => d.Sale).WithMany(p => p.SaleLineItems)
                .HasForeignKey(d => d.SaleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SaleLineItem_Sale");

            entity.HasOne(d => d.Seeding).WithMany(p => p.SaleLineItems)
                .HasForeignKey(d => d.SeedingId)
                .HasConstraintName("FK_SaleLineItem_Seeding");

            entity.HasOne(d => d.TagRange).WithMany(p => p.SaleLineItems)
                .HasForeignKey(d => d.TagRangeId)
                .HasConstraintName("FK_SaleLineItem_TagRange");
        });

        modelBuilder.Entity<SaleType>(entity =>
        {
            entity.HasKey(e => e.SaleTypeId).HasName("PK_SaleType");

            entity.ToTable("SaleType");

            entity.HasIndex(e => e.SaleTypeCode, "UK_SaleType_Code").IsUnique();

            entity.Property(e => e.SaleTypeId).HasColumnName("SaleTypeId");
            entity.Property(e => e.CertificateTypeId).HasColumnName("CertificateTypeId");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("Description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive");
            entity.Property(e => e.RequiresCertificate).HasColumnName("RequiresCertificate");
            entity.Property(e => e.SaleTypeCode)
                .HasMaxLength(20)
                .HasColumnName("SaleTypeCode");
            entity.Property(e => e.SaleTypeName)
                .HasMaxLength(50)
                .HasColumnName("SaleTypeName");
            entity.Property(e => e.TaxApplicable)
                .HasDefaultValue(true)
                .HasColumnName("TaxApplicable");

            entity.HasOne(d => d.CertificateType).WithMany(p => p.SaleTypes)
                .HasForeignKey(d => d.CertificateTypeId)
                .HasConstraintName("FK_SaleType_CertificateType");
        });

        modelBuilder.Entity<Seeding>(entity =>
        {
            entity.HasKey(e => e.SeedingId).HasName("PK_Seeding");

            entity.ToTable("Seeding");

            entity.HasIndex(e => new { e.FarmId, e.SeedingDate }, "IX_Seeding_FarmDate");

            entity.HasIndex(e => e.FieldId, "IX_Seeding_Field");

            entity.HasIndex(e => new { e.UserId, e.SeedingDate }, "IX_Seeding_UserDate");

            entity.Property(e => e.SeedingId).HasColumnName("SeedingId");
            entity.Property(e => e.AreaAmount)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("AreaAmount");
            entity.Property(e => e.AreaTypeId).HasColumnName("AreaTypeId");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.FarmId).HasColumnName("FarmId");
            entity.Property(e => e.FieldId).HasColumnName("FieldId");
            entity.Property(e => e.Notes).HasColumnName("Notes");
            entity.Property(e => e.SeedRatePerUnit)
                .HasColumnType("decimal(10, 4)")
                .HasColumnName("SeedRatePerUnit");
            entity.Property(e => e.SeedingDate).HasColumnName("SeedingDate");
            entity.Property(e => e.SeedingMethod)
                .HasMaxLength(50)
                .HasColumnName("SeedingMethod");
            entity.Property(e => e.SoilMoisture)
                .HasMaxLength(50)
                .HasColumnName("SoilMoisture");
            entity.Property(e => e.SoilTemperature)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("SoilTemperature");
            entity.Property(e => e.TagRangeId).HasColumnName("TagRangeId");
            entity.Property(e => e.UserId).HasColumnName("UserId");
            entity.Property(e => e.WeatherConditions)
                .HasMaxLength(255)
                .HasColumnName("WeatherConditions");

            entity.HasOne(d => d.AreaType).WithMany(p => p.Seedings)
                .HasForeignKey(d => d.AreaTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Seeding_AreaType");

            entity.HasOne(d => d.Farm).WithMany(p => p.Seedings)
                .HasForeignKey(d => d.FarmId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Seeding_Farm");

            entity.HasOne(d => d.Field).WithMany(p => p.Seedings)
                .HasForeignKey(d => d.FieldId)
                .HasConstraintName("FK_Seeding_Field");

            entity.HasOne(d => d.TagRange).WithMany(p => p.Seedings)
                .HasForeignKey(d => d.TagRangeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Seeding_TagRange");

            entity.HasOne(d => d.User).WithMany(p => p.Seedings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Seeding_TpaUser");
        });

        modelBuilder.Entity<StateProvince>(entity =>
        {
            entity.HasKey(e => e.StateProvinceId).HasName("PK_StateProvince");

            entity.ToTable("StateProvince");

            entity.HasIndex(e => new { e.StateCode, e.CountryId }, "UK_StateProvince_CodeCountry").IsUnique();

            entity.Property(e => e.StateProvinceId).HasColumnName("StateProvinceId");
            entity.Property(e => e.CountryId).HasColumnName("CountryId");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive");
            entity.Property(e => e.StateCode)
                .HasMaxLength(10)
                .HasColumnName("StateCode");
            entity.Property(e => e.StateName)
                .HasMaxLength(100)
                .HasColumnName("StateName");

            entity.HasOne(d => d.Country).WithMany(p => p.StateProvinces)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StateProvince_Country");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK_Status");

            entity.ToTable("Status");

            entity.HasIndex(e => new { e.StatusCategory, e.StatusCode }, "UK_Status_CategoryCode").IsUnique();

            entity.Property(e => e.StatusId).HasColumnName("StatusId");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("Description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive");
            entity.Property(e => e.IsFinalStatus).HasColumnName("IsFinalStatus");
            entity.Property(e => e.StatusCategory)
                .HasMaxLength(50)
                .HasColumnName("StatusCategory");
            entity.Property(e => e.StatusCode)
                .HasMaxLength(20)
                .HasColumnName("StatusCode");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .HasColumnName("StatusName");
            entity.Property(e => e.StatusOrder).HasColumnName("StatusOrder");
        });

        modelBuilder.Entity<TagRange>(entity =>
        {
            entity.HasKey(e => e.TagRangeId).HasName("PK_TagRange");

            entity.ToTable("TagRange");

            entity.HasIndex(e => e.TagRangeCode, "UK_TagRange_Code").IsUnique();

            entity.Property(e => e.TagRangeId).HasColumnName("TagRangeId");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive");
            entity.Property(e => e.TagEndNumber).HasColumnName("TagEndNumber");
            entity.Property(e => e.TagPrefix)
                .HasMaxLength(20)
                .HasColumnName("TagPrefix");
            entity.Property(e => e.TagRangeCode)
                .HasMaxLength(200)
                .HasColumnName("TagRangeCode");
            entity.Property(e => e.TagStartNumber).HasColumnName("TagStartNumber");
            entity.Property(e => e.TagSuffix)
                .HasMaxLength(20)
                .HasColumnName("TagSuffix");
            entity.Property(e => e.TotalTags).HasColumnName("TotalTags");
        });

        modelBuilder.Entity<Testimonial>(entity =>
        {
            entity.HasKey(e => e.TestimonialId).HasName("PK_Testimonial");

            entity.ToTable("Testimonial");

            entity.Property(e => e.TestimonialId).HasColumnName("TestimonialId");
            entity.Property(e => e.ApprovedByUserId).HasColumnName("ApprovedByUserId");
            entity.Property(e => e.ApprovedDate).HasColumnName("ApprovedDate");
            entity.Property(e => e.ApproverName)
                .HasMaxLength(100)
                .HasColumnName("ApproverName");
            entity.Property(e => e.Comments).HasColumnName("Comments");
            entity.Property(e => e.ContactNumber)
                .HasMaxLength(50)
                .HasColumnName("ContactNumber");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerId");
            entity.Property(e => e.DisplayOrder).HasColumnName("DisplayOrder");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("Email");
            entity.Property(e => e.FarmId).HasColumnName("FarmId");
            entity.Property(e => e.Featured).HasColumnName("Featured");
            entity.Property(e => e.Improvements).HasColumnName("Improvements");
            entity.Property(e => e.Location)
                .HasMaxLength(100)
                .HasColumnName("Location");
            entity.Property(e => e.Organization)
                .HasMaxLength(100)
                .HasColumnName("Organization");
            entity.Property(e => e.PersonId).HasColumnName("PersonId");
            entity.Property(e => e.PhotoReference)
                .HasMaxLength(100)
                .HasColumnName("PhotoReference");
            entity.Property(e => e.ProblemsToSolve).HasColumnName("ProblemsToSolve");
            entity.Property(e => e.PublishedDate).HasColumnName("PublishedDate");
            entity.Property(e => e.Rating).HasColumnName("Rating");
            entity.Property(e => e.StatusId).HasColumnName("StatusId");
            entity.Property(e => e.SubmittedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("SubmittedDate");
            entity.Property(e => e.TestimonialTitle)
                .HasMaxLength(200)
                .HasColumnName("TestimonialTitle");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("Username");
            entity.Property(e => e.VideoReference)
                .HasMaxLength(100)
                .HasColumnName("VideoReference");
            entity.Property(e => e.WouldRecommend).HasColumnName("WouldRecommend");

            entity.HasOne(d => d.ApprovedByUser).WithMany(p => p.Testimonials)
                .HasForeignKey(d => d.ApprovedByUserId)
                .HasConstraintName("FK_Testimonial_TpaUser_ApprovedBy");

            entity.HasOne(d => d.Customer).WithMany(p => p.Testimonials)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Testimonial_Customer");

            entity.HasOne(d => d.Farm).WithMany(p => p.Testimonials)
                .HasForeignKey(d => d.FarmId)
                .HasConstraintName("FK_Testimonial_Farm");

            entity.HasOne(d => d.Person).WithMany(p => p.Testimonials)
                .HasForeignKey(d => d.PersonId)
                .HasConstraintName("FK_Testimonial_Person");

            entity.HasOne(d => d.Status).WithMany(p => p.Testimonials)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Testimonial_Status");
        });

        modelBuilder.Entity<TpaUser>(entity =>
        {
            entity.Ignore(e => e.Id);
            
            entity.Ignore(e => e.UserName);
            
            entity.HasKey(e => e.UserId).HasName("PK_TpaUser");

            entity.ToTable("TpaUser");

            entity.HasIndex(e => e.IsActive, "IX_TpaUser_Active").HasFilter("([IsActive]=(1))");

            entity.HasIndex(e => e.Email, "IX_TpaUser_Email");

            entity.HasIndex(e => e.FarmId, "IX_TpaUser_Farm");

            entity.HasIndex(e => e.Username, "IX_TpaUser_Username");

            entity.HasIndex(e => e.Email, "UK_TpaUser_Email").IsUnique();

            entity.HasIndex(e => e.PersonId, "UK_TpaUser_Person").IsUnique();

            entity.HasIndex(e => e.Username, "UK_TpaUser_Username").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserId");
            entity.Property(e => e.AccountLockedUntil).HasColumnName("AccountLockedUntil");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("Email");
            entity.Property(e => e.EmailVerificationToken)
                .HasMaxLength(255)
                .HasColumnName("EmailVerificationToken");
            entity.Property(e => e.EmailVerified).HasColumnName("EmailVerified");
            entity.Property(e => e.FailedLoginAttempts).HasColumnName("FailedLoginAttempts");
            entity.Property(e => e.FarmId).HasColumnName("FarmId");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive");
            entity.Property(e => e.LastLoginDate).HasColumnName("LastLoginDate");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("PasswordHash");
            entity.Property(e => e.PasswordResetExpires).HasColumnName("PasswordResetExpires");
            entity.Property(e => e.PasswordResetToken)
                .HasMaxLength(255)
                .HasColumnName("PasswordResetToken");
            entity.Property(e => e.PasswordSalt)
                .HasMaxLength(255)
                .HasColumnName("PasswordSalt");
            entity.Property(e => e.PdfErrorCount).HasColumnName("PdfErrorCount");
            entity.Property(e => e.PersonId).HasColumnName("PersonId");
            entity.Property(e => e.SimpleLogin)
                .HasMaxLength(100)
                .HasColumnName("SimpleLogin");
            entity.Property(e => e.SimpleModeEnabled).HasColumnName("SimpleModeEnabled");
            entity.Property(e => e.SimplePassword)
                .HasMaxLength(100)
                .HasColumnName("SimplePassword");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("UpdatedDate");
            entity.Property(e => e.UserRoleId).HasColumnName("UserRoleId");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("Username");
            
            entity.Property(e => e.AddressId).HasColumnName("AddressId");
            
            entity.Property(e => e.WebsiteId).HasColumnName("WebsiteId");

            entity.HasOne(d => d.Farm).WithMany(p => p.TpaUsers)
                .HasForeignKey(d => d.FarmId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TpaUser_Farm");

            entity.HasOne(d => d.Person).WithOne(p => p.TpaUser)
                .HasForeignKey<TpaUser>(d => d.PersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TpaUser_Person");

            entity.HasOne(d => d.UserRole).WithMany(p => p.TpaUsers)
                .HasForeignKey(d => d.UserRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TpaUser_Role");
            
            entity.HasOne(d => d.Address).WithOne(p => p.TpaUser)
                .HasForeignKey<TpaUser>(d => d.AddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TpaUser_Address");
            
            entity.HasOne(d => d.Website).WithMany(p => p.TpaUsers)
                .HasForeignKey(d => d.WebsiteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TpaUser_Website");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.Ignore(e => e.Id);
            entity.Ignore(e => e.Name);
            
            entity.HasKey(e => e.UserRoleId).HasName("PK_UserRole");

            entity.ToTable("UserRole");

            entity.HasIndex(e => e.RoleCode, "UK_UserRole_Code").IsUnique();

            entity.Property(e => e.UserRoleId).HasColumnName("UserRoleId");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("Description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive");
            entity.Property(e => e.RoleCode)
                .HasMaxLength(20)
                .HasColumnName("RoleCode");
            entity.Property(e => e.RoleLevel).HasColumnName("RoleLevel");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("RoleName");
        });

        modelBuilder.Entity<Waste>(entity =>
        {
            entity.HasKey(e => e.WasteId).HasName("PK_Waste");

            entity.ToTable("Waste");

            entity.HasIndex(e => new { e.FarmId, e.WasteDate }, "IX_Waste_FarmDate");

            entity.HasIndex(e => new { e.UserId, e.WasteDate }, "IX_Waste_UserDate");

            entity.Property(e => e.WasteId).HasColumnName("WasteId");
            entity.Property(e => e.AreaAmount)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("AreaAmount");
            entity.Property(e => e.AreaTypeId).HasColumnName("AreaTypeId");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.CurrencyId).HasColumnName("CurrencyId");
            entity.Property(e => e.DisposalLocation)
                .HasMaxLength(255)
                .HasColumnName("DisposalLocation");
            entity.Property(e => e.DisposalMethod)
                .HasMaxLength(100)
                .HasColumnName("DisposalMethod");
            entity.Property(e => e.EnvironmentalImpactAssessed).HasColumnName("EnvironmentalImpactAssessed");
            entity.Property(e => e.EstimatedLossValue)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("EstimatedLossValue");
            entity.Property(e => e.FarmId).HasColumnName("FarmId");
            entity.Property(e => e.FieldId).HasColumnName("FieldId");
            entity.Property(e => e.Notes).HasColumnName("Notes");
            entity.Property(e => e.RegulatoryReported).HasColumnName("RegulatoryReported");
            entity.Property(e => e.SeedingId).HasColumnName("SeedingId");
            entity.Property(e => e.UserId).HasColumnName("UserId");
            entity.Property(e => e.WasteDate).HasColumnName("WasteDate");
            entity.Property(e => e.WasteReasonId).HasColumnName("WasteReasonId");

            entity.HasOne(d => d.AreaType).WithMany(p => p.Wastes)
                .HasForeignKey(d => d.AreaTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Waste_AreaType");

            entity.HasOne(d => d.Currency).WithMany(p => p.Wastes)
                .HasForeignKey(d => d.CurrencyId)
                .HasConstraintName("FK_Waste_Currency");

            entity.HasOne(d => d.Farm).WithMany(p => p.Wastes)
                .HasForeignKey(d => d.FarmId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Waste_Farm");

            entity.HasOne(d => d.Field).WithMany(p => p.Wastes)
                .HasForeignKey(d => d.FieldId)
                .HasConstraintName("FK_Waste_Field");

            entity.HasOne(d => d.Seeding).WithMany(p => p.Wastes)
                .HasForeignKey(d => d.SeedingId)
                .HasConstraintName("FK_Waste_Seeding");

            entity.HasOne(d => d.User).WithMany(p => p.Wastes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Waste_User");

            entity.HasOne(d => d.WasteReason).WithMany(p => p.Wastes)
                .HasForeignKey(d => d.WasteReasonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Waste_Reason");
        });

        modelBuilder.Entity<WasteCertificate>(entity =>
        {
            entity.HasKey(e => e.WasteCertificateId).HasName("PK_WasteCertificate");

            entity.ToTable("WasteCertificate");

            entity.HasIndex(e => e.CertificateNumber, "UK_WasteCertificate_Number").IsUnique();

            entity.Property(e => e.WasteCertificateId).HasColumnName("WasteCertificateId");
            entity.Property(e => e.CertificateId).HasColumnName("CertificateId");
            entity.Property(e => e.CertificateNumber).HasColumnName("CertificateNumber");
            entity.Property(e => e.Notes).HasColumnName("Notes");
            entity.Property(e => e.WasteId).HasColumnName("WasteId");

            entity.HasOne(d => d.Certificate).WithMany(p => p.WasteCertificates)
                .HasForeignKey(d => d.CertificateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WasteCertificate_Certificate");

            entity.HasOne(d => d.Waste).WithMany(p => p.WasteCertificates)
                .HasForeignKey(d => d.WasteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WasteCertificate_waste");
        });

        modelBuilder.Entity<WasteReason>(entity =>
        {
            entity.HasKey(e => e.WasteReasonId).HasName("PK_WasteReason");

            entity.ToTable("WasteReason");

            entity.HasIndex(e => e.ReasonCode, "UK_WasteReason_Code").IsUnique();

            entity.Property(e => e.WasteReasonId).HasColumnName("WasteReasonId");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("Description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive");
            entity.Property(e => e.ReasonCategory)
                .HasMaxLength(50)
                .HasColumnName("ReasonCategory");
            entity.Property(e => e.ReasonCode)
                .HasMaxLength(20)
                .HasColumnName("ReasonCode");
            entity.Property(e => e.ReasonName)
                .HasMaxLength(100)
                .HasColumnName("ReasonName");
        });

        modelBuilder.Entity<Website>(entity =>
        {
            entity.HasKey(e => e.WebsiteId).HasName("PK_Website");

            entity.ToTable("Website");

            entity.Property(e => e.WebsiteId).HasColumnName("WebsiteId");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("CreatedDate");
            // entity.Property(e => e.EntityId).HasColumnName("EntityId");
            /* entity.Property(e => e.EntityType)
                .HasMaxLength(20)
                .HasColumnName("EntityType");
                */
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive");
            entity.Property(e => e.IsPrimary).HasColumnName("IsPrimary");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("UpdatedDate");
            entity.Property(e => e.WebsiteType)
                .HasMaxLength(50)
                .HasColumnName("WebsiteType");
            entity.Property(e => e.WebsiteUrl)
                .HasMaxLength(255)
                .HasColumnName("WebsiteUrl");
        });
        
        // Configure composite key for IdentityUserLogin<long>
        modelBuilder.Entity<IdentityUserLogin<long>>(entity =>
        {
            entity.HasKey(login => new { login.LoginProvider, login.ProviderKey });
            
            // Define the foreign key relationship with AspNetUsers
            entity.HasOne<TpaUser>()
                .WithMany() // No navigation property by default
                .HasForeignKey(e => e.UserId)
                .IsRequired();

        });
        // Configure composite primary key for IdentityUserRole<long>
        modelBuilder.Entity<IdentityUserRole<long>>(entity =>
        {
            entity.HasKey(role => new { role.UserId, role.RoleId });
            
            
            // Define foreign key relationship to AspNetUsers
            entity.HasOne<TpaUser>()
                .WithMany() // No navigation property by default
                .HasForeignKey(e => e.UserId)
                .IsRequired();

            // Define foreign key relationship to AspNetRoles
            entity.HasOne<UserRole>()
                .WithMany() // No navigation property by default
                .HasForeignKey(e => e.RoleId)
                .IsRequired();
        });
        // Configure composite primary key for IdentityUserToken<long>
        modelBuilder.Entity<IdentityUserToken<long>>(entity =>
        {
            entity.HasKey(token => new { token.UserId, token.LoginProvider, token.Name });
            
            // Foreign key relationship with AspNetUsers
            entity.HasOne<TpaUser>()
                .WithMany() // No navigation property in IdentityUser by default
                .HasForeignKey(t => t.UserId)
                .IsRequired();
        });
        modelBuilder.Entity<IdentityUserClaim<long>>(entity =>
        {
            // Rename 'Id' column to 'UserClaimsId'
            entity.Property(e => e.Id).HasColumnName("UserClaimsId");

            // Define the foreign key relationship to AspNetUsers
            entity.HasOne<TpaUser>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .IsRequired();

        });
        // Customize AspNetRoleClaims table
        modelBuilder.Entity<IdentityRoleClaim<long>>(entity =>
        {
            // Set primary key
            // Rename 'Id' column to 'UserClaimsId'
            entity.Property(e => e.Id).HasColumnName("UserClaimsId");
            
            // Define foreign key relationship to AspNetRoles
            entity.HasOne<UserRole>()
                .WithMany() // No navigation property by default
                .HasForeignKey(e => e.RoleId)
                .IsRequired();
        });
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
