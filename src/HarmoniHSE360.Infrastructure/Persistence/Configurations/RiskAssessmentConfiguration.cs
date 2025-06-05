using HarmoniHSE360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarmoniHSE360.Infrastructure.Persistence.Configurations;

public class RiskAssessmentConfiguration : IEntityTypeConfiguration<RiskAssessment>
{
    public void Configure(EntityTypeBuilder<RiskAssessment> builder)
    {
        builder.HasKey(ra => ra.Id);

        builder.Property(ra => ra.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(ra => ra.RiskLevel)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(ra => ra.ProbabilityScore)
            .IsRequired()
            .HasComment("Risk probability score (1-5)");

        builder.Property(ra => ra.SeverityScore)
            .IsRequired()
            .HasComment("Risk severity score (1-5)");

        builder.Property(ra => ra.RiskScore)
            .IsRequired()
            .HasComment("Calculated risk score (Probability × Severity)");

        builder.Property(ra => ra.PotentialConsequences)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(ra => ra.ExistingControls)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(ra => ra.RecommendedActions)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(ra => ra.AdditionalNotes)
            .HasMaxLength(2000);

        builder.Property(ra => ra.ApprovalNotes)
            .HasMaxLength(500);

        builder.Property(ra => ra.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(ra => ra.LastModifiedBy)
            .HasMaxLength(256);

        // Configure relationships
        builder.HasOne(ra => ra.Hazard)
            .WithMany(h => h.RiskAssessments)
            .HasForeignKey(ra => ra.HazardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ra => ra.Assessor)
            .WithMany()
            .HasForeignKey(ra => ra.AssessorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ra => ra.ApprovedBy)
            .WithMany()
            .HasForeignKey(ra => ra.ApprovedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore domain events
        builder.Ignore(ra => ra.DomainEvents);

        // Indexes for performance
        builder.HasIndex(ra => ra.HazardId);
        builder.HasIndex(ra => ra.Type);
        builder.HasIndex(ra => ra.RiskLevel);
        builder.HasIndex(ra => ra.RiskScore);
        builder.HasIndex(ra => ra.AssessmentDate);
        builder.HasIndex(ra => ra.NextReviewDate);
        builder.HasIndex(ra => ra.IsActive);
        builder.HasIndex(ra => ra.IsApproved);
        builder.HasIndex(ra => new { ra.RiskLevel, ra.IsActive }); // Composite index for active high-risk assessments
        builder.HasIndex(ra => new { ra.HazardId, ra.IsActive }); // Composite index for hazard's active assessments
    }
}