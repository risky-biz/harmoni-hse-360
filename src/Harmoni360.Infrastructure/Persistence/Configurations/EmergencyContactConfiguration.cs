using Harmoni360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class EmergencyContactConfiguration : IEntityTypeConfiguration<EmergencyContact>
{
    public void Configure(EntityTypeBuilder<EmergencyContact> builder)
    {
        builder.HasKey(ec => ec.Id);

        builder.Property(ec => ec.HealthRecordId)
            .IsRequired();

        builder.Property(ec => ec.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ec => ec.Relationship)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(ec => ec.CustomRelationship)
            .HasMaxLength(100);

        builder.Property(ec => ec.PrimaryPhone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(ec => ec.SecondaryPhone)
            .HasMaxLength(20);

        builder.Property(ec => ec.Email)
            .HasMaxLength(200);

        builder.Property(ec => ec.Address)
            .HasMaxLength(500);

        builder.Property(ec => ec.IsPrimaryContact)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ec => ec.AuthorizedForPickup)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ec => ec.AuthorizedForMedicalDecisions)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ec => ec.ContactOrder)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(ec => ec.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ec => ec.Notes)
            .HasMaxLength(500);

        // Audit fields
        builder.Property(ec => ec.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(ec => ec.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ec => ec.LastModifiedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(ec => ec.LastModifiedBy)
            .HasMaxLength(100);

        // Configure relationships
        builder.HasOne(ec => ec.HealthRecord)
            .WithMany(hr => hr.EmergencyContacts)
            .HasForeignKey(ec => ec.HealthRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(ec => ec.DomainEvents);

        // Indexes for performance
        builder.HasIndex(ec => ec.HealthRecordId);
        builder.HasIndex(ec => ec.Relationship);
        builder.HasIndex(ec => ec.IsPrimaryContact);
        builder.HasIndex(ec => ec.AuthorizedForPickup);
        builder.HasIndex(ec => ec.AuthorizedForMedicalDecisions);
        builder.HasIndex(ec => ec.IsActive);
        builder.HasIndex(ec => ec.CreatedAt);

        // Composite index for contact priority
        builder.HasIndex(ec => new { ec.HealthRecordId, ec.ContactOrder, ec.IsActive });

        // Table name
        builder.ToTable("EmergencyContacts");
    }
}