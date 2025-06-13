using Harmoni360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class HazardAttachmentConfiguration : IEntityTypeConfiguration<HazardAttachment>
{
    public void Configure(EntityTypeBuilder<HazardAttachment> builder)
    {
        builder.HasKey(ha => ha.Id);

        builder.Property(ha => ha.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(ha => ha.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ha => ha.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ha => ha.UploadedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(ha => ha.Description)
            .HasMaxLength(500);

        builder.Property(ha => ha.FileSize)
            .IsRequired()
            .HasComment("File size in bytes");

        // Configure relationships
        builder.HasOne(ha => ha.Hazard)
            .WithMany(h => h.Attachments)
            .HasForeignKey(ha => ha.HazardId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for performance
        builder.HasIndex(ha => ha.HazardId);
        builder.HasIndex(ha => ha.UploadedAt);
        builder.HasIndex(ha => ha.ContentType);
        builder.HasIndex(ha => ha.FileName);
    }
}