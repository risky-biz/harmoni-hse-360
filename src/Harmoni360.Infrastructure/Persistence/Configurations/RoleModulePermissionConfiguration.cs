using Harmoni360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for RoleModulePermission entity
/// </summary>
public class RoleModulePermissionConfiguration : IEntityTypeConfiguration<RoleModulePermission>
{
    public void Configure(EntityTypeBuilder<RoleModulePermission> builder)
    {
        builder.HasKey(rmp => rmp.Id);

        // Configure RoleId property
        builder.Property(rmp => rmp.RoleId)
            .IsRequired();

        // Configure ModulePermissionId property
        builder.Property(rmp => rmp.ModulePermissionId)
            .IsRequired();

        // Configure IsActive property
        builder.Property(rmp => rmp.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Configure GrantedAt property
        builder.Property(rmp => rmp.GrantedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Configure GrantedByUserId property (nullable)
        builder.Property(rmp => rmp.GrantedByUserId)
            .IsRequired(false);

        // Configure GrantReason property (nullable)
        builder.Property(rmp => rmp.GrantReason)
            .IsRequired(false)
            .HasMaxLength(500);

        // Create unique index on RoleId and ModulePermissionId combination
        builder.HasIndex(rmp => new { rmp.RoleId, rmp.ModulePermissionId })
            .IsUnique()
            .HasDatabaseName("IX_RoleModulePermission_Role_ModulePermission");

        // Create index on IsActive for filtering
        builder.HasIndex(rmp => rmp.IsActive)
            .HasDatabaseName("IX_RoleModulePermission_IsActive");

        // Create index on RoleId for better query performance
        builder.HasIndex(rmp => rmp.RoleId)
            .HasDatabaseName("IX_RoleModulePermission_RoleId");

        // Create index on ModulePermissionId for better query performance
        builder.HasIndex(rmp => rmp.ModulePermissionId)
            .HasDatabaseName("IX_RoleModulePermission_ModulePermissionId");

        // Create index on GrantedByUserId for auditing queries
        builder.HasIndex(rmp => rmp.GrantedByUserId)
            .HasDatabaseName("IX_RoleModulePermission_GrantedByUserId");

        // Configure relationship with Role
        builder.HasOne(rmp => rmp.Role)
            .WithMany(r => r.RoleModulePermissions)
            .HasForeignKey(rmp => rmp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship with ModulePermission
        builder.HasOne(rmp => rmp.ModulePermission)
            .WithMany(mp => mp.RoleModulePermissions)
            .HasForeignKey(rmp => rmp.ModulePermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship with GrantedByUser (User entity)
        builder.HasOne(rmp => rmp.GrantedByUser)
            .WithMany()
            .HasForeignKey(rmp => rmp.GrantedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure table name
        builder.ToTable("RoleModulePermissions");
    }
}