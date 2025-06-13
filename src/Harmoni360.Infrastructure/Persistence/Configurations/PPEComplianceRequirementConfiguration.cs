using Harmoni360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class PPEComplianceRequirementConfiguration : IEntityTypeConfiguration<PPEComplianceRequirement>
{
    public void Configure(EntityTypeBuilder<PPEComplianceRequirement> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RiskAssessmentReference)
            .HasMaxLength(200);

        builder.Property(r => r.ComplianceNote)
            .HasMaxLength(1000);

        builder.Property(r => r.TrainingRequirements)
            .HasMaxLength(1000);

        builder.Property(r => r.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.LastModifiedBy)
            .HasMaxLength(256);

        // Configure relationships
        builder.HasOne(r => r.Role)
            .WithMany()
            .HasForeignKey(r => r.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Category)
            .WithMany()
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint - one requirement per role-category combination
        builder.HasIndex(r => new { r.RoleId, r.CategoryId })
            .IsUnique();

        // Other indexes
        builder.HasIndex(r => r.RoleId);
        builder.HasIndex(r => r.CategoryId);
        builder.HasIndex(r => r.IsMandatory);
        builder.HasIndex(r => r.IsActive);

        // Ignore domain events
        builder.Ignore(r => r.DomainEvents);
    }
}