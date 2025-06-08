using Harmoni360.Domain.Entities;
using Harmoni360.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class HazardConfiguration : IEntityTypeConfiguration<Hazard>
{
    public void Configure(EntityTypeBuilder<Hazard> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(h => h.Location)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.Category)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(h => h.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(h => h.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(h => h.Severity)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(h => h.ReporterDepartment)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(h => h.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(h => h.LastModifiedBy)
            .HasMaxLength(256);

        // Configure GeoLocation value object
        builder.OwnsOne(h => h.GeoLocation, geo =>
        {
            geo.Property(g => g.Latitude)
                .HasColumnName("Latitude")
                .HasPrecision(18, 6);

            geo.Property(g => g.Longitude)
                .HasColumnName("Longitude")
                .HasPrecision(18, 6);
        });

        // Configure relationships
        builder.HasOne(h => h.Reporter)
            .WithMany()
            .HasForeignKey(h => h.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.CurrentRiskAssessment)
            .WithOne()
            .HasForeignKey<Hazard>(h => h.CurrentRiskAssessmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(h => h.Attachments)
            .WithOne(a => a.Hazard)
            .HasForeignKey(a => a.HazardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(h => h.RiskAssessments)
            .WithOne(ra => ra.Hazard)
            .HasForeignKey(ra => ra.HazardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(h => h.MitigationActions)
            .WithOne(ma => ma.Hazard)
            .HasForeignKey(ma => ma.HazardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(h => h.Reassessments)
            .WithOne(r => r.Hazard)
            .HasForeignKey(r => r.HazardId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(h => h.DomainEvents);

        // Indexes for performance
        builder.HasIndex(h => h.Category);
        builder.HasIndex(h => h.Type);
        builder.HasIndex(h => h.Status);
        builder.HasIndex(h => h.Severity);
        builder.HasIndex(h => h.IdentifiedDate);
        builder.HasIndex(h => h.ReporterId);
        builder.HasIndex(h => h.ReporterDepartment);
        builder.HasIndex(h => h.CreatedAt);
        builder.HasIndex(h => new { h.Status, h.Severity }); // Composite index for common queries
        builder.HasIndex(h => new { h.Category, h.Status }); // Composite index for filtering
    }
}