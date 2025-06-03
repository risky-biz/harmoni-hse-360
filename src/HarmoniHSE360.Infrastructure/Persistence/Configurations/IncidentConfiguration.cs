using HarmoniHSE360.Domain.Entities;
using HarmoniHSE360.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarmoniHSE360.Infrastructure.Persistence.Configurations;

public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(i => i.Location)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Severity)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(i => i.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(i => i.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(i => i.LastModifiedBy)
            .HasMaxLength(256);

        // Configure GeoLocation value object
        builder.OwnsOne(i => i.GeoLocation, geo =>
        {
            geo.Property(g => g.Latitude)
                .HasColumnName("Latitude")
                .HasPrecision(18, 6);

            geo.Property(g => g.Longitude)
                .HasColumnName("Longitude")
                .HasPrecision(18, 6);
        });

        // Configure relationships
        builder.HasOne(i => i.Reporter)
            .WithMany()
            .HasForeignKey(i => i.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Investigator)
            .WithMany()
            .HasForeignKey(i => i.InvestigatorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(i => i.Attachments)
            .WithOne()
            .HasForeignKey(a => a.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.InvolvedPersons)
            .WithOne()
            .HasForeignKey(ip => ip.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.CorrectiveActions)
            .WithOne()
            .HasForeignKey(ca => ca.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(i => i.DomainEvents);

        // Indexes
        builder.HasIndex(i => i.Severity);
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.IncidentDate);
        builder.HasIndex(i => i.ReporterId);
        builder.HasIndex(i => i.CreatedAt);
    }
}