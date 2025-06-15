using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class LicenseAuditLogConfiguration : IEntityTypeConfiguration<LicenseAuditLog>
{
    public void Configure(EntityTypeBuilder<LicenseAuditLog> builder)
    {
        builder.ToTable("LicenseAuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.LicenseId)
            .IsRequired();

        builder.Property(a => a.Action)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.ActionDescription)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.OldValues)
            .HasColumnType("text");

        builder.Property(a => a.NewValues)
            .HasColumnType("text");

        builder.Property(a => a.PerformedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.PerformedAt)
            .IsRequired();

        builder.Property(a => a.IpAddress)
            .HasMaxLength(45); // IPv6 support

        builder.Property(a => a.UserAgent)
            .HasMaxLength(500);

        builder.Property(a => a.Comments)
            .HasMaxLength(2000);

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
            .WithMany(l => l.AuditLogs)
            .HasForeignKey(a => a.LicenseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(a => a.LicenseId)
            .HasDatabaseName("IX_LicenseAuditLogs_LicenseId");

        builder.HasIndex(a => a.Action)
            .HasDatabaseName("IX_LicenseAuditLogs_Action");

        builder.HasIndex(a => a.PerformedAt)
            .HasDatabaseName("IX_LicenseAuditLogs_PerformedAt");

        builder.HasIndex(a => a.PerformedBy)
            .HasDatabaseName("IX_LicenseAuditLogs_PerformedBy");
    }
}