using Harmoni360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class VaccinationRecordConfiguration : IEntityTypeConfiguration<VaccinationRecord>
{
    public void Configure(EntityTypeBuilder<VaccinationRecord> builder)
    {
        builder.HasKey(vr => vr.Id);

        builder.Property(vr => vr.HealthRecordId)
            .IsRequired();

        builder.Property(vr => vr.VaccineName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(vr => vr.DateAdministered)
            .HasColumnType("timestamp with time zone");

        builder.Property(vr => vr.ExpiryDate)
            .HasColumnType("timestamp with time zone");

        builder.Property(vr => vr.BatchNumber)
            .HasMaxLength(100);

        builder.Property(vr => vr.AdministeredBy)
            .HasMaxLength(200);

        builder.Property(vr => vr.AdministrationLocation)
            .HasMaxLength(200);

        builder.Property(vr => vr.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(VaccinationStatus.Scheduled)
            .HasSentinel(VaccinationStatus.Scheduled);

        builder.Property(vr => vr.Notes)
            .HasMaxLength(1000);

        builder.Property(vr => vr.IsRequired)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(vr => vr.ExemptionReason)
            .HasMaxLength(500);

        // Audit fields
        builder.Property(vr => vr.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(vr => vr.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(vr => vr.LastModifiedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(vr => vr.LastModifiedBy)
            .HasMaxLength(100);

        // Configure relationships
        builder.HasOne(vr => vr.HealthRecord)
            .WithMany(hr => hr.Vaccinations)
            .HasForeignKey(vr => vr.HealthRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(vr => vr.DomainEvents);

        // Indexes for performance
        builder.HasIndex(vr => vr.HealthRecordId);
        builder.HasIndex(vr => vr.VaccineName);
        builder.HasIndex(vr => vr.Status);
        builder.HasIndex(vr => vr.ExpiryDate);
        builder.HasIndex(vr => vr.DateAdministered);
        builder.HasIndex(vr => vr.IsRequired);
        builder.HasIndex(vr => vr.CreatedAt);

        // Composite index for vaccination compliance queries
        builder.HasIndex(vr => new { vr.HealthRecordId, vr.VaccineName, vr.Status });

        // Table name
        builder.ToTable("VaccinationRecords");
    }
}