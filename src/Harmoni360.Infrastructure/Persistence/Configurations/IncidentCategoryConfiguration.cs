using Harmoni360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class IncidentCategoryConfiguration : IEntityTypeConfiguration<IncidentCategory>
{
    public void Configure(EntityTypeBuilder<IncidentCategory> builder)
    {
        builder.ToTable("IncidentCategories");

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

        builder.Property(c => c.Icon)
            .IsRequired()
            .HasMaxLength(50);

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

        // Relationships
        builder.HasMany(c => c.Incidents)
            .WithOne(i => i.Category)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}