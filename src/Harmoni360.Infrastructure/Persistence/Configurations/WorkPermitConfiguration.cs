using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.ValueObjects;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class WorkPermitConfiguration : IEntityTypeConfiguration<WorkPermit>
{
    public void Configure(EntityTypeBuilder<WorkPermit> builder)
    {
        // Primary Key
        builder.HasKey(wp => wp.Id);

        // Basic Properties
        builder.Property(wp => wp.PermitNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(wp => wp.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(wp => wp.Description)
            .HasMaxLength(2000);

        builder.Property(wp => wp.WorkLocation)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(wp => wp.WorkScope)
            .HasMaxLength(2000);

        builder.Property(wp => wp.EquipmentToBeUsed)
            .HasMaxLength(1000);

        builder.Property(wp => wp.MaterialsInvolved)
            .HasMaxLength(1000);

        builder.Property(wp => wp.ContractorCompany)
            .HasMaxLength(200);

        // Enum Conversions
        builder.Property(wp => wp.Type)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(wp => wp.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(wp => wp.Priority)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(wp => wp.RiskLevel)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        // Personnel Information
        builder.Property(wp => wp.RequestedByName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(wp => wp.RequestedByDepartment)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(wp => wp.RequestedByPosition)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(wp => wp.ContactPhone)
            .HasMaxLength(20);

        builder.Property(wp => wp.WorkSupervisor)
            .HasMaxLength(100);

        builder.Property(wp => wp.SafetyOfficer)
            .HasMaxLength(100);

        // Indonesian Compliance Fields
        builder.Property(wp => wp.K3LicenseNumber)
            .HasMaxLength(100);

        builder.Property(wp => wp.CompanyWorkPermitNumber)
            .HasMaxLength(100);

        builder.Property(wp => wp.EnvironmentalPermitNumber)
            .HasMaxLength(100);

        // Risk Assessment
        builder.Property(wp => wp.RiskAssessmentSummary)
            .HasMaxLength(2000);

        builder.Property(wp => wp.EmergencyProcedures)
            .HasMaxLength(2000);

        // Completion
        builder.Property(wp => wp.CompletionNotes)
            .HasMaxLength(2000);

        builder.Property(wp => wp.LessonsLearned)
            .HasMaxLength(2000);

        // Value Object Configuration - GeoLocation
        builder.OwnsOne(wp => wp.GeoLocation, geo =>
        {
            geo.Property(g => g.Latitude)
                .HasColumnName("Latitude")
                .HasPrecision(10, 8);

            geo.Property(g => g.Longitude)
                .HasColumnName("Longitude")
                .HasPrecision(11, 8);
        });

        // Relationships
        builder.HasMany(wp => wp.Attachments)
            .WithOne(a => a.WorkPermit)
            .HasForeignKey(a => a.WorkPermitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(wp => wp.Approvals)
            .WithOne(a => a.WorkPermit)
            .HasForeignKey(a => a.WorkPermitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(wp => wp.Hazards)
            .WithOne(h => h.WorkPermit)
            .HasForeignKey(h => h.WorkPermitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(wp => wp.Precautions)
            .WithOne(p => p.WorkPermit)
            .HasForeignKey(p => p.WorkPermitId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for Performance
        builder.HasIndex(wp => wp.PermitNumber)
            .IsUnique();

        builder.HasIndex(wp => wp.Type);

        builder.HasIndex(wp => wp.Status);

        builder.HasIndex(wp => wp.Priority);

        builder.HasIndex(wp => wp.RiskLevel);

        builder.HasIndex(wp => wp.RequestedById);

        builder.HasIndex(wp => wp.PlannedStartDate);

        builder.HasIndex(wp => wp.PlannedEndDate);

        builder.HasIndex(wp => wp.CreatedAt);

        // Composite indexes for common queries
        builder.HasIndex(wp => new { wp.Status, wp.Type });
        builder.HasIndex(wp => new { wp.RequestedById, wp.Status });
        builder.HasIndex(wp => new { wp.PlannedStartDate, wp.Status });

        // Table Configuration
        builder.ToTable("WorkPermits");

        // Audit fields configuration
        builder.Property(wp => wp.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(wp => wp.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(wp => wp.LastModifiedAt);

        builder.Property(wp => wp.LastModifiedBy)
            .HasMaxLength(100);
    }
}