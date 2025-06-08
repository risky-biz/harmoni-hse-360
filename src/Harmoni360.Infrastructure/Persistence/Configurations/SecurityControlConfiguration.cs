using Harmoni360.Domain.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class SecurityControlConfiguration : IEntityTypeConfiguration<SecurityControl>
{
    public void Configure(EntityTypeBuilder<SecurityControl> builder)
    {
        builder.HasKey(sc => sc.Id);

        // Properties
        builder.Property(sc => sc.ControlName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sc => sc.ControlDescription)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(sc => sc.EffectivenessNotes)
            .HasColumnType("text");

        // Enum Properties
        builder.Property(sc => sc.ControlType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(sc => sc.Category)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(sc => sc.Status)
            .IsRequired()
            .HasConversion<int>();

        // Numeric Properties with Constraints
        builder.Property(sc => sc.EffectivenessScore)
            .HasAnnotation("CheckConstraint", "EffectivenessScore BETWEEN 1 AND 10");

        // Decimal Properties
        builder.Property(sc => sc.ImplementationCost)
            .HasColumnType("decimal(15,2)");

        builder.Property(sc => sc.AnnualMaintenanceCost)
            .HasColumnType("decimal(15,2)");

        // Note: Computed properties IsOverdue, DaysUntilReview, and IsEffective 
        // are implemented as calculated properties in the domain entity, not as computed columns

        // Navigation Properties
        builder.HasOne(sc => sc.RelatedIncident)
            .WithMany()
            .HasForeignKey(sc => sc.RelatedIncidentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(sc => sc.ImplementedBy)
            .WithMany()
            .HasForeignKey(sc => sc.ImplementedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sc => sc.ReviewedBy)
            .WithMany()
            .HasForeignKey(sc => sc.ReviewedById)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(sc => sc.ControlType)
            .HasDatabaseName("IX_SecurityControls_ControlType");

        builder.HasIndex(sc => sc.Category)
            .HasDatabaseName("IX_SecurityControls_Category");

        builder.HasIndex(sc => sc.Status)
            .HasDatabaseName("IX_SecurityControls_Status");

        builder.HasIndex(sc => sc.NextReviewDate)
            .HasDatabaseName("IX_SecurityControls_NextReviewDate");

        builder.HasIndex(sc => sc.RelatedIncidentId)
            .HasDatabaseName("IX_SecurityControls_RelatedIncidentId");

        builder.HasIndex(sc => sc.ImplementedById)
            .HasDatabaseName("IX_SecurityControls_ImplementedById");

        builder.HasIndex(sc => new { sc.Status, sc.NextReviewDate })
            .HasDatabaseName("IX_SecurityControls_Status_NextReviewDate");

        builder.HasIndex(sc => new { sc.ControlType, sc.Category })
            .HasDatabaseName("IX_SecurityControls_Type_Category");

        // Table Name
        builder.ToTable("SecurityControls");
    }
}