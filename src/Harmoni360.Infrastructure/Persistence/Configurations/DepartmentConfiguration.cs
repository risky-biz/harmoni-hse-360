using Harmoni360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(d => d.Description)
            .HasMaxLength(500);

        builder.Property(d => d.HeadOfDepartment)
            .HasMaxLength(200);

        builder.Property(d => d.Contact)
            .HasMaxLength(200);

        builder.Property(d => d.Location)
            .HasMaxLength(200);

        builder.Property(d => d.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.LastModifiedBy)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(d => d.Code)
            .IsUnique();

        builder.HasIndex(d => d.Name);
        
        builder.HasIndex(d => d.IsActive);

        // Relationships
        builder.HasMany(d => d.Incidents)
            .WithOne(i => i.DepartmentEntity)
            .HasForeignKey(i => i.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(d => d.CorrectiveActions)
            .WithOne(ca => ca.Department)
            .HasForeignKey(ca => ca.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}