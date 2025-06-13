using Harmoni360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class HazardMitigationActionConfiguration : IEntityTypeConfiguration<HazardMitigationAction>
{
    public void Configure(EntityTypeBuilder<HazardMitigationAction> builder)
    {
        builder.HasKey(hma => hma.Id);

        builder.Property(hma => hma.ActionDescription)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(hma => hma.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(hma => hma.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(hma => hma.Priority)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(hma => hma.CompletionNotes)
            .HasMaxLength(1000);

        builder.Property(hma => hma.EffectivenessNotes)
            .HasMaxLength(500);

        builder.Property(hma => hma.VerificationNotes)
            .HasMaxLength(500);

        builder.Property(hma => hma.EstimatedCost)
            .HasColumnType("decimal(18,2)");

        builder.Property(hma => hma.ActualCost)
            .HasColumnType("decimal(18,2)");

        builder.Property(hma => hma.EffectivenessRating)
            .HasComment("Effectiveness rating (1-5)");

        builder.Property(hma => hma.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(hma => hma.LastModifiedBy)
            .HasMaxLength(256);

        // Configure relationships
        builder.HasOne(hma => hma.Hazard)
            .WithMany(h => h.MitigationActions)
            .HasForeignKey(hma => hma.HazardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(hma => hma.AssignedTo)
            .WithMany()
            .HasForeignKey(hma => hma.AssignedToId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(hma => hma.VerifiedBy)
            .WithMany()
            .HasForeignKey(hma => hma.VerifiedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore domain events
        builder.Ignore(hma => hma.DomainEvents);

        // Indexes for performance
        builder.HasIndex(hma => hma.HazardId);
        builder.HasIndex(hma => hma.Type);
        builder.HasIndex(hma => hma.Status);
        builder.HasIndex(hma => hma.Priority);
        builder.HasIndex(hma => hma.TargetDate);
        builder.HasIndex(hma => hma.AssignedToId);
        builder.HasIndex(hma => hma.RequiresVerification);
        builder.HasIndex(hma => new { hma.Status, hma.TargetDate }); // Composite index for overdue actions
        builder.HasIndex(hma => new { hma.HazardId, hma.Status }); // Composite index for hazard's actions by status
        builder.HasIndex(hma => new { hma.AssignedToId, hma.Status }); // Composite index for user's assigned actions
    }
}