using Harmoni360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class HazardCategoryConfiguration : IEntityTypeConfiguration<HazardCategory>
{
    public void Configure(EntityTypeBuilder<HazardCategory> builder)
    {
        builder.ToTable("HazardCategories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.Color)
            .IsRequired()
            .HasMaxLength(7); // Hex color code

        builder.Property(c => c.RiskLevel)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.LastModifiedBy)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.HasIndex(c => c.Name);
        
        builder.HasIndex(c => c.IsActive);

        builder.HasIndex(c => c.RiskLevel);

        // Relationships
        builder.HasMany(c => c.Hazards)
            .WithOne(h => h.Category)
            .HasForeignKey(h => h.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.HazardTypes)
            .WithOne(ht => ht.Category)
            .HasForeignKey(ht => ht.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}