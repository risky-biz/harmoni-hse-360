using Harmoni360.Domain.Entities.Audits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class AuditAttachmentConfiguration : IEntityTypeConfiguration<AuditAttachment>
{
    public void Configure(EntityTypeBuilder<AuditAttachment> builder)
    {
        builder.ToTable("AuditAttachments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
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

        builder.Property(e => e.FilePath)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.FileSize)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Category)
            .HasMaxLength(100);

        builder.Property(e => e.UploadedBy)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(e => e.UploadedAt)
            .IsRequired();

        builder.Property(e => e.AttachmentType)
            .IsRequired();

        builder.Property(e => e.IsEvidence)
            .IsRequired();

        builder.Property(e => e.AuditItemId);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.LastModifiedAt);

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(450);

        builder.Property(e => e.LastModifiedBy)
            .HasMaxLength(450);

        // Relationships
        builder.HasOne(e => e.Audit)
            .WithMany(a => a.Attachments)
            .HasForeignKey(e => e.AuditId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.AuditItem)
            .WithMany()
            .HasForeignKey(e => e.AuditItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.AuditId);
        builder.HasIndex(e => e.FileName);
        builder.HasIndex(e => e.ContentType);
        builder.HasIndex(e => e.AttachmentType);
        builder.HasIndex(e => e.UploadedAt);
        builder.HasIndex(e => e.IsEvidence);
        builder.HasIndex(e => e.AuditItemId);
        builder.HasIndex(e => new { e.AuditId, e.IsEvidence });
        builder.HasIndex(e => new { e.AuditId, e.Category });
    }
}