using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(200);
        
        // Configure RoleType enum property
        builder.Property(r => r.RoleType)
            .IsRequired()
            .HasConversion<int>();
        
        // Configure IsActive property
        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        // Configure DisplayOrder property
        builder.Property(r => r.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasIndex(r => r.Name)
            .IsUnique();
        
        // Add index on RoleType for better query performance
        builder.HasIndex(r => r.RoleType)
            .IsUnique();
        
        // Add index on IsActive for filtering
        builder.HasIndex(r => r.IsActive);

        // Configure many-to-many relationship with legacy permissions (backward compatibility)
        builder.HasMany(r => r.Permissions)
            .WithMany(p => p.Roles)
            .UsingEntity(
                "RolePermissions",
                l => l.HasOne(typeof(Permission)).WithMany().HasForeignKey("PermissionId").HasPrincipalKey(nameof(Permission.Id)),
                r => r.HasOne(typeof(Role)).WithMany().HasForeignKey("RoleId").HasPrincipalKey(nameof(Role.Id)),
                j => j.HasKey("RoleId", "PermissionId"));
        
        // Configure relationship with RoleModulePermissions
        builder.HasMany(r => r.RoleModulePermissions)
            .WithOne(rmp => rmp.Role)
            .HasForeignKey(rmp => rmp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Configure relationship with UserRoles
        builder.HasMany(r => r.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}