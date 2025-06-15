using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class LicenseAttachmentConfiguration : IEntityTypeConfiguration<LicenseAttachment>
{
    public void Configure(EntityTypeBuilder<LicenseAttachment> builder)
    {
        builder.ToTable("LicenseAttachments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.LicenseId)
            .IsRequired();

        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.FileSize)
            .IsRequired();

        builder.Property(a => a.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.UploadedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.UploadedAt)
            .IsRequired();

        builder.Property(a => a.AttachmentType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.Description)
            .HasMaxLength(500);

        builder.Property(a => a.IsRequired)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.ValidUntil);

        // Audit fields
        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.LastModifiedAt);

        builder.Property(a => a.LastModifiedBy)
            .HasMaxLength(100);

        // Relationship
        builder.HasOne(a => a.License)
            .WithMany(l => l.Attachments)
            .HasForeignKey(a => a.LicenseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(a => a.LicenseId)
            .HasDatabaseName("IX_LicenseAttachments_LicenseId");

        builder.HasIndex(a => a.AttachmentType)
            .HasDatabaseName("IX_LicenseAttachments_Type");

        builder.HasIndex(a => a.UploadedAt)
            .HasDatabaseName("IX_LicenseAttachments_UploadedAt");

        // Computed properties are ignored
        builder.Ignore(a => a.IsExpired);
    }
}