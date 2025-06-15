using Harmoni360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Category)
            .IsRequired()
            .HasMaxLength(100);

        // Create unique index on Name
        builder.HasIndex(p => p.Name)
            .IsUnique();

        // Many-to-many relationship with Roles
        builder.HasMany(p => p.Roles)
            .WithMany(r => r.Permissions)
            .UsingEntity(
                "RolePermissions",
                l => l.HasOne(typeof(Role)).WithMany().HasForeignKey("RoleId").HasPrincipalKey(nameof(Role.Id)),
                r => r.HasOne(typeof(Permission)).WithMany().HasForeignKey("PermissionId").HasPrincipalKey(nameof(Permission.Id)),
                j => j.HasKey("RoleId", "PermissionId"));
    }
}