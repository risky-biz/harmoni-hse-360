using Harmoni360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class HealthIncidentConfiguration : IEntityTypeConfiguration<HealthIncident>
{
    public void Configure(EntityTypeBuilder<HealthIncident> builder)
    {
        builder.HasKey(hi => hi.Id);

        builder.Property(hi => hi.IncidentId);

        builder.Property(hi => hi.HealthRecordId)
            .IsRequired();

        builder.Property(hi => hi.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(hi => hi.Severity)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(hi => hi.Symptoms)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(hi => hi.TreatmentProvided)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(hi => hi.TreatmentLocation)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(hi => hi.RequiredHospitalization)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(hi => hi.ParentsNotified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(hi => hi.ParentNotificationTime)
            .HasColumnType("timestamp with time zone");

        builder.Property(hi => hi.ReturnToSchoolDate)
            .HasColumnType("timestamp with time zone");

        builder.Property(hi => hi.FollowUpRequired)
            .HasMaxLength(1000);

        builder.Property(hi => hi.TreatedBy)
            .HasMaxLength(200);

        builder.Property(hi => hi.IncidentDateTime)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(hi => hi.IsResolved)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(hi => hi.ResolutionNotes)
            .HasMaxLength(1000);

        // Audit fields
        builder.Property(hi => hi.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(hi => hi.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(hi => hi.LastModifiedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(hi => hi.LastModifiedBy)
            .HasMaxLength(100);

        // Configure relationships
        builder.HasOne(hi => hi.HealthRecord)
            .WithMany(hr => hr.HealthIncidents)
            .HasForeignKey(hi => hi.HealthRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(hi => hi.Incident)
            .WithMany()
            .HasForeignKey(hi => hi.IncidentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Ignore domain events
        builder.Ignore(hi => hi.DomainEvents);

        // Indexes for performance
        builder.HasIndex(hi => hi.HealthRecordId);
        builder.HasIndex(hi => hi.IncidentId);
        builder.HasIndex(hi => hi.Type);
        builder.HasIndex(hi => hi.Severity);
        builder.HasIndex(hi => hi.IncidentDateTime);
        builder.HasIndex(hi => hi.IsResolved);
        builder.HasIndex(hi => hi.RequiredHospitalization);
        builder.HasIndex(hi => hi.CreatedAt);

        // Composite index for incident analysis
        builder.HasIndex(hi => new { hi.Type, hi.Severity, hi.IncidentDateTime });

        // Table name
        builder.ToTable("HealthIncidents");
    }
}