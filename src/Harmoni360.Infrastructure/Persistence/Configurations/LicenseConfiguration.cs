using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class LicenseConfiguration : IEntityTypeConfiguration<License>
{
    public void Configure(EntityTypeBuilder<License> builder)
    {
        builder.ToTable("Licenses");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.LicenseNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(l => l.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Description)
            .HasMaxLength(2000);

        builder.Property(l => l.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(l => l.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(l => l.Priority)
            .IsRequired()
            .HasConversion<int>();

        // Issuing Information
        builder.Property(l => l.IssuingAuthority)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.IssuingAuthorityContact)
            .HasMaxLength(200);

        builder.Property(l => l.IssuedDate)
            .IsRequired();

        builder.Property(l => l.ExpiryDate)
            .IsRequired();

        builder.Property(l => l.IssuedLocation)
            .HasMaxLength(200);

        // Renewal Information
        builder.Property(l => l.RenewalRequired)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(l => l.RenewalPeriodDays)
            .HasDefaultValue(90);

        builder.Property(l => l.RenewalProcedure)
            .HasMaxLength(1000);

        // Regulatory Information
        builder.Property(l => l.RegulatoryFramework)
            .HasMaxLength(500);

        builder.Property(l => l.ApplicableRegulations)
            .HasMaxLength(2000);

        builder.Property(l => l.ComplianceStandards)
            .HasMaxLength(2000);

        // Scope and Coverage
        builder.Property(l => l.Scope)
            .HasMaxLength(2000);

        builder.Property(l => l.CoverageAreas)
            .HasMaxLength(1000);

        builder.Property(l => l.Restrictions)
            .HasMaxLength(2000);

        builder.Property(l => l.Conditions)
            .HasMaxLength(2000);

        // Business Information
        builder.Property(l => l.HolderId)
            .IsRequired();

        builder.Property(l => l.HolderName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.Department)
            .HasMaxLength(100);

        builder.Property(l => l.LicenseFee)
            .HasColumnType("decimal(18,2)");

        builder.Property(l => l.Currency)
            .HasMaxLength(3)
            .HasDefaultValue("USD");

        // Risk and Compliance
        builder.Property(l => l.RiskLevel)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(RiskLevel.Medium);

        builder.Property(l => l.IsCriticalLicense)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(l => l.RequiresInsurance)
            .HasDefaultValue(false);

        builder.Property(l => l.RequiredInsuranceAmount)
            .HasColumnType("decimal(18,2)");

        // Status Notes
        builder.Property(l => l.StatusNotes)
            .HasMaxLength(2000);

        // Audit fields
        builder.Property(l => l.CreatedAt)
            .IsRequired();

        builder.Property(l => l.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.LastModifiedAt);

        builder.Property(l => l.LastModifiedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasMany(l => l.Attachments)
            .WithOne(a => a.License)
            .HasForeignKey(a => a.LicenseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Renewals)
            .WithOne(r => r.License)
            .HasForeignKey(r => r.LicenseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.LicenseConditions)
            .WithOne(c => c.License)
            .HasForeignKey(c => c.LicenseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.AuditLogs)
            .WithOne(a => a.License)
            .HasForeignKey(a => a.LicenseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(l => l.LicenseNumber)
            .IsUnique();

        builder.HasIndex(l => new { l.LicenseNumber, l.IssuingAuthority })
            .IsUnique()
            .HasDatabaseName("IX_Licenses_Number_Authority");

        builder.HasIndex(l => l.Status)
            .HasDatabaseName("IX_Licenses_Status");

        builder.HasIndex(l => l.ExpiryDate)
            .HasDatabaseName("IX_Licenses_ExpiryDate");

        builder.HasIndex(l => l.HolderId)
            .HasDatabaseName("IX_Licenses_Holder");

        builder.HasIndex(l => l.Type)
            .HasDatabaseName("IX_Licenses_Type");

        builder.HasIndex(l => l.IssuingAuthority)
            .HasDatabaseName("IX_Licenses_IssuingAuthority");

        // Computed properties are ignored
        builder.Ignore(l => l.IsExpired);
        builder.Ignore(l => l.IsExpiringSoon);
        builder.Ignore(l => l.DaysUntilExpiry);
        builder.Ignore(l => l.RequiresRenewal);
        builder.Ignore(l => l.ComplianceScore);
    }
}