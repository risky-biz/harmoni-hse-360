using Harmoni360.Domain.Entities.Inspections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class InspectionItemConfiguration : IEntityTypeConfiguration<InspectionItem>
{
    public void Configure(EntityTypeBuilder<InspectionItem> builder)
    {
        builder.ToTable("InspectionItems");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .IsRequired();

        builder.Property(e => e.InspectionId)
            .IsRequired();

        builder.Property(e => e.ChecklistItemId);

        builder.Property(e => e.Question)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.IsRequired)
            .IsRequired();

        builder.Property(e => e.Response)
            .HasMaxLength(2000);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.SortOrder)
            .IsRequired();

        builder.Property(e => e.ExpectedValue)
            .HasMaxLength(100);

        builder.Property(e => e.Unit)
            .HasMaxLength(50);

        builder.Property(e => e.MinValue)
            .HasPrecision(18, 4);

        builder.Property(e => e.MaxValue)
            .HasPrecision(18, 4);

        builder.Property(e => e.Options)
            .HasMaxLength(2000);


        // Relationships
        builder.HasOne(e => e.Inspection)
            .WithMany(i => i.Items)
            .HasForeignKey(e => e.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.InspectionId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => new { e.InspectionId, e.SortOrder });

        // Ignore computed properties
        builder.Ignore(e => e.IsCompliant);
        builder.Ignore(e => e.IsCompleted);
        builder.Ignore(e => e.HasResponse);
    }
}