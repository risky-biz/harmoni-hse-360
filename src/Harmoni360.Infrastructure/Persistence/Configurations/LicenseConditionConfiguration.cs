using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class LicenseConditionConfiguration : IEntityTypeConfiguration<LicenseCondition>
{
    public void Configure(EntityTypeBuilder<LicenseCondition> builder)
    {
        builder.ToTable("LicenseConditions");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.LicenseId)
            .IsRequired();

        builder.Property(c => c.ConditionType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.IsMandatory)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.DueDate);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.ComplianceEvidence)
            .HasMaxLength(2000);

        builder.Property(c => c.ComplianceDate);

        builder.Property(c => c.VerifiedBy)
            .HasMaxLength(100);

        builder.Property(c => c.ResponsiblePerson)
            .HasMaxLength(100);

        builder.Property(c => c.Notes)
            .HasMaxLength(2000);

        // Audit fields
        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.LastModifiedAt);

        builder.Property(c => c.LastModifiedBy)
            .HasMaxLength(100);

        // Relationship
        builder.HasOne(c => c.License)
            .WithMany(l => l.LicenseConditions)
            .HasForeignKey(c => c.LicenseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(c => c.LicenseId)
            .HasDatabaseName("IX_LicenseConditions_LicenseId");

        builder.HasIndex(c => c.Status)
            .HasDatabaseName("IX_LicenseConditions_Status");

        builder.HasIndex(c => c.DueDate)
            .HasDatabaseName("IX_LicenseConditions_DueDate");

        builder.HasIndex(c => c.IsMandatory)
            .HasDatabaseName("IX_LicenseConditions_IsMandatory");

        // Computed properties are ignored
        builder.Ignore(c => c.IsOverdue);
    }
}