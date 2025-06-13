using Harmoni360.Domain.Entities.Inspections;
using Harmoni360.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class InspectionConfiguration : IEntityTypeConfiguration<Inspection>
{
    public void Configure(EntityTypeBuilder<Inspection> builder)
    {
        builder.ToTable("Inspections");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .IsRequired();

        builder.Property(e => e.InspectionNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(e => e.InspectionNumber)
            .IsUnique();

        builder.Property(e => e.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.Category)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.Priority)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.RiskLevel)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.ScheduledDate)
            .IsRequired();

        builder.Property(e => e.StartedDate);

        builder.Property(e => e.CompletedDate);

        builder.Property(e => e.InspectorId)
            .IsRequired();

        builder.Property(e => e.LocationId);

        builder.Property(e => e.DepartmentId);

        builder.Property(e => e.FacilityId);

        builder.Property(e => e.Summary)
            .HasMaxLength(2000);

        builder.Property(e => e.Recommendations)
            .HasMaxLength(2000);

        builder.Property(e => e.EstimatedDurationMinutes);

        builder.Property(e => e.ActualDurationMinutes);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.LastModifiedAt);

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(450);

        builder.Property(e => e.LastModifiedBy)
            .HasMaxLength(450);

        // Relationships
        builder.HasOne(e => e.Inspector)
            .WithMany()
            .HasForeignKey(e => e.InspectorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Department)
            .WithMany()
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Items)
            .WithOne(i => i.Inspection)
            .HasForeignKey(i => i.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Attachments)
            .WithOne(a => a.Inspection)
            .HasForeignKey(a => a.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Findings)
            .WithOne(f => f.Inspection)
            .HasForeignKey(f => f.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Comments)
            .WithOne(c => c.Inspection)
            .HasForeignKey(c => c.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.Priority);
        builder.HasIndex(e => e.ScheduledDate);
        builder.HasIndex(e => e.InspectorId);
        builder.HasIndex(e => e.DepartmentId);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => new { e.Status, e.ScheduledDate });
        builder.HasIndex(e => new { e.InspectorId, e.Status });

        // Ignore computed properties
        builder.Ignore(e => e.IsOverdue);
        builder.Ignore(e => e.CanEdit);
        builder.Ignore(e => e.CanStart);
        builder.Ignore(e => e.CanComplete);
        builder.Ignore(e => e.CanCancel);
    }
}