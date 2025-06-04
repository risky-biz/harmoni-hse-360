using HarmoniHSE360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarmoniHSE360.Infrastructure.Persistence.Configurations;

public class CorrectiveActionConfiguration : IEntityTypeConfiguration<CorrectiveAction>
{
    public void Configure(EntityTypeBuilder<CorrectiveAction> builder)
    {
        builder.ToTable("CorrectiveActions");

        builder.HasKey(ca => ca.Id);

        builder.Property(ca => ca.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(ca => ca.AssignedToDepartment)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ca => ca.DueDate)
            .IsRequired();

        builder.Property(ca => ca.CompletedDate)
            .IsRequired(false);

        builder.Property(ca => ca.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(ca => ca.Priority)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(ca => ca.CompletionNotes)
            .HasMaxLength(1000);

        // Audit fields
        builder.Property(ca => ca.CreatedAt)
            .IsRequired();

        builder.Property(ca => ca.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(ca => ca.LastModifiedAt);

        builder.Property(ca => ca.LastModifiedBy)
            .HasMaxLength(256);

        // Relationships
        builder.HasOne<Incident>()
            .WithMany()
            .HasForeignKey(ca => ca.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ca => ca.AssignedTo)
            .WithMany()
            .HasForeignKey(ca => ca.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(ca => ca.IncidentId);
        builder.HasIndex(ca => ca.AssignedToId);
        builder.HasIndex(ca => ca.DueDate);
        builder.HasIndex(ca => ca.Status);
    }
}