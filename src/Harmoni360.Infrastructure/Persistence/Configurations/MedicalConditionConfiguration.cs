using Harmoni360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class MedicalConditionConfiguration : IEntityTypeConfiguration<MedicalCondition>
{
    public void Configure(EntityTypeBuilder<MedicalCondition> builder)
    {
        builder.HasKey(mc => mc.Id);

        builder.Property(mc => mc.HealthRecordId)
            .IsRequired();

        builder.Property(mc => mc.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(mc => mc.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(mc => mc.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(mc => mc.Severity)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(mc => mc.TreatmentPlan)
            .HasMaxLength(1000);

        builder.Property(mc => mc.DiagnosedDate)
            .HasColumnType("timestamp with time zone");

        builder.Property(mc => mc.RequiresEmergencyAction)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(mc => mc.EmergencyInstructions)
            .HasMaxLength(1000);

        builder.Property(mc => mc.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit fields
        builder.Property(mc => mc.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(mc => mc.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(mc => mc.LastModifiedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(mc => mc.LastModifiedBy)
            .HasMaxLength(100);

        // Configure relationships
        builder.HasOne(mc => mc.HealthRecord)
            .WithMany(hr => hr.MedicalConditions)
            .HasForeignKey(mc => mc.HealthRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(mc => mc.DomainEvents);

        // Indexes for performance
        builder.HasIndex(mc => mc.HealthRecordId);
        builder.HasIndex(mc => mc.Type);
        builder.HasIndex(mc => mc.Severity);
        builder.HasIndex(mc => mc.RequiresEmergencyAction);
        builder.HasIndex(mc => mc.IsActive);
        builder.HasIndex(mc => mc.CreatedAt);

        // Table name
        builder.ToTable("MedicalConditions");
    }
}