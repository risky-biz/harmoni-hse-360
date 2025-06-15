using Harmoni360.Domain.Entities.Inspections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class InspectionCommentConfiguration : IEntityTypeConfiguration<InspectionComment>
{
    public void Configure(EntityTypeBuilder<InspectionComment> builder)
    {
        builder.ToTable("InspectionComments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .IsRequired();

        builder.Property(e => e.InspectionId)
            .IsRequired();

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.Comment)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(e => e.IsInternal)
            .IsRequired();

        builder.Property(e => e.ParentCommentId);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.LastModifiedAt);

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(450);

        builder.Property(e => e.LastModifiedBy)
            .HasMaxLength(450);

        // Relationships
        builder.HasOne(e => e.Inspection)
            .WithMany(i => i.Comments)
            .HasForeignKey(e => e.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(e => e.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.InspectionId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.ParentCommentId);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => new { e.InspectionId, e.CreatedAt });

        // Ignore computed properties
        builder.Ignore(e => e.IsReply);
        builder.Ignore(e => e.HasReplies);
    }
}