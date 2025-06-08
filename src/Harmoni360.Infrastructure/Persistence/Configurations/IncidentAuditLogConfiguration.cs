using Harmoni360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class IncidentAuditLogConfiguration : IEntityTypeConfiguration<IncidentAuditLog>
{
    public void Configure(EntityTypeBuilder<IncidentAuditLog> builder)
    {
        builder.ToTable("IncidentAuditLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.IncidentId)
            .IsRequired();

        builder.Property(x => x.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.FieldName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.OldValue)
            .HasMaxLength(1000);

        builder.Property(x => x.NewValue)
            .HasMaxLength(1000);

        builder.Property(x => x.ChangedBy)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.ChangedAt)
            .IsRequired();

        builder.Property(x => x.ChangeDescription)
            .HasMaxLength(500);

        builder.HasOne(x => x.Incident)
            .WithMany()
            .HasForeignKey(x => x.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.IncidentId);
        builder.HasIndex(x => x.ChangedAt);
        builder.HasIndex(x => new { x.IncidentId, x.ChangedAt });
    }
}