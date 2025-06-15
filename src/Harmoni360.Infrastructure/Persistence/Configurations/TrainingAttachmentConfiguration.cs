using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class TrainingAttachmentConfiguration : IEntityTypeConfiguration<TrainingAttachment>
{
    public void Configure(EntityTypeBuilder<TrainingAttachment> builder)
    {
        // Primary Key
        builder.HasKey(ta => ta.Id);

        // Basic Properties
        builder.Property(ta => ta.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(ta => ta.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(ta => ta.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ta => ta.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ta => ta.UploadedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ta => ta.Description)
            .HasMaxLength(1000);

        // Enum Conversions
        builder.Property(ta => ta.AttachmentType)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        // Version Control
        builder.Property(ta => ta.VersionNotes)
            .HasMaxLength(500);

        // Access and Security
        builder.Property(ta => ta.IsPublic)
            .HasDefaultValue(false);

        builder.Property(ta => ta.IsInstructorOnly)
            .HasDefaultValue(false);

        builder.Property(ta => ta.IsParticipantSubmission)
            .HasDefaultValue(false);

        builder.Property(ta => ta.IsCurrentVersion)
            .HasDefaultValue(true);

        // Indonesian Compliance Fields
        builder.Property(ta => ta.IsComplianceDocument)
            .HasDefaultValue(false);

        builder.Property(ta => ta.RegulatoryReference)
            .HasMaxLength(200);

        builder.Property(ta => ta.IsK3Document)
            .HasDefaultValue(false);

        builder.Property(ta => ta.K3DocumentType)
            .HasMaxLength(100);

        builder.Property(ta => ta.RequiresApproval)
            .HasDefaultValue(false);

        builder.Property(ta => ta.IsApproved)
            .HasDefaultValue(false);

        builder.Property(ta => ta.ApprovedBy)
            .HasMaxLength(100);

        // Document Properties
        builder.Property(ta => ta.Language)
            .HasMaxLength(10)
            .HasDefaultValue("id-ID");

        builder.Property(ta => ta.IsTranslationRequired)
            .HasDefaultValue(false);

        builder.Property(ta => ta.TranslatedFrom)
            .HasMaxLength(10);

        builder.Property(ta => ta.HasDigitalSignature)
            .HasDefaultValue(false);

        builder.Property(ta => ta.SignatureInfo)
            .HasMaxLength(500);

        // Usage Tracking
        builder.Property(ta => ta.LastAccessedBy)
            .HasMaxLength(100);

        builder.Property(ta => ta.IsArchived)
            .HasDefaultValue(false);

        // Validation and Verification
        builder.Property(ta => ta.ChecksumMD5)
            .HasMaxLength(32);

        builder.Property(ta => ta.ChecksumSHA256)
            .HasMaxLength(64);

        builder.Property(ta => ta.IsVirusScanned)
            .HasDefaultValue(false);

        builder.Property(ta => ta.IsVirusClean)
            .HasDefaultValue(true);

        // Relationships
        builder.HasOne(ta => ta.Training)
            .WithMany(t => t.Attachments)
            .HasForeignKey(ta => ta.TrainingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ta => ta.SubmittedByParticipant)
            .WithMany()
            .HasForeignKey(ta => ta.SubmittedByParticipantId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes for Performance
        builder.HasIndex(ta => ta.TrainingId);

        builder.HasIndex(ta => ta.AttachmentType);

        builder.HasIndex(ta => ta.IsPublic);

        builder.HasIndex(ta => ta.UploadedAt);

        builder.HasIndex(ta => ta.IsApproved);

        builder.HasIndex(ta => ta.IsCurrentVersion);

        builder.HasIndex(ta => ta.IsComplianceDocument);

        // Composite indexes for common queries
        builder.HasIndex(ta => new { ta.TrainingId, ta.AttachmentType });

        builder.HasIndex(ta => new { ta.TrainingId, ta.IsCurrentVersion });

        builder.HasIndex(ta => new { ta.IsPublic, ta.IsApproved });

        builder.HasIndex(ta => new { ta.TrainingId, ta.IsK3Document });

        // Table Configuration
        builder.ToTable("TrainingAttachments");

        // Audit fields configuration
        builder.Property(ta => ta.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(ta => ta.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ta => ta.LastModifiedAt);

        builder.Property(ta => ta.LastModifiedBy)
            .HasMaxLength(100);
    }
}