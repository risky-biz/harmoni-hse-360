using HarmoniHSE360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarmoniHSE360.Infrastructure.Persistence.Configurations;

public class HazardReassessmentConfiguration : IEntityTypeConfiguration<HazardReassessment>
{
    public void Configure(EntityTypeBuilder<HazardReassessment> builder)
    {
        builder.HasKey(hr => hr.Id);

        builder.Property(hr => hr.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(hr => hr.CompletionNotes)
            .HasMaxLength(1000);

        // Configure relationships
        builder.HasOne(hr => hr.Hazard)
            .WithMany(h => h.Reassessments)
            .HasForeignKey(hr => hr.HazardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(hr => hr.CompletedBy)
            .WithMany()
            .HasForeignKey(hr => hr.CompletedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for performance
        builder.HasIndex(hr => hr.HazardId);
        builder.HasIndex(hr => hr.ScheduledDate);
        builder.HasIndex(hr => hr.IsCompleted);
        builder.HasIndex(hr => hr.CompletedAt);
        builder.HasIndex(hr => new { hr.IsCompleted, hr.ScheduledDate }); // Composite index for pending reassessments
    }
}