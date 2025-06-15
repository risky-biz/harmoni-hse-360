using Harmoni360.Domain.Entities.Audits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class AuditCommentConfiguration : IEntityTypeConfiguration<AuditComment>
{
    public void Configure(EntityTypeBuilder<AuditComment> builder)
    {
        builder.ToTable("AuditComments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .IsRequired();

        builder.Property(e => e.Comment)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(e => e.CommentedBy)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(e => e.CommentedAt)
            .IsRequired();

        builder.Property(e => e.AuditItemId);

        builder.Property(e => e.AuditFindingId);

        builder.Property(e => e.Category)
            .HasMaxLength(100);

        builder.Property(e => e.IsInternal)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.LastModifiedAt);

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(450);

        builder.Property(e => e.LastModifiedBy)
            .HasMaxLength(450);

        // Relationships
        builder.HasOne(e => e.Audit)
            .WithMany(a => a.Comments)
            .HasForeignKey(e => e.AuditId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.AuditItem)
            .WithMany()
            .HasForeignKey(e => e.AuditItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.AuditFinding)
            .WithMany()
            .HasForeignKey(e => e.AuditFindingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.AuditId);
        builder.HasIndex(e => e.AuditItemId);
        builder.HasIndex(e => e.AuditFindingId);
        builder.HasIndex(e => e.CommentedAt);
        builder.HasIndex(e => e.IsInternal);
        builder.HasIndex(e => new { e.AuditId, e.CommentedAt });
        builder.HasIndex(e => new { e.AuditId, e.Category });
    }
}