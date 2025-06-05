using HarmoniHSE360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarmoniHSE360.Infrastructure.Persistence.Configurations;

public class PPEAssignmentConfiguration : IEntityTypeConfiguration<PPEAssignment>
{
    public void Configure(EntityTypeBuilder<PPEAssignment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AssignedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.ReturnedBy)
            .HasMaxLength(256);

        builder.Property(a => a.Purpose)
            .HasMaxLength(500);

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(a => a.ReturnNotes)
            .HasMaxLength(1000);

        builder.Property(a => a.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.LastModifiedBy)
            .HasMaxLength(256);

        // Configure relationships
        builder.HasOne(a => a.PPEItem)
            .WithMany(i => i.AssignmentHistory)
            .HasForeignKey(a => a.PPEItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.AssignedTo)
            .WithMany()
            .HasForeignKey(a => a.AssignedToId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(a => a.PPEItemId);
        builder.HasIndex(a => a.AssignedToId);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.AssignedDate);
        builder.HasIndex(a => a.ReturnedDate);
        builder.HasIndex(a => new { a.PPEItemId, a.Status });

        // Ignore domain events
        builder.Ignore(a => a.DomainEvents);
    }
}