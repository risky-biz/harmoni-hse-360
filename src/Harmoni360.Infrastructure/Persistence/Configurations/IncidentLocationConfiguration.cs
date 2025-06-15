using Harmoni360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class IncidentLocationConfiguration : IEntityTypeConfiguration<IncidentLocation>
{
    public void Configure(EntityTypeBuilder<IncidentLocation> builder)
    {
        builder.ToTable("IncidentLocations");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(l => l.Description)
            .HasMaxLength(500);

        builder.Property(l => l.Building)
            .HasMaxLength(100);

        builder.Property(l => l.Floor)
            .HasMaxLength(50);

        builder.Property(l => l.Room)
            .HasMaxLength(50);

        builder.Property(l => l.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.LastModifiedBy)
            .HasMaxLength(100);

        // Value object configuration
        builder.OwnsOne(l => l.GeoLocation, geo =>
        {
            geo.Property(g => g.Latitude)
                .HasColumnName("Latitude")
                .HasPrecision(10, 8);

            geo.Property(g => g.Longitude)
                .HasColumnName("Longitude")
                .HasPrecision(11, 8);
        });

        // Indexes
        builder.HasIndex(l => l.Code)
            .IsUnique();

        builder.HasIndex(l => l.Name);
        
        builder.HasIndex(l => l.IsActive);
        
        builder.HasIndex(l => l.Building);

        // Relationships
        builder.HasMany(l => l.Incidents)
            .WithOne(i => i.LocationEntity)
            .HasForeignKey(i => i.LocationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}