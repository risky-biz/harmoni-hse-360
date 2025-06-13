using Harmoni360.Domain.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class SecurityIncidentAttachmentConfiguration : IEntityTypeConfiguration<SecurityIncidentAttachment>
{
    public void Configure(EntityTypeBuilder<SecurityIncidentAttachment> builder)
    {
        builder.HasKey(sia => sia.Id);

        // Properties
        builder.Property(sia => sia.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(sia => sia.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(sia => sia.FileType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(sia => sia.Description)
            .HasColumnType("text");

        builder.Property(sia => sia.Hash)
            .HasMaxLength(64); // SHA-256 hash length

        builder.Property(sia => sia.UploadedBy)
            .IsRequired()
            .HasMaxLength(100);

        // Enum Properties
        builder.Property(sia => sia.AttachmentType)
            .IsRequired()
            .HasConversion<int>();

        // Note: Computed properties FileSizeFormatted, IsImage, and IsDocument 
        // are implemented as calculated properties in the domain entity, not as computed columns

        // Navigation Properties
        builder.HasOne(sia => sia.SecurityIncident)
            .WithMany(si => si.Attachments)
            .HasForeignKey(sia => sia.SecurityIncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(sia => sia.SecurityIncidentId)
            .HasDatabaseName("IX_SecurityIncidentAttachments_SecurityIncidentId");

        builder.HasIndex(sia => sia.AttachmentType)
            .HasDatabaseName("IX_SecurityIncidentAttachments_AttachmentType");

        builder.HasIndex(sia => sia.UploadedAt)
            .HasDatabaseName("IX_SecurityIncidentAttachments_UploadedAt");

        builder.HasIndex(sia => sia.IsConfidential)
            .HasDatabaseName("IX_SecurityIncidentAttachments_IsConfidential");

        builder.HasIndex(sia => new { sia.SecurityIncidentId, sia.AttachmentType })
            .HasDatabaseName("IX_SecurityIncidentAttachments_Incident_Type");

        // Table Name
        builder.ToTable("SecurityIncidentAttachments");
    }
}