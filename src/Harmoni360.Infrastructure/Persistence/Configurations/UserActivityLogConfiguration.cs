using Harmoni360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class UserActivityLogConfiguration : IEntityTypeConfiguration<UserActivityLog>
{
    public void Configure(EntityTypeBuilder<UserActivityLog> builder)
    {
        builder.HasKey(ual => ual.Id);

        builder.Property(ual => ual.ActivityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ual => ual.EntityType)
            .HasMaxLength(50);

        builder.Property(ual => ual.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ual => ual.IpAddress)
            .HasMaxLength(45);

        builder.Property(ual => ual.UserAgent)
            .HasMaxLength(500);

        builder.Property(ual => ual.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes for performance
        builder.HasIndex(ual => new { ual.UserId, ual.CreatedAt });
        builder.HasIndex(ual => ual.ActivityType);
        builder.HasIndex(ual => ual.CreatedAt);

        // Foreign key relationship
        builder.HasOne(ual => ual.User)
            .WithMany()
            .HasForeignKey(ual => ual.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}