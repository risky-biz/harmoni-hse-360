using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for ModulePermission entity
/// </summary>
public class ModulePermissionConfiguration : IEntityTypeConfiguration<ModulePermission>
{
    public void Configure(EntityTypeBuilder<ModulePermission> builder)
    {
        builder.HasKey(mp => mp.Id);

        // Configure Module enum property
        builder.Property(mp => mp.Module)
            .IsRequired()
            .HasConversion<int>();

        // Configure Permission enum property
        builder.Property(mp => mp.Permission)
            .IsRequired()
            .HasConversion<int>();

        // Configure Name property
        builder.Property(mp => mp.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Configure Description property
        builder.Property(mp => mp.Description)
            .IsRequired()
            .HasMaxLength(500);

        // Configure IsActive property
        builder.Property(mp => mp.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Create unique index on Module and Permission combination
        builder.HasIndex(mp => new { mp.Module, mp.Permission })
            .IsUnique()
            .HasDatabaseName("IX_ModulePermission_Module_Permission");

        // Create index on IsActive for filtering
        builder.HasIndex(mp => mp.IsActive)
            .HasDatabaseName("IX_ModulePermission_IsActive");

        // Create index on Module for better query performance
        builder.HasIndex(mp => mp.Module)
            .HasDatabaseName("IX_ModulePermission_Module");

        // Configure relationship with RoleModulePermissions
        builder.HasMany(mp => mp.RoleModulePermissions)
            .WithOne(rmp => rmp.ModulePermission)
            .HasForeignKey(rmp => rmp.ModulePermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure table name
        builder.ToTable("ModulePermissions");
    }
}