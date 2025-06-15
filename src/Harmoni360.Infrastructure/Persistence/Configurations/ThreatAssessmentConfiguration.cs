using Harmoni360.Domain.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class ThreatAssessmentConfiguration : IEntityTypeConfiguration<ThreatAssessment>
{
    public void Configure(EntityTypeBuilder<ThreatAssessment> builder)
    {
        builder.HasKey(ta => ta.Id);

        // Properties
        builder.Property(ta => ta.AssessmentRationale)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(ta => ta.ThreatIntelSource)
            .HasMaxLength(200);

        builder.Property(ta => ta.ThreatIntelDetails)
            .HasColumnType("text");

        // Enum Properties
        builder.Property(ta => ta.CurrentThreatLevel)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(ta => ta.PreviousThreatLevel)
            .IsRequired()
            .HasConversion<int>();

        // Risk Factor Constraints
        builder.Property(ta => ta.ThreatCapability)
            .IsRequired()
            .HasAnnotation("CheckConstraint", "ThreatCapability BETWEEN 1 AND 5");

        builder.Property(ta => ta.ThreatIntent)
            .IsRequired()
            .HasAnnotation("CheckConstraint", "ThreatIntent BETWEEN 1 AND 5");

        builder.Property(ta => ta.TargetVulnerability)
            .IsRequired()
            .HasAnnotation("CheckConstraint", "TargetVulnerability BETWEEN 1 AND 5");

        builder.Property(ta => ta.ImpactPotential)
            .IsRequired()
            .HasAnnotation("CheckConstraint", "ImpactPotential BETWEEN 1 AND 5");

        // Note: RiskScore is implemented as a calculated property in the domain entity

        // Navigation Properties
        builder.HasOne(ta => ta.SecurityIncident)
            .WithMany()
            .HasForeignKey(ta => ta.SecurityIncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ta => ta.AssessedBy)
            .WithMany()
            .HasForeignKey(ta => ta.AssessedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(ta => ta.SecurityIncidentId)
            .HasDatabaseName("IX_ThreatAssessments_SecurityIncidentId");

        builder.HasIndex(ta => ta.AssessmentDateTime)
            .HasDatabaseName("IX_ThreatAssessments_AssessmentDateTime");

        builder.HasIndex(ta => ta.CurrentThreatLevel)
            .HasDatabaseName("IX_ThreatAssessments_CurrentThreatLevel");

        builder.HasIndex(ta => new { ta.SecurityIncidentId, ta.AssessmentDateTime })
            .HasDatabaseName("IX_ThreatAssessments_Incident_DateTime");

        // Table Name
        builder.ToTable("ThreatAssessments");
    }
}