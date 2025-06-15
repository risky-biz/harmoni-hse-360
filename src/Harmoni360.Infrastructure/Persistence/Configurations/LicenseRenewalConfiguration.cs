using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class LicenseRenewalConfiguration : IEntityTypeConfiguration<LicenseRenewal>
{
    public void Configure(EntityTypeBuilder<LicenseRenewal> builder)
    {
        builder.ToTable("LicenseRenewals");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.LicenseId)
            .IsRequired();

        builder.Property(r => r.RenewalNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.ApplicationDate)
            .IsRequired();

        builder.Property(r => r.SubmittedDate);

        builder.Property(r => r.ApprovedDate);

        builder.Property(r => r.RejectedDate);

        builder.Property(r => r.NewExpiryDate)
            .IsRequired();

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.RenewalNotes)
            .HasMaxLength(2000);

        builder.Property(r => r.RenewalFee)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.DocumentsRequired)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.InspectionRequired)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.InspectionDate);

        builder.Property(r => r.ProcessedBy)
            .HasMaxLength(100);

        // Audit fields
        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.LastModifiedAt);

        builder.Property(r => r.LastModifiedBy)
            .HasMaxLength(100);

        // Relationship
        builder.HasOne(r => r.License)
            .WithMany(l => l.Renewals)
            .HasForeignKey(r => r.LicenseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(r => r.LicenseId)
            .HasDatabaseName("IX_LicenseRenewals_LicenseId");

        builder.HasIndex(r => r.RenewalNumber)
            .IsUnique()
            .HasDatabaseName("IX_LicenseRenewals_RenewalNumber");

        builder.HasIndex(r => r.Status)
            .HasDatabaseName("IX_LicenseRenewals_Status");

        builder.HasIndex(r => r.ApplicationDate)
            .HasDatabaseName("IX_LicenseRenewals_ApplicationDate");

        builder.HasIndex(r => r.NewExpiryDate)
            .HasDatabaseName("IX_LicenseRenewals_NewExpiryDate");
    }
}