using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class WorkPermitAttachmentConfiguration : IEntityTypeConfiguration<WorkPermitAttachment>
{
    public void Configure(EntityTypeBuilder<WorkPermitAttachment> builder)
    {
        // Primary Key
        builder.HasKey(wpa => wpa.Id);

        // Properties
        builder.Property(wpa => wpa.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(wpa => wpa.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(wpa => wpa.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(wpa => wpa.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(wpa => wpa.UploadedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(wpa => wpa.Description)
            .HasMaxLength(500);

        // Enum Conversion
        builder.Property(wpa => wpa.AttachmentType)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        // Relationships
        builder.HasOne(wpa => wpa.WorkPermit)
            .WithMany(wp => wp.Attachments)
            .HasForeignKey(wpa => wpa.WorkPermitId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(wpa => wpa.WorkPermitId);
        builder.HasIndex(wpa => wpa.AttachmentType);
        builder.HasIndex(wpa => wpa.UploadedAt);

        // Table Configuration
        builder.ToTable("WorkPermitAttachments");

        // Audit fields
        builder.Property(wpa => wpa.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(wpa => wpa.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(wpa => wpa.LastModifiedAt);

        builder.Property(wpa => wpa.LastModifiedBy)
            .HasMaxLength(100);
    }
}