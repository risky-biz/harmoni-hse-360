using Harmoni360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class HealthRecordConfiguration : IEntityTypeConfiguration<HealthRecord>
{
    public void Configure(EntityTypeBuilder<HealthRecord> builder)
    {
        builder.HasKey(hr => hr.Id);

        builder.Property(hr => hr.PersonId)
            .IsRequired();

        builder.Property(hr => hr.PersonType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(hr => hr.DateOfBirth)
            .HasColumnType("timestamp with time zone");

        builder.Property(hr => hr.BloodType)
            .HasConversion<int>();

        builder.Property(hr => hr.MedicalNotes)
            .HasMaxLength(2000);

        builder.Property(hr => hr.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit fields
        builder.Property(hr => hr.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(hr => hr.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(hr => hr.LastModifiedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(hr => hr.LastModifiedBy)
            .HasMaxLength(100);

        // Configure relationships
        builder.HasOne(hr => hr.Person)
            .WithMany()
            .HasForeignKey(hr => hr.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(hr => hr.MedicalConditions)
            .WithOne(mc => mc.HealthRecord)
            .HasForeignKey(mc => mc.HealthRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(hr => hr.Vaccinations)
            .WithOne(v => v.HealthRecord)
            .HasForeignKey(v => v.HealthRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(hr => hr.HealthIncidents)
            .WithOne(hi => hi.HealthRecord)
            .HasForeignKey(hi => hi.HealthRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(hr => hr.EmergencyContacts)
            .WithOne(ec => ec.HealthRecord)
            .HasForeignKey(ec => ec.HealthRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(hr => hr.DomainEvents);

        // Indexes for performance
        builder.HasIndex(hr => hr.PersonId)
            .IsUnique(); // One health record per person

        builder.HasIndex(hr => hr.PersonType);
        builder.HasIndex(hr => hr.IsActive);
        builder.HasIndex(hr => hr.CreatedAt);

        // Table name
        builder.ToTable("HealthRecords");
    }
}