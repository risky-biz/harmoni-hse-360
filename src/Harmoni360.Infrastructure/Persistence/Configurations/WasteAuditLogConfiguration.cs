using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Infrastructure.Persistence.Configurations
{
    public class WasteAuditLogConfiguration : IEntityTypeConfiguration<WasteAuditLog>
    {
        public void Configure(EntityTypeBuilder<WasteAuditLog> builder)
        {
            builder.ToTable("WasteAuditLogs");

            builder.Property(wal => wal.Action)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(wal => wal.FieldName)
                .HasMaxLength(100);

            builder.Property(wal => wal.OldValue)
                .HasMaxLength(500);

            builder.Property(wal => wal.NewValue)
                .HasMaxLength(500);

            builder.Property(wal => wal.ChangeDescription)
                .HasMaxLength(1000);

            builder.Property(wal => wal.ChangedBy)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(wal => wal.IpAddress)
                .HasMaxLength(45); // Support IPv6

            builder.Property(wal => wal.UserAgent)
                .HasMaxLength(500);

            builder.Property(wal => wal.ComplianceNotes)
                .HasMaxLength(2000);

            builder.HasOne(wal => wal.WasteReport)
                .WithMany()
                .HasForeignKey(wal => wal.WasteReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(wal => new { wal.WasteReportId, wal.ChangedAt })
                .HasDatabaseName("IX_WasteAuditLogs_WasteReportId_ChangedAt");

            builder.HasIndex(wal => wal.IsCriticalAction)
                .HasDatabaseName("IX_WasteAuditLogs_IsCriticalAction");

            builder.HasIndex(wal => wal.ChangedAt)
                .HasDatabaseName("IX_WasteAuditLogs_ChangedAt");
        }
    }
}