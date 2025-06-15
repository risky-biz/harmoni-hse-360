using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class TrainingRequirementConfiguration : IEntityTypeConfiguration<TrainingRequirement>
{
    public void Configure(EntityTypeBuilder<TrainingRequirement> builder)
    {
        // Primary Key
        builder.HasKey(tr => tr.Id);

        // Basic Properties
        builder.Property(tr => tr.RequirementDescription)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(tr => tr.CompletedBy)
            .HasMaxLength(100);

        builder.Property(tr => tr.CompletionNotes)
            .HasMaxLength(1000);

        // Enum Conversions
        builder.Property(tr => tr.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        // Assignment and Verification Details
        builder.Property(tr => tr.AssignedTo)
            .HasMaxLength(200);

        builder.Property(tr => tr.AssignedBy)
            .HasMaxLength(200);

        builder.Property(tr => tr.VerificationMethod)
            .HasMaxLength(500);

        builder.Property(tr => tr.VerifiedBy)
            .HasMaxLength(100);

        // Documentation and Evidence
        builder.Property(tr => tr.DocumentationRequired)
            .HasMaxLength(1000);

        builder.Property(tr => tr.EvidenceProvided)
            .HasMaxLength(1000);

        builder.Property(tr => tr.AttachmentPath)
            .HasMaxLength(500);

        // Compliance and Risk
        builder.Property(tr => tr.ComplianceNotes)
            .HasMaxLength(2000);

        builder.Property(tr => tr.ComplianceCost)
            .HasPrecision(10, 2);

        // Indonesian Compliance
        builder.Property(tr => tr.K3RegulationReference)
            .HasMaxLength(200);

        builder.Property(tr => tr.RegulatoryReference)
            .HasMaxLength(200);

        // Risk Level Enum
        builder.Property(tr => tr.RiskLevelIfNotCompleted)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Relationships
        builder.HasOne(tr => tr.Training)
            .WithMany(t => t.Requirements)
            .HasForeignKey(tr => tr.TrainingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for Performance
        builder.HasIndex(tr => tr.TrainingId);

        builder.HasIndex(tr => tr.Status);

        builder.HasIndex(tr => tr.IsMandatory);

        builder.HasIndex(tr => tr.DueDate);

        builder.HasIndex(tr => tr.IsVerified);

        // Composite indexes for common queries
        builder.HasIndex(tr => new { tr.TrainingId, tr.Status });

        builder.HasIndex(tr => new { tr.TrainingId, tr.IsMandatory });

        builder.HasIndex(tr => new { tr.Status, tr.DueDate });

        // Table Configuration
        builder.ToTable("TrainingRequirements");

        // Audit fields configuration
        builder.Property(tr => tr.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(tr => tr.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(tr => tr.LastModifiedAt);

        builder.Property(tr => tr.LastModifiedBy)
            .HasMaxLength(100);
    }
}