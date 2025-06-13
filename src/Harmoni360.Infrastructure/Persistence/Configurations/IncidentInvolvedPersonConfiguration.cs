using Harmoni360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class IncidentInvolvedPersonConfiguration : IEntityTypeConfiguration<IncidentInvolvedPerson>
{
    public void Configure(EntityTypeBuilder<IncidentInvolvedPerson> builder)
    {
        builder.ToTable("IncidentInvolvedPersons");

        builder.HasKey(ip => ip.Id);

        builder.Property(ip => ip.IncidentId)
            .IsRequired();

        builder.Property(ip => ip.PersonId)
            .IsRequired(false); // Make nullable for manual entries

        builder.Property(ip => ip.InvolvementType)
            .IsRequired();

        builder.Property(ip => ip.InjuryDescription)
            .HasMaxLength(1000);

        // Add manual person fields
        builder.Property(ip => ip.ManualPersonName)
            .HasMaxLength(200);

        builder.Property(ip => ip.ManualPersonEmail)
            .HasMaxLength(200);

        // Configure relationship with User (nullable)
        builder.HasOne(ip => ip.Person)
            .WithMany()
            .HasForeignKey(ip => ip.PersonId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Create composite unique index to prevent duplicate person involvement
        // Need to handle nullable PersonId
        builder.HasIndex(ip => new { ip.IncidentId, ip.PersonId })
            .IsUnique()
            .HasDatabaseName("IX_IncidentInvolvedPersons_IncidentId_PersonId")
            .HasFilter("[PersonId] IS NOT NULL");

        // Add index for manual person entries
        builder.HasIndex(ip => new { ip.IncidentId, ip.ManualPersonName })
            .HasDatabaseName("IX_IncidentInvolvedPersons_IncidentId_ManualPersonName");
    }
}