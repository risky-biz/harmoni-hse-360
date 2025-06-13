using Harmoni360.Domain.Entities.Inspections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class FindingAttachmentConfiguration : IEntityTypeConfiguration<FindingAttachment>
{
    public void Configure(EntityTypeBuilder<FindingAttachment> builder)
    {
        builder.ToTable("FindingAttachments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .IsRequired();

        builder.Property(e => e.FindingId)
            .IsRequired();

        builder.Property(e => e.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.OriginalFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.FileSize)
            .IsRequired();

        builder.Property(e => e.FilePath)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.IsPhoto)
            .IsRequired();

        builder.Property(e => e.ThumbnailPath)
            .HasMaxLength(500);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.LastModifiedAt);

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(450);

        builder.Property(e => e.LastModifiedBy)
            .HasMaxLength(450);

        // Relationships
        builder.HasOne(e => e.Finding)
            .WithMany(f => f.Attachments)
            .HasForeignKey(e => e.FindingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.FindingId);
        builder.HasIndex(e => e.IsPhoto);
        builder.HasIndex(e => e.CreatedAt);

        // Ignore computed properties
        builder.Ignore(e => e.IsDocument);
        builder.Ignore(e => e.FileExtension);
    }
}