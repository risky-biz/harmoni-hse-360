using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class WorkPermitSettingsConfiguration : IEntityTypeConfiguration<WorkPermitSettings>
{
    public void Configure(EntityTypeBuilder<WorkPermitSettings> builder)
    {
        // Primary Key
        builder.HasKey(wps => wps.Id);

        // Properties
        builder.Property(wps => wps.RequireSafetyInduction)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(wps => wps.EnableFormValidation)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(wps => wps.AllowAttachments)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(wps => wps.MaxAttachmentSizeMB)
            .IsRequired()
            .HasDefaultValue(10);

        builder.Property(wps => wps.FormInstructions)
            .HasMaxLength(2000);

        builder.Property(wps => wps.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit Properties
        builder.Property(wps => wps.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(wps => wps.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(wps => wps.LastModifiedAt);

        builder.Property(wps => wps.LastModifiedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasMany(wps => wps.SafetyVideos)
            .WithOne()
            .HasForeignKey("WorkPermitSettingsId")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(wps => wps.IsActive)
            .HasFilter("\"IsActive\" = true")
            .IsUnique(); // Only one active settings at a time

        builder.HasIndex(wps => wps.CreatedAt);

        // Table Configuration
        builder.ToTable("WorkPermitSettings");

        // Ignore computed properties (they are calculated from other properties)
        builder.Ignore(wps => wps.ActiveSafetyVideo);
        builder.Ignore(wps => wps.IsSafetyInductionConfigured);
        builder.Ignore(wps => wps.IsConfigurationComplete);
    }
}