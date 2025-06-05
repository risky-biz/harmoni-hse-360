using HarmoniHSE360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarmoniHSE360.Infrastructure.Persistence.Configurations;

public class PPECategoryConfiguration : IEntityTypeConfiguration<PPECategory>
{
    public void Configure(EntityTypeBuilder<PPECategory> builder)
    {
        builder.ToTable("PPECategories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(c => c.ComplianceStandard)
            .HasMaxLength(200);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit fields
        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.LastModifiedAt);

        builder.Property(c => c.LastModifiedBy)
            .HasMaxLength(255);

        // Indexes
        builder.HasIndex(c => c.Code)
            .IsUnique()
            .HasDatabaseName("IX_PPECategories_Code");

        builder.HasIndex(c => c.Name)
            .HasDatabaseName("IX_PPECategories_Name");
        
        builder.HasIndex(c => c.Type)
            .HasDatabaseName("IX_PPECategories_Type");

        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_PPECategories_IsActive");

        // Relationships
        builder.HasMany(c => c.PPEItems)
            .WithOne(i => i.Category)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore domain events
        builder.Ignore(c => c.DomainEvents);
    }
}