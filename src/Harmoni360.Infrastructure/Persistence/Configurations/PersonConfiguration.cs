using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("Persons");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.PhoneNumber)
            .HasMaxLength(50);

        builder.Property(p => p.Department)
            .HasMaxLength(100);

        builder.Property(p => p.Position)
            .HasMaxLength(100);

        builder.Property(p => p.EmployeeId)
            .HasMaxLength(50);

        builder.Property(p => p.PersonType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.LastModifiedAt);

        builder.Property(p => p.LastModifiedBy)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(p => p.Email)
            .IsUnique()
            .HasDatabaseName("IX_Persons_Email");

        builder.HasIndex(p => new { p.EmployeeId, p.IsActive })
            .HasDatabaseName("IX_Persons_EmployeeId_IsActive")
            .HasFilter("\"EmployeeId\" IS NOT NULL");

        builder.HasIndex(p => new { p.PersonType, p.IsActive })
            .HasDatabaseName("IX_Persons_PersonType_IsActive");

        // Foreign key to User (optional)
        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Navigation to health records
        builder.HasMany(p => p.HealthRecords)
            .WithOne(hr => hr.Person)
            .HasForeignKey(hr => hr.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}