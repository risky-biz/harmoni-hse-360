using Harmoni360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class PPESizeConfiguration : IEntityTypeConfiguration<PPESize>
{
    public void Configure(EntityTypeBuilder<PPESize> builder)
    {
        builder.ToTable("PPESizes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit fields
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.LastModifiedAt);

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(255);

        // Indexes
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("IX_PPESizes_Code");

        builder.HasIndex(x => x.Name)
            .HasDatabaseName("IX_PPESizes_Name");

        builder.HasIndex(x => x.SortOrder)
            .HasDatabaseName("IX_PPESizes_SortOrder");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_PPESizes_IsActive");

        // Relationships
        builder.HasMany(x => x.PPEItems)
            .WithOne(x => x.Size)
            .HasForeignKey(x => x.SizeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}