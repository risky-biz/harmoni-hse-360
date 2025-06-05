using HarmoniHSE360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarmoniHSE360.Infrastructure.Persistence.Configurations;

public class PPEStorageLocationConfiguration : IEntityTypeConfiguration<PPEStorageLocation>
{
    public void Configure(EntityTypeBuilder<PPEStorageLocation> builder)
    {
        builder.ToTable("PPEStorageLocations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.ContactPerson)
            .HasMaxLength(200);

        builder.Property(x => x.ContactPhone)
            .HasMaxLength(50);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.Capacity)
            .IsRequired()
            .HasDefaultValue(1000);

        builder.Property(x => x.CurrentStock)
            .IsRequired()
            .HasDefaultValue(0);

        // Audit fields
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.LastModifiedAt);

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(255);

        // Indexes
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("IX_PPEStorageLocations_Code");

        builder.HasIndex(x => x.Name)
            .HasDatabaseName("IX_PPEStorageLocations_Name");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_PPEStorageLocations_IsActive");

        // Relationships
        builder.HasMany(x => x.PPEItems)
            .WithOne(x => x.StorageLocation)
            .HasForeignKey(x => x.StorageLocationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}