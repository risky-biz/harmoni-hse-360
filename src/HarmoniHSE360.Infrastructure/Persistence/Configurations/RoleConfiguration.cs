using HarmoniHSE360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarmoniHSE360.Infrastructure.Persistence.Configurations;

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

        builder.HasIndex(r => r.Name)
            .IsUnique();

        // Configure many-to-many relationship with permissions
        builder.HasMany(r => r.Permissions)
            .WithMany(p => p.Roles)
            .UsingEntity(
                "RolePermissions",
                l => l.HasOne(typeof(Permission)).WithMany().HasForeignKey("PermissionId").HasPrincipalKey(nameof(Permission.Id)),
                r => r.HasOne(typeof(Role)).WithMany().HasForeignKey("RoleId").HasPrincipalKey(nameof(Role.Id)),
                j => j.HasKey("RoleId", "PermissionId"));
    }
}


public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasKey(ur => ur.Id);

        builder.HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique();

        // Configure relationship with Role
        builder.HasOne(ur => ur.Role)
            .WithMany()
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}