using Harmoni360.Domain.Entities.Audits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class AuditItemConfiguration : IEntityTypeConfiguration<AuditItem>
{
    public void Configure(EntityTypeBuilder<AuditItem> builder)
    {
        builder.ToTable("AuditItems");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .IsRequired();

        builder.Property(e => e.ItemNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(e => e.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.Category)
            .HasMaxLength(100);

        builder.Property(e => e.IsRequired)
            .IsRequired();

        builder.Property(e => e.SortOrder)
            .IsRequired();

        builder.Property(e => e.ExpectedResult)
            .HasMaxLength(500);

        builder.Property(e => e.ActualResult)
            .HasMaxLength(500);

        builder.Property(e => e.Comments)
            .HasMaxLength(1000);

        builder.Property(e => e.AssessedBy)
            .HasMaxLength(200);

        builder.Property(e => e.Evidence)
            .HasMaxLength(1000);

        builder.Property(e => e.CorrectiveAction)
            .HasMaxLength(1000);

        builder.Property(e => e.ValidationCriteria)
            .HasMaxLength(500);

        builder.Property(e => e.AcceptanceCriteria)
            .HasMaxLength(500);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.LastModifiedAt);

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(450);

        builder.Property(e => e.LastModifiedBy)
            .HasMaxLength(450);

        // Relationships
        builder.HasOne(e => e.Audit)
            .WithMany(a => a.Items)
            .HasForeignKey(e => e.AuditId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ResponsiblePerson)
            .WithMany()
            .HasForeignKey(e => e.ResponsiblePersonId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(e => e.AuditId);
        builder.HasIndex(e => e.ItemNumber);
        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => e.IsRequired);
        builder.HasIndex(e => e.SortOrder);
        builder.HasIndex(e => e.ResponsiblePersonId);
        builder.HasIndex(e => e.DueDate);
        builder.HasIndex(e => new { e.AuditId, e.Status });
        builder.HasIndex(e => new { e.AuditId, e.SortOrder });
        builder.HasIndex(e => new { e.Status, e.IsRequired });
    }
}