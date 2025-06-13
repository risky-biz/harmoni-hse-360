using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class WorkPermitPrecautionConfiguration : IEntityTypeConfiguration<WorkPermitPrecaution>
{
    public void Configure(EntityTypeBuilder<WorkPermitPrecaution> builder)
    {
        // Primary Key
        builder.HasKey(wpp => wpp.Id);

        // Properties
        builder.Property(wpp => wpp.PrecautionDescription)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(wpp => wpp.CompletedBy)
            .HasMaxLength(100);

        builder.Property(wpp => wpp.CompletionNotes)
            .HasMaxLength(1000);

        builder.Property(wpp => wpp.ResponsiblePerson)
            .HasMaxLength(100);

        builder.Property(wpp => wpp.VerificationMethod)
            .HasMaxLength(500);

        builder.Property(wpp => wpp.VerifiedBy)
            .HasMaxLength(100);

        builder.Property(wpp => wpp.K3StandardReference)
            .HasMaxLength(200);

        // Enum Conversion
        builder.Property(wpp => wpp.Category)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        // Priority validation
        builder.Property(wpp => wpp.Priority)
            .IsRequired()
            .HasDefaultValue(1);

        // Relationships
        builder.HasOne(wpp => wpp.WorkPermit)
            .WithMany(wp => wp.Precautions)
            .HasForeignKey(wpp => wpp.WorkPermitId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(wpp => wpp.WorkPermitId);
        builder.HasIndex(wpp => wpp.Category);
        builder.HasIndex(wpp => wpp.IsRequired);
        builder.HasIndex(wpp => wpp.IsCompleted);
        builder.HasIndex(wpp => wpp.IsVerified);
        builder.HasIndex(wpp => wpp.Priority);
        builder.HasIndex(wpp => wpp.IsK3Requirement);
        builder.HasIndex(wpp => wpp.IsMandatoryByLaw);

        // Composite indexes
        builder.HasIndex(wpp => new { wpp.WorkPermitId, wpp.IsRequired });
        builder.HasIndex(wpp => new { wpp.WorkPermitId, wpp.IsCompleted });
        builder.HasIndex(wpp => new { wpp.WorkPermitId, wpp.Priority });
        builder.HasIndex(wpp => new { wpp.IsK3Requirement, wpp.IsCompleted });

        // Table Configuration
        builder.ToTable("WorkPermitPrecautions");

        // Audit fields
        builder.Property(wpp => wpp.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(wpp => wpp.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(wpp => wpp.LastModifiedAt);

        builder.Property(wpp => wpp.LastModifiedBy)
            .HasMaxLength(100);
    }
}