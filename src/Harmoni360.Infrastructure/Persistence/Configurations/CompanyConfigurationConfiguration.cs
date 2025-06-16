using Harmoni360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class CompanyConfigurationConfiguration : IEntityTypeConfiguration<CompanyConfiguration>
{
    public void Configure(EntityTypeBuilder<CompanyConfiguration> builder)
    {
        builder.ToTable("CompanyConfigurations");

        builder.HasKey(c => c.Id);

        // Basic Information
        builder.Property(c => c.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.CompanyCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.CompanyDescription)
            .HasMaxLength(1000);

        builder.Property(c => c.WebsiteUrl)
            .HasMaxLength(500);

        builder.Property(c => c.LogoUrl)
            .HasMaxLength(500);

        builder.Property(c => c.FaviconUrl)
            .HasMaxLength(500);

        // Contact Information
        builder.Property(c => c.PrimaryEmail)
            .HasMaxLength(200);

        builder.Property(c => c.PrimaryPhone)
            .HasMaxLength(50);

        builder.Property(c => c.EmergencyContactNumber)
            .HasMaxLength(50);

        // Address Information
        builder.Property(c => c.Address)
            .HasMaxLength(500);

        builder.Property(c => c.City)
            .HasMaxLength(100);

        builder.Property(c => c.State)
            .HasMaxLength(100);

        builder.Property(c => c.PostalCode)
            .HasMaxLength(20);

        builder.Property(c => c.Country)
            .HasMaxLength(100);

        // Geographic Coordinates
        builder.Property(c => c.DefaultLatitude)
            .HasPrecision(10, 8);

        builder.Property(c => c.DefaultLongitude)
            .HasPrecision(11, 8);

        // Branding & Themes
        builder.Property(c => c.PrimaryColor)
            .HasMaxLength(7); // Hex color code

        builder.Property(c => c.SecondaryColor)
            .HasMaxLength(7);

        builder.Property(c => c.AccentColor)
            .HasMaxLength(7);

        // Compliance & Industry
        builder.Property(c => c.IndustryType)
            .HasMaxLength(100);

        builder.Property(c => c.ComplianceStandards)
            .HasMaxLength(500);

        builder.Property(c => c.RegulatoryAuthority)
            .HasMaxLength(200);

        // System Settings
        builder.Property(c => c.TimeZone)
            .HasMaxLength(100);

        builder.Property(c => c.DateFormat)
            .HasMaxLength(50);

        builder.Property(c => c.Currency)
            .HasMaxLength(10);

        builder.Property(c => c.Language)
            .HasMaxLength(10);

        // System fields
        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.Version)
            .IsRequired()
            .HasDefaultValue(1);

        // Auditable fields
        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.LastModifiedAt);

        builder.Property(c => c.LastModifiedBy)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(c => c.CompanyCode)
            .IsUnique();

        builder.HasIndex(c => c.CompanyName);

        builder.HasIndex(c => c.IsActive);

        builder.HasIndex(c => c.Version);

        // Only allow one active configuration at a time
        builder.HasIndex(c => c.IsActive)
            .IsUnique()
            .HasFilter("\"IsActive\" = true");
    }
}