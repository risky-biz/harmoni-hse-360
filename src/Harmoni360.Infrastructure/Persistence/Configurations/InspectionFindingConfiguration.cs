using Harmoni360.Domain.Entities.Inspections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class InspectionFindingConfiguration : IEntityTypeConfiguration<InspectionFinding>
{
    public void Configure(EntityTypeBuilder<InspectionFinding> builder)
    {
        builder.ToTable("InspectionFindings");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .IsRequired();

        builder.Property(e => e.InspectionId)
            .IsRequired();

        builder.Property(e => e.FindingNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(e => e.FindingNumber)
            .IsUnique();

        builder.Property(e => e.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(e => e.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.Severity)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.RiskLevel)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.RootCause)
            .HasMaxLength(1000);

        builder.Property(e => e.ImmediateAction)
            .HasMaxLength(1000);

        builder.Property(e => e.CorrectiveAction)
            .HasMaxLength(1000);

        builder.Property(e => e.DueDate);

        builder.Property(e => e.ResponsiblePersonId);

        builder.Property(e => e.Location)
            .HasMaxLength(200);

        builder.Property(e => e.Equipment)
            .HasMaxLength(200);

        builder.Property(e => e.Regulation)
            .HasMaxLength(200);

        builder.Property(e => e.ClosedDate);

        builder.Property(e => e.ClosureNotes)
            .HasMaxLength(1000);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.LastModifiedAt);

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(450);

        builder.Property(e => e.LastModifiedBy)
            .HasMaxLength(450);

        // Relationships
        builder.HasOne(e => e.Inspection)
            .WithMany(i => i.Findings)
            .HasForeignKey(e => e.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ResponsiblePerson)
            .WithMany()
            .HasForeignKey(e => e.ResponsiblePersonId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Attachments)
            .WithOne(a => a.Finding)
            .HasForeignKey(a => a.FindingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.InspectionId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.Severity);
        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.DueDate);
        builder.HasIndex(e => e.ResponsiblePersonId);
        builder.HasIndex(e => new { e.Status, e.DueDate });

        // Ignore computed properties
        builder.Ignore(e => e.IsOverdue);
        builder.Ignore(e => e.CanEdit);
        builder.Ignore(e => e.CanClose);
        builder.Ignore(e => e.HasCorrectiveAction);
    }
}