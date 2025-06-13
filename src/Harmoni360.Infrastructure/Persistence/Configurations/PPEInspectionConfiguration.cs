using Harmoni360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class PPEInspectionConfiguration : IEntityTypeConfiguration<PPEInspection>
{
    public void Configure(EntityTypeBuilder<PPEInspection> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Result)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(i => i.Findings)
            .HasMaxLength(2000);

        builder.Property(i => i.CorrectiveActions)
            .HasMaxLength(2000);

        builder.Property(i => i.RecommendedCondition)
            .HasConversion<string>();

        builder.Property(i => i.MaintenanceNotes)
            .HasMaxLength(1000);

        builder.Property(i => i.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(i => i.LastModifiedBy)
            .HasMaxLength(256);

        // Store photo paths as serialized string
        builder.Property(i => i.PhotoPathsJson)
            .HasColumnName("PhotoPaths")
            .HasMaxLength(4000);
        
        // Ignore the readonly collection
        builder.Ignore(i => i.PhotoPaths);

        // Configure relationships
        builder.HasOne(i => i.PPEItem)
            .WithMany(item => item.Inspections)
            .HasForeignKey(i => i.PPEItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Inspector)
            .WithMany()
            .HasForeignKey(i => i.InspectorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(i => i.PPEItemId);
        builder.HasIndex(i => i.InspectorId);
        builder.HasIndex(i => i.InspectionDate);
        builder.HasIndex(i => i.NextInspectionDate);
        builder.HasIndex(i => i.Result);
        builder.HasIndex(i => new { i.PPEItemId, i.InspectionDate });

        // Ignore domain events
        builder.Ignore(i => i.DomainEvents);
    }
}