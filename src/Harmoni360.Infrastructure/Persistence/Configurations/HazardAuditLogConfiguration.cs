using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Infrastructure.Persistence.Configurations
{
    public class HazardAuditLogConfiguration : IEntityTypeConfiguration<HazardAuditLog>
    {
        public void Configure(EntityTypeBuilder<HazardAuditLog> builder)
        {
            builder.ToTable("HazardAuditLogs");

            builder.Property(hal => hal.Action)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(hal => hal.FieldName)
                .HasMaxLength(100);

            builder.Property(hal => hal.OldValue)
                .HasMaxLength(500);

            builder.Property(hal => hal.NewValue)
                .HasMaxLength(500);

            builder.Property(hal => hal.ChangeDescription)
                .HasMaxLength(1000);

            builder.Property(hal => hal.ChangedBy)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(hal => hal.IpAddress)
                .HasMaxLength(45); // Support IPv6

            builder.Property(hal => hal.UserAgent)
                .HasMaxLength(500);

            builder.HasOne(hal => hal.Hazard)
                .WithMany()
                .HasForeignKey(hal => hal.HazardId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(hal => new { hal.HazardId, hal.ChangedAt })
                .HasDatabaseName("IX_HazardAuditLogs_HazardId_ChangedAt");
        }
    }
}