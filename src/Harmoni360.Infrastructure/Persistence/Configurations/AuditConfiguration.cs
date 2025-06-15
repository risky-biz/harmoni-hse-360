using Harmoni360.Domain.Entities.Audits;
using Harmoni360.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class AuditConfiguration : IEntityTypeConfiguration<Audit>
{
    public void Configure(EntityTypeBuilder<Audit> builder)
    {
        builder.ToTable("Audits");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .IsRequired();

        builder.Property(e => e.AuditNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(e => e.AuditNumber)
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

        builder.Property(e => e.AuditorId)
            .IsRequired();

        builder.Property(e => e.LocationId);

        builder.Property(e => e.DepartmentId);

        builder.Property(e => e.FacilityId);

        builder.Property(e => e.Summary)
            .HasMaxLength(2000);

        builder.Property(e => e.Recommendations)
            .HasMaxLength(2000);

        builder.Property(e => e.OverallScore)
            .HasConversion<int>();

        builder.Property(e => e.EstimatedDurationMinutes);

        builder.Property(e => e.ActualDurationMinutes);

        builder.Property(e => e.StandardsApplied)
            .HasMaxLength(1000);

        builder.Property(e => e.IsRegulatory)
            .IsRequired();

        builder.Property(e => e.RegulatoryReference)
            .HasMaxLength(500);

        builder.Property(e => e.ScorePercentage)
            .HasPrecision(5, 2);

        builder.Property(e => e.TotalPossiblePoints);

        builder.Property(e => e.AchievedPoints);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.LastModifiedAt);

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(450);

        builder.Property(e => e.LastModifiedBy)
            .HasMaxLength(450);

        // Relationships
        builder.HasOne(e => e.Auditor)
            .WithMany()
            .HasForeignKey(e => e.AuditorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Department)
            .WithMany()
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Items)
            .WithOne(i => i.Audit)
            .HasForeignKey(i => i.AuditId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Attachments)
            .WithOne(a => a.Audit)
            .HasForeignKey(a => a.AuditId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Findings)
            .WithOne(f => f.Audit)
            .HasForeignKey(f => f.AuditId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Comments)
            .WithOne(c => c.Audit)
            .HasForeignKey(c => c.AuditId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => e.Priority);
        builder.HasIndex(e => e.RiskLevel);
        builder.HasIndex(e => e.ScheduledDate);
        builder.HasIndex(e => e.AuditorId);
        builder.HasIndex(e => e.DepartmentId);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => new { e.Status, e.ScheduledDate });
        builder.HasIndex(e => new { e.AuditorId, e.Status });
        builder.HasIndex(e => new { e.Type, e.Status });

        // Ignore computed properties
        builder.Ignore(e => e.IsOverdue);
        builder.Ignore(e => e.CanEdit);
        builder.Ignore(e => e.CanStart);
        builder.Ignore(e => e.CanComplete);
        builder.Ignore(e => e.CanCancel);
        builder.Ignore(e => e.CanArchive);
        builder.Ignore(e => e.HasFindings);
        builder.Ignore(e => e.HasCriticalFindings);
        builder.Ignore(e => e.CompletionPercentage);
    }
}