using Harmoni360.Domain.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class ThreatIndicatorConfiguration : IEntityTypeConfiguration<ThreatIndicator>
{
    public void Configure(EntityTypeBuilder<ThreatIndicator> builder)
    {
        builder.HasKey(ti => ti.Id);

        // Properties
        builder.Property(ti => ti.IndicatorType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ti => ti.IndicatorValue)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ti => ti.ThreatType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ti => ti.Source)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ti => ti.Description)
            .HasColumnType("text");

        builder.Property(ti => ti.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        // Constraints
        builder.Property(ti => ti.Confidence)
            .IsRequired()
            .HasAnnotation("CheckConstraint", "Confidence BETWEEN 1 AND 100");

        // Array property for Tags - stored as JSON for broader database compatibility
        builder.Property(ti => ti.Tags)
            .HasConversion(
                tags => tags != null ? string.Join(",", tags) : null,
                value => value != null ? value.Split(',', StringSplitOptions.RemoveEmptyEntries) : null);

        // Note: Computed properties (IsHighConfidence, IsMediumConfidence, IsLowConfidence, 
        // IsRecentlyUpdated, IsStale, ConfidenceLevel) are implemented as calculated 
        // properties in the domain entity, not as computed columns

        // Unique Constraint
        builder.HasIndex(ti => new { ti.IndicatorType, ti.IndicatorValue })
            .IsUnique()
            .HasDatabaseName("IX_ThreatIndicators_Type_Value");

        // Other Indexes
        builder.HasIndex(ti => ti.IsActive)
            .HasDatabaseName("IX_ThreatIndicators_IsActive");

        builder.HasIndex(ti => ti.ThreatType)
            .HasDatabaseName("IX_ThreatIndicators_ThreatType");

        builder.HasIndex(ti => ti.Confidence)
            .HasDatabaseName("IX_ThreatIndicators_Confidence");

        builder.HasIndex(ti => ti.FirstSeen)
            .HasDatabaseName("IX_ThreatIndicators_FirstSeen");

        builder.HasIndex(ti => ti.LastSeen)
            .HasDatabaseName("IX_ThreatIndicators_LastSeen");

        builder.HasIndex(ti => ti.Source)
            .HasDatabaseName("IX_ThreatIndicators_Source");

        builder.HasIndex(ti => new { ti.IsActive, ti.Confidence })
            .HasDatabaseName("IX_ThreatIndicators_Active_Confidence");

        builder.HasIndex(ti => new { ti.ThreatType, ti.IsActive })
            .HasDatabaseName("IX_ThreatIndicators_ThreatType_Active");

        // Note: Tags index removed for database compatibility

        // Table Name
        builder.ToTable("ThreatIndicators");
    }
}