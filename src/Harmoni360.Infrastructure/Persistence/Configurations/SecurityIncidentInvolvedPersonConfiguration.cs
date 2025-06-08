using Harmoni360.Domain.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class SecurityIncidentInvolvedPersonConfiguration : IEntityTypeConfiguration<SecurityIncidentInvolvedPerson>
{
    public void Configure(EntityTypeBuilder<SecurityIncidentInvolvedPerson> builder)
    {
        builder.HasKey(siip => siip.Id);

        // Properties
        builder.Property(siip => siip.Involvement)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(siip => siip.Statement)
            .HasColumnType("text");

        builder.Property(siip => siip.ContactMethod)
            .HasMaxLength(200);

        builder.Property(siip => siip.AdditionalNotes)
            .HasColumnType("text");

        builder.Property(siip => siip.AddedBy)
            .IsRequired()
            .HasMaxLength(100);

        // Navigation Properties
        builder.HasOne(siip => siip.SecurityIncident)
            .WithMany(si => si.InvolvedPersons)
            .HasForeignKey(siip => siip.SecurityIncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(siip => siip.Person)
            .WithMany()
            .HasForeignKey(siip => siip.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(siip => siip.SecurityIncidentId)
            .HasDatabaseName("IX_SecurityIncidentInvolvedPersons_SecurityIncidentId");

        builder.HasIndex(siip => siip.PersonId)
            .HasDatabaseName("IX_SecurityIncidentInvolvedPersons_PersonId");

        builder.HasIndex(siip => siip.IsWitness)
            .HasDatabaseName("IX_SecurityIncidentInvolvedPersons_IsWitness");

        builder.HasIndex(siip => siip.IsVictim)
            .HasDatabaseName("IX_SecurityIncidentInvolvedPersons_IsVictim");

        builder.HasIndex(siip => siip.IsSuspect)
            .HasDatabaseName("IX_SecurityIncidentInvolvedPersons_IsSuspect");

        builder.HasIndex(siip => siip.StatementTaken)
            .HasDatabaseName("IX_SecurityIncidentInvolvedPersons_StatementTaken");

        builder.HasIndex(siip => new { siip.SecurityIncidentId, siip.PersonId })
            .IsUnique()
            .HasDatabaseName("IX_SecurityIncidentInvolvedPersons_Incident_Person");

        // Table Name
        builder.ToTable("SecurityIncidentInvolvedPersons");
    }
}