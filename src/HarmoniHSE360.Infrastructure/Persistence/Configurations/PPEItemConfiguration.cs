using HarmoniHSE360.Domain.Entities;
using HarmoniHSE360.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarmoniHSE360.Infrastructure.Persistence.Configurations;

public class PPEItemConfiguration : IEntityTypeConfiguration<PPEItem>
{
    public void Configure(EntityTypeBuilder<PPEItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ItemCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(i => i.Manufacturer)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.Model)
            .IsRequired()
            .HasMaxLength(100);


        builder.Property(i => i.Color)
            .HasMaxLength(50);

        builder.Property(i => i.Condition)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(i => i.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(i => i.Cost)
            .HasPrecision(18, 2);

        builder.Property(i => i.Location)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(i => i.LastModifiedBy)
            .HasMaxLength(256);

        builder.Property(i => i.Notes)
            .HasMaxLength(2000);

        // Configure CertificationInfo value object
        builder.OwnsOne(i => i.Certification, cert =>
        {
            cert.Property(c => c.CertificationNumber)
                .HasColumnName("CertificationNumber")
                .HasMaxLength(100);

            cert.Property(c => c.CertifyingBody)
                .HasColumnName("CertifyingBody")
                .HasMaxLength(200);

            cert.Property(c => c.CertificationDate)
                .HasColumnName("CertificationDate");

            cert.Property(c => c.ExpiryDate)
                .HasColumnName("CertificationExpiryDate");

            cert.Property(c => c.Standard)
                .HasColumnName("CertificationStandard")
                .HasMaxLength(100);
        });

        // Configure MaintenanceSchedule value object
        builder.OwnsOne(i => i.MaintenanceInfo, maint =>
        {
            maint.Property(m => m.IntervalDays)
                .HasColumnName("MaintenanceIntervalDays");

            maint.Property(m => m.LastMaintenanceDate)
                .HasColumnName("LastMaintenanceDate");

            maint.Property(m => m.NextMaintenanceDate)
                .HasColumnName("NextMaintenanceDate");

            maint.Property(m => m.MaintenanceInstructions)
                .HasColumnName("MaintenanceInstructions")
                .HasMaxLength(1000);
        });

        // Configure relationships
        builder.HasOne(i => i.Category)
            .WithMany(c => c.PPEItems)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Size)
            .WithMany(s => s.PPEItems)
            .HasForeignKey(i => i.SizeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.StorageLocation)
            .WithMany(sl => sl.PPEItems)
            .HasForeignKey(i => i.StorageLocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.AssignedTo)
            .WithMany()
            .HasForeignKey(i => i.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(i => i.Inspections)
            .WithOne(insp => insp.PPEItem)
            .HasForeignKey(insp => insp.PPEItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.AssignmentHistory)
            .WithOne(a => a.PPEItem)
            .HasForeignKey(a => a.PPEItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(i => i.ItemCode)
            .IsUnique();

        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.Condition);
        builder.HasIndex(i => i.CategoryId);
        builder.HasIndex(i => i.SizeId);
        builder.HasIndex(i => i.StorageLocationId);
        builder.HasIndex(i => i.AssignedToId);
        builder.HasIndex(i => i.ExpiryDate);
        builder.HasIndex(i => i.CreatedAt);
        builder.HasIndex(i => new { i.Status, i.CategoryId });

        // Ignore domain events
        builder.Ignore(i => i.DomainEvents);
    }
}