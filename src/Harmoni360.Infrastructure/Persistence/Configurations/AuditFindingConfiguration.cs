using Harmoni360.Domain.Entities.Audits;
using Harmoni360.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class AuditFindingConfiguration : IEntityTypeConfiguration<AuditFinding>
{
    public void Configure(EntityTypeBuilder<AuditFinding> builder)
    {
        builder.ToTable("AuditFindings");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
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

        builder.Property(e => e.Location)
            .HasMaxLength(200);

        builder.Property(e => e.Equipment)
            .HasMaxLength(200);

        builder.Property(e => e.Standard)
            .HasMaxLength(200);

        builder.Property(e => e.Regulation)
            .HasMaxLength(200);

        builder.Property(e => e.RootCause)
            .HasMaxLength(1000);

        builder.Property(e => e.ImmediateAction)
            .HasMaxLength(1000);

        builder.Property(e => e.CorrectiveAction)
            .HasMaxLength(2000);

        builder.Property(e => e.PreventiveAction)
            .HasMaxLength(2000);

        builder.Property(e => e.ResponsiblePersonName)
            .HasMaxLength(200);

        builder.Property(e => e.ClosureNotes)
            .HasMaxLength(1000);

        builder.Property(e => e.ClosedBy)
            .HasMaxLength(200);

        builder.Property(e => e.VerificationMethod)
            .HasMaxLength(200);

        builder.Property(e => e.VerifiedBy)
            .HasMaxLength(200);

        builder.Property(e => e.BusinessImpact)
            .HasMaxLength(1000);

        builder.Property(e => e.EstimatedCost)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.ActualCost)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.RequiresVerification)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.LastModifiedAt);

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(450);

        builder.Property(e => e.LastModifiedBy)
            .HasMaxLength(450);

        // Relationships
        builder.HasOne(e => e.Audit)
            .WithMany(a => a.Findings)
            .HasForeignKey(e => e.AuditId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.AuditItem)
            .WithMany()
            .HasForeignKey(e => e.AuditItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.ResponsiblePerson)
            .WithMany()
            .HasForeignKey(e => e.ResponsiblePersonId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(e => e.AuditId);
        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.Severity);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.RiskLevel);
        builder.HasIndex(e => e.RequiresVerification);
        builder.HasIndex(e => e.ResponsiblePersonId);
        builder.HasIndex(e => e.DueDate);
        builder.HasIndex(e => e.VerificationDate);
        builder.HasIndex(e => new { e.AuditId, e.Status });
        builder.HasIndex(e => new { e.Status, e.Severity });
        builder.HasIndex(e => new { e.Type, e.Severity });
    }
}