using Harmoni360.Domain.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class SecurityIncidentConfiguration : IEntityTypeConfiguration<SecurityIncident>
{
    public void Configure(EntityTypeBuilder<SecurityIncident> builder)
    {
        builder.HasKey(si => si.Id);

        // Core Properties
        builder.Property(si => si.IncidentNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(si => si.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(si => si.Description)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(si => si.Location)
            .IsRequired()
            .HasMaxLength(200);

        // Enum Properties
        builder.Property(si => si.IncidentType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(si => si.Category)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(si => si.Severity)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(si => si.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(si => si.ThreatLevel)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(si => si.ThreatActorType)
            .HasConversion<int>();

        builder.Property(si => si.Impact)
            .IsRequired()
            .HasConversion<int>();

        // Optional Properties
        builder.Property(si => si.ThreatActorDescription)
            .HasMaxLength(500);

        builder.Property(si => si.ContainmentActions)
            .HasColumnType("text");

        builder.Property(si => si.RootCause)
            .HasColumnType("text");

        // Decimal Properties
        builder.Property(si => si.EstimatedLoss)
            .HasColumnType("decimal(15,2)");

        // GeoLocation as owned entity
        builder.OwnsOne(si => si.GeoLocation, gl =>
        {
            gl.Property(g => g.Latitude)
                .HasColumnName("Latitude")
                .HasColumnType("decimal(10,8)");

            gl.Property(g => g.Longitude)
                .HasColumnName("Longitude")
                .HasColumnType("decimal(11,8)");
        });

        // Navigation Properties
        builder.HasOne(si => si.Reporter)
            .WithMany()
            .HasForeignKey(si => si.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(si => si.AssignedTo)
            .WithMany()
            .HasForeignKey(si => si.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(si => si.Investigator)
            .WithMany()
            .HasForeignKey(si => si.InvestigatorId)
            .OnDelete(DeleteBehavior.SetNull);

        // Collection Relationships
        builder.HasMany(si => si.Attachments)
            .WithOne(a => a.SecurityIncident)
            .HasForeignKey(a => a.SecurityIncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(si => si.InvolvedPersons)
            .WithOne(ip => ip.SecurityIncident)
            .HasForeignKey(ip => ip.SecurityIncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(si => si.Responses)
            .WithOne(r => r.SecurityIncident)
            .HasForeignKey(r => r.SecurityIncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Many-to-many with SecurityControl through SecurityIncidentControls
        builder.HasMany(si => si.ImplementedControls)
            .WithMany(sc => sc.MitigatedIncidents)
            .UsingEntity<Dictionary<string, object>>(
                "SecurityIncidentControls",
                j => j.HasOne<SecurityControl>().WithMany().HasForeignKey("SecurityControlId"),
                j => j.HasOne<SecurityIncident>().WithMany().HasForeignKey("SecurityIncidentId"));

        // Many-to-many with ThreatIndicator through SecurityIncidentIndicators  
        builder.HasMany(si => si.ThreatIndicators)
            .WithMany(ti => ti.RelatedIncidents)
            .UsingEntity<Dictionary<string, object>>(
                "SecurityIncidentIndicators",
                j => j.HasOne<ThreatIndicator>().WithMany().HasForeignKey("ThreatIndicatorId"),
                j => j.HasOne<SecurityIncident>().WithMany().HasForeignKey("SecurityIncidentId"),
                j =>
                {
                    j.Property<DateTime>("DetectedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                    j.Property<string>("Context").HasMaxLength(500);
                });

        // Indexes
        builder.HasIndex(si => si.IncidentNumber)
            .IsUnique();

        builder.HasIndex(si => si.IncidentType)
            .HasDatabaseName("IX_SecurityIncidents_IncidentType");

        builder.HasIndex(si => si.Severity)
            .HasDatabaseName("IX_SecurityIncidents_Severity");

        builder.HasIndex(si => si.Status)
            .HasDatabaseName("IX_SecurityIncidents_Status");

        builder.HasIndex(si => si.ThreatLevel)
            .HasDatabaseName("IX_SecurityIncidents_ThreatLevel");

        builder.HasIndex(si => si.IncidentDateTime)
            .HasDatabaseName("IX_SecurityIncidents_IncidentDateTime");

        builder.HasIndex(si => si.ReporterId)
            .HasDatabaseName("IX_SecurityIncidents_ReporterId");

        builder.HasIndex(si => si.AssignedToId)
            .HasDatabaseName("IX_SecurityIncidents_AssignedToId");

        builder.HasIndex(si => si.InvestigatorId)
            .HasDatabaseName("IX_SecurityIncidents_InvestigatorId");

        builder.HasIndex(si => new { si.IncidentType, si.Status })
            .HasDatabaseName("IX_SecurityIncidents_Type_Status");

        builder.HasIndex(si => new { si.Severity, si.CreatedAt })
            .HasDatabaseName("IX_SecurityIncidents_Severity_CreatedAt");

        // Table Name
        builder.ToTable("SecurityIncidents");
    }
}