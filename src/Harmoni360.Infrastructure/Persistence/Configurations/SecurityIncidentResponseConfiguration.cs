using Harmoni360.Domain.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class SecurityIncidentResponseConfiguration : IEntityTypeConfiguration<SecurityIncidentResponse>
{
    public void Configure(EntityTypeBuilder<SecurityIncidentResponse> builder)
    {
        builder.HasKey(sir => sir.Id);

        // Properties
        builder.Property(sir => sir.ActionTaken)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(sir => sir.FollowUpDetails)
            .HasColumnType("text");

        builder.Property(sir => sir.ToolsUsed)
            .HasMaxLength(500);

        builder.Property(sir => sir.ResourcesUsed)
            .HasMaxLength(500);

        builder.Property(sir => sir.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        // Enum Properties
        builder.Property(sir => sir.ResponseType)
            .IsRequired()
            .HasConversion<int>();

        // Decimal Properties
        builder.Property(sir => sir.Cost)
            .HasColumnType("decimal(15,2)");

        // Constraints
        builder.Property(sir => sir.EffortHours)
            .HasAnnotation("CheckConstraint", "EffortHours >= 0");

        builder.Property(sir => sir.Cost)
            .HasAnnotation("CheckConstraint", "Cost >= 0");

        // Note: Computed properties IsOverdue, DaysUntilFollowUp, and ResponseTypeDescription 
        // are implemented as calculated properties in the domain entity, not as computed columns

        // Navigation Properties
        builder.HasOne(sir => sir.SecurityIncident)
            .WithMany(si => si.Responses)
            .HasForeignKey(sir => sir.SecurityIncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sir => sir.Responder)
            .WithMany()
            .HasForeignKey(sir => sir.ResponderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(sir => sir.SecurityIncidentId)
            .HasDatabaseName("IX_SecurityIncidentResponses_SecurityIncidentId");

        builder.HasIndex(sir => sir.ResponseType)
            .HasDatabaseName("IX_SecurityIncidentResponses_ResponseType");

        builder.HasIndex(sir => sir.ActionDateTime)
            .HasDatabaseName("IX_SecurityIncidentResponses_ActionDateTime");

        builder.HasIndex(sir => sir.ResponderId)
            .HasDatabaseName("IX_SecurityIncidentResponses_ResponderId");

        builder.HasIndex(sir => sir.FollowUpRequired)
            .HasDatabaseName("IX_SecurityIncidentResponses_FollowUpRequired");

        builder.HasIndex(sir => sir.FollowUpDueDate)
            .HasDatabaseName("IX_SecurityIncidentResponses_FollowUpDueDate");

        builder.HasIndex(sir => new { sir.SecurityIncidentId, sir.ResponseType })
            .HasDatabaseName("IX_SecurityIncidentResponses_Incident_Type");

        builder.HasIndex(sir => new { sir.SecurityIncidentId, sir.ActionDateTime })
            .HasDatabaseName("IX_SecurityIncidentResponses_Incident_DateTime");

        // Table Name
        builder.ToTable("SecurityIncidentResponses");
    }
}