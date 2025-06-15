using Harmoni360.Domain.Entities.Inspections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class InspectionAttachmentConfiguration : IEntityTypeConfiguration<InspectionAttachment>
{
    public void Configure(EntityTypeBuilder<InspectionAttachment> builder)
    {
        builder.ToTable("InspectionAttachments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .IsRequired();

        builder.Property(e => e.InspectionId)
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

        builder.Property(e => e.Category)
            .HasMaxLength(50);

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
        builder.HasOne(e => e.Inspection)
            .WithMany(i => i.Attachments)
            .HasForeignKey(e => e.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.InspectionId);
        builder.HasIndex(e => e.IsPhoto);
        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => e.CreatedAt);

        // Ignore computed properties
        builder.Ignore(e => e.IsDocument);
        builder.Ignore(e => e.FileExtension);
    }
}