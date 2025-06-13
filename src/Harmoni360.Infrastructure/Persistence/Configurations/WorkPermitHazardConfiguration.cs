using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class WorkPermitHazardConfiguration : IEntityTypeConfiguration<WorkPermitHazard>
{
    public void Configure(EntityTypeBuilder<WorkPermitHazard> builder)
    {
        // Primary Key
        builder.HasKey(wph => wph.Id);

        // Properties
        builder.Property(wph => wph.HazardDescription)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(wph => wph.ControlMeasures)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(wph => wph.ResponsiblePerson)
            .HasMaxLength(100);

        builder.Property(wph => wph.ImplementationNotes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(wph => wph.Category)
            .WithMany()
            .HasForeignKey(wph => wph.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(wph => wph.RiskLevel)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(wph => wph.ResidualRiskLevel)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        // Risk Assessment Values
        builder.Property(wph => wph.Likelihood)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(wph => wph.Severity)
            .IsRequired()
            .HasDefaultValue(1);

        // Relationships
        builder.HasOne(wph => wph.WorkPermit)
            .WithMany(wp => wp.Hazards)
            .HasForeignKey(wph => wph.WorkPermitId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(wph => wph.WorkPermitId);
        builder.HasIndex(wph => wph.CategoryId);
        builder.HasIndex(wph => wph.RiskLevel);
        builder.HasIndex(wph => wph.IsControlImplemented);

        // Composite indexes
        builder.HasIndex(wph => new { wph.WorkPermitId, wph.RiskLevel });
        builder.HasIndex(wph => new { wph.WorkPermitId, wph.IsControlImplemented });

        // Table Configuration
        builder.ToTable("WorkPermitHazards");

        // Audit fields
        builder.Property(wph => wph.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(wph => wph.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(wph => wph.LastModifiedAt);

        builder.Property(wph => wph.LastModifiedBy)
            .HasMaxLength(100);
    }
}