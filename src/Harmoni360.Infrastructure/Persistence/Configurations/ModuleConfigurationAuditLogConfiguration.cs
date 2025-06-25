using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for ModuleConfigurationAuditLog entity
/// </summary>
public class ModuleConfigurationAuditLogConfiguration : IEntityTypeConfiguration<ModuleConfigurationAuditLog>
{
    public void Configure(EntityTypeBuilder<ModuleConfigurationAuditLog> builder)
    {
        builder.HasKey(mcal => mcal.Id);

        // Configure ModuleType enum property
        builder.Property(mcal => mcal.ModuleType)
            .IsRequired()
            .HasConversion<int>();

        // Configure Action property
        builder.Property(mcal => mcal.Action)
            .IsRequired()
            .HasMaxLength(50);

        // Configure OldValue property as JSON
        builder.Property(mcal => mcal.OldValue)
            .HasColumnType("jsonb"); // PostgreSQL JSONB for better performance

        // Configure NewValue property as JSON
        builder.Property(mcal => mcal.NewValue)
            .HasColumnType("jsonb"); // PostgreSQL JSONB for better performance

        // Configure UserId property
        builder.Property(mcal => mcal.UserId)
            .IsRequired();

        // Configure Timestamp property
        builder.Property(mcal => mcal.Timestamp)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Configure IpAddress property
        builder.Property(mcal => mcal.IpAddress)
            .HasMaxLength(45); // IPv6 maximum length

        // Configure UserAgent property
        builder.Property(mcal => mcal.UserAgent)
            .HasMaxLength(500);

        // Configure Context property
        builder.Property(mcal => mcal.Context)
            .HasMaxLength(1000);

        // Create index on ModuleType for filtering
        builder.HasIndex(mcal => mcal.ModuleType)
            .HasDatabaseName("IX_ModuleConfigurationAuditLog_ModuleType");

        // Create index on Action for filtering
        builder.HasIndex(mcal => mcal.Action)
            .HasDatabaseName("IX_ModuleConfigurationAuditLog_Action");

        // Create index on UserId for filtering
        builder.HasIndex(mcal => mcal.UserId)
            .HasDatabaseName("IX_ModuleConfigurationAuditLog_UserId");

        // Create index on Timestamp for sorting and date range queries
        builder.HasIndex(mcal => mcal.Timestamp)
            .HasDatabaseName("IX_ModuleConfigurationAuditLog_Timestamp");

        // Create composite index for common queries
        builder.HasIndex(mcal => new { mcal.ModuleType, mcal.Timestamp })
            .HasDatabaseName("IX_ModuleConfigurationAuditLog_Module_Timestamp");

        // Configure relationship with User
        builder.HasOne(mcal => mcal.User)
            .WithMany() // User doesn't need a navigation property to audit logs
            .HasForeignKey(mcal => mcal.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Don't cascade delete audit logs

        // Configure table name
        builder.ToTable("ModuleConfigurationAuditLogs");
    }
}