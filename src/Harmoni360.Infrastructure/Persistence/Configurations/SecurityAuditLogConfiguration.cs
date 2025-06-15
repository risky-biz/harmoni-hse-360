using Harmoni360.Domain.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class SecurityAuditLogConfiguration : IEntityTypeConfiguration<SecurityAuditLog>
{
    public void Configure(EntityTypeBuilder<SecurityAuditLog> builder)
    {
        builder.HasKey(sal => sal.Id);

        // Properties
        builder.Property(sal => sal.UserName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(sal => sal.UserRole)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(sal => sal.Resource)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sal => sal.Details)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(sal => sal.IpAddress)
            .IsRequired()
            .HasMaxLength(45); // IPv6 support

        builder.Property(sal => sal.UserAgent)
            .HasMaxLength(500);

        builder.Property(sal => sal.SessionId)
            .HasMaxLength(100);

        builder.Property(sal => sal.Metadata)
            .HasColumnType("text");

        // Enum Properties
        builder.Property(sal => sal.Action)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(sal => sal.Category)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(sal => sal.Severity)
            .IsRequired()
            .HasConversion<int>();

        // Navigation Properties
        builder.HasOne(sal => sal.User)
            .WithMany()
            .HasForeignKey(sal => sal.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sal => sal.RelatedIncident)
            .WithMany()
            .HasForeignKey(sal => sal.RelatedIncidentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(sal => sal.UserId)
            .HasDatabaseName("IX_SecurityAuditLogs_UserId");

        builder.HasIndex(sal => sal.ActionTimestamp)
            .HasDatabaseName("IX_SecurityAuditLogs_ActionTimestamp");

        builder.HasIndex(sal => sal.Action)
            .HasDatabaseName("IX_SecurityAuditLogs_Action");

        builder.HasIndex(sal => sal.Category)
            .HasDatabaseName("IX_SecurityAuditLogs_Category");

        builder.HasIndex(sal => sal.Severity)
            .HasDatabaseName("IX_SecurityAuditLogs_Severity");

        builder.HasIndex(sal => sal.IsSecurityCritical)
            .HasDatabaseName("IX_SecurityAuditLogs_IsSecurityCritical");

        builder.HasIndex(sal => sal.RelatedIncidentId)
            .HasDatabaseName("IX_SecurityAuditLogs_RelatedIncidentId");

        builder.HasIndex(sal => sal.IpAddress)
            .HasDatabaseName("IX_SecurityAuditLogs_IpAddress");

        builder.HasIndex(sal => sal.SessionId)
            .HasDatabaseName("IX_SecurityAuditLogs_SessionId");

        // Composite indexes for common queries
        builder.HasIndex(sal => new { sal.UserId, sal.ActionTimestamp })
            .HasDatabaseName("IX_SecurityAuditLogs_User_Timestamp");

        builder.HasIndex(sal => new { sal.Category, sal.ActionTimestamp })
            .HasDatabaseName("IX_SecurityAuditLogs_Category_Timestamp");

        builder.HasIndex(sal => new { sal.Action, sal.ActionTimestamp })
            .HasDatabaseName("IX_SecurityAuditLogs_Action_Timestamp");

        builder.HasIndex(sal => new { sal.IsSecurityCritical, sal.ActionTimestamp })
            .HasDatabaseName("IX_SecurityAuditLogs_Critical_Timestamp");

        builder.HasIndex(sal => new { sal.RelatedIncidentId, sal.ActionTimestamp })
            .HasDatabaseName("IX_SecurityAuditLogs_Incident_Timestamp");

        // Table Name
        builder.ToTable("SecurityAuditLogs");
    }
}