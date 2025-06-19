using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class WorkPermitSafetyVideoConfiguration : IEntityTypeConfiguration<WorkPermitSafetyVideo>
{
    public void Configure(EntityTypeBuilder<WorkPermitSafetyVideo> builder)
    {
        // Primary Key
        builder.HasKey(wsv => wsv.Id);

        // File Properties
        builder.Property(wsv => wsv.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(wsv => wsv.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(wsv => wsv.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(wsv => wsv.FileSize)
            .IsRequired();

        builder.Property(wsv => wsv.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        // Video Properties
        builder.Property(wsv => wsv.Duration)
            .IsRequired();

        builder.Property(wsv => wsv.Description)
            .HasMaxLength(1000);

        builder.Property(wsv => wsv.ThumbnailPath)
            .HasMaxLength(500);

        builder.Property(wsv => wsv.Resolution)
            .HasMaxLength(20); // e.g., "1920x1080"

        builder.Property(wsv => wsv.Bitrate);

        // Status
        builder.Property(wsv => wsv.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit Properties
        builder.Property(wsv => wsv.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(wsv => wsv.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(wsv => wsv.LastModifiedAt);

        builder.Property(wsv => wsv.LastModifiedBy)
            .HasMaxLength(100);

        // Indexes for Performance
        builder.HasIndex(wsv => wsv.IsActive);
        builder.HasIndex(wsv => wsv.ContentType);
        builder.HasIndex(wsv => wsv.CreatedAt);
        builder.HasIndex(wsv => wsv.FileName);

        // Composite indexes for common queries
        builder.HasIndex(wsv => new { wsv.IsActive, wsv.CreatedAt });

        // Check constraints
        builder.ToTable("WorkPermitSafetyVideos", t =>
        {
            t.HasCheckConstraint("CK_WorkPermitSafetyVideos_FileSize_Positive", "\"FileSize\" > 0");
            t.HasCheckConstraint("CK_WorkPermitSafetyVideos_Duration_Positive", "\"Duration\" > '00:00:00'");
        });

        // Ignore computed properties
        builder.Ignore(wsv => wsv.FileSizeMB);
        builder.Ignore(wsv => wsv.DurationFormatted);
        builder.Ignore(wsv => wsv.IsSupportedFormat);
    }
}