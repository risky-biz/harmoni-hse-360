using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.EmployeeId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Department)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Position)
            .IsRequired()
            .HasMaxLength(100);

        // HSSE-specific field configurations
        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(u => u.EmergencyContactName)
            .HasMaxLength(100);

        builder.Property(u => u.EmergencyContactPhone)
            .HasMaxLength(20);

        builder.Property(u => u.SupervisorEmployeeId)
            .HasMaxLength(50);

        builder.Property(u => u.WorkLocation)
            .HasMaxLength(100);

        builder.Property(u => u.CostCenter)
            .HasMaxLength(50);

        builder.Property(u => u.PreferredLanguage)
            .HasMaxLength(5)
            .HasDefaultValue("en");

        builder.Property(u => u.TimeZone)
            .HasMaxLength(50);

        builder.Property(u => u.Status)
            .HasConversion<int>()
            .HasDefaultValue(UserStatus.Active);

        builder.Property(u => u.RequiresMFA)
            .HasDefaultValue(false);

        builder.Property(u => u.FailedLoginAttempts)
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.EmployeeId)
            .IsUnique();

        builder.HasIndex(u => u.Status);
        builder.HasIndex(u => u.Department);
        builder.HasIndex(u => u.WorkLocation);
        builder.HasIndex(u => u.SupervisorEmployeeId);

        // Configure navigation properties
        builder.HasMany(u => u.UserRoles)
            .WithOne()
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}