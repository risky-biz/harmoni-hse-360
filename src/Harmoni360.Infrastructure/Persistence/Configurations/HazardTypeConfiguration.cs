using Harmoni360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class HazardTypeConfiguration : IEntityTypeConfiguration<HazardType>
{
    public void Configure(EntityTypeBuilder<HazardType> builder)
    {
        builder.ToTable("HazardTypes");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.RiskMultiplier)
            .HasPrecision(5, 2) // 999.99 max value
            .IsRequired();

        builder.Property(t => t.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.LastModifiedBy)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(t => t.Code)
            .IsUnique();

        builder.HasIndex(t => t.Name);
        
        builder.HasIndex(t => t.IsActive);

        builder.HasIndex(t => t.CategoryId);

        builder.HasIndex(t => t.RequiresPermit);

        // Relationships
        builder.HasOne(t => t.Category)
            .WithMany(c => c.HazardTypes)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(t => t.Hazards)
            .WithOne(h => h.Type)
            .HasForeignKey(h => h.TypeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}