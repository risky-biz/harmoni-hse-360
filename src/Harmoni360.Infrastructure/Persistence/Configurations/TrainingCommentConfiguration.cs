using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class TrainingCommentConfiguration : IEntityTypeConfiguration<TrainingComment>
{
    public void Configure(EntityTypeBuilder<TrainingComment> builder)
    {
        // Primary Key
        builder.HasKey(tc => tc.Id);

        // Basic Properties
        builder.Property(tc => tc.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(tc => tc.AuthorName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(tc => tc.AuthorRole)
            .HasMaxLength(100);

        // Enum Conversions
        builder.Property(tc => tc.CommentType)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        // Visibility and Access
        builder.Property(tc => tc.IsPublic)
            .HasDefaultValue(true);

        builder.Property(tc => tc.IsInstructorOnly)
            .HasDefaultValue(false);

        builder.Property(tc => tc.IsPrivateNote)
            .HasDefaultValue(false);

        builder.Property(tc => tc.IsSystemGenerated)
            .HasDefaultValue(false);

        // Priority and Status
        builder.Property(tc => tc.IsImportant)
            .HasDefaultValue(false);

        builder.Property(tc => tc.RequiresResponse)
            .HasDefaultValue(false);

        builder.Property(tc => tc.IsResolved)
            .HasDefaultValue(false);

        builder.Property(tc => tc.ResolvedBy)
            .HasMaxLength(100);

        // Indonesian Compliance Fields
        builder.Property(tc => tc.IsComplianceNote)
            .HasDefaultValue(false);

        builder.Property(tc => tc.RegulatoryContext)
            .HasMaxLength(500);

        builder.Property(tc => tc.IsK3Related)
            .HasDefaultValue(false);

        builder.Property(tc => tc.K3IssueType)
            .HasMaxLength(200);

        // Feedback Context
        builder.Property(tc => tc.RelatedRating)
            .HasPrecision(3, 2);

        builder.Property(tc => tc.FeedbackCategory)
            .HasMaxLength(200);

        builder.Property(tc => tc.IsAnonymous)
            .HasDefaultValue(false);

        // Attachments and References
        builder.Property(tc => tc.AttachmentPath)
            .HasMaxLength(500);

        builder.Property(tc => tc.ReferencedDocuments)
            .HasMaxLength(1000);

        builder.Property(tc => tc.Tags)
            .HasMaxLength(500);

        // Moderation
        builder.Property(tc => tc.IsModerated)
            .HasDefaultValue(false);

        builder.Property(tc => tc.IsApproved)
            .HasDefaultValue(true);

        builder.Property(tc => tc.ModeratedBy)
            .HasMaxLength(100);

        builder.Property(tc => tc.ModerationNotes)
            .HasMaxLength(1000);

        // Edit History
        builder.Property(tc => tc.IsEdited)
            .HasDefaultValue(false);

        builder.Property(tc => tc.EditReason)
            .HasMaxLength(500);

        // Engagement
        builder.Property(tc => tc.LikeCount)
            .HasDefaultValue(0);

        builder.Property(tc => tc.PinnedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(tc => tc.Training)
            .WithMany(t => t.Comments)
            .HasForeignKey(tc => tc.TrainingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Self-referencing relationship for comment threading
        builder.HasOne(tc => tc.ParentComment)
            .WithMany()
            .HasForeignKey(tc => tc.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for Performance
        builder.HasIndex(tc => tc.TrainingId);

        builder.HasIndex(tc => tc.CommentType);

        builder.HasIndex(tc => tc.AuthorId);

        builder.HasIndex(tc => tc.CommentDate);

        builder.HasIndex(tc => tc.IsPublic);

        builder.HasIndex(tc => tc.IsImportant);

        builder.HasIndex(tc => tc.IsPinned);

        builder.HasIndex(tc => tc.ParentCommentId);

        // Composite indexes for common queries
        builder.HasIndex(tc => new { tc.TrainingId, tc.IsPublic });

        builder.HasIndex(tc => new { tc.TrainingId, tc.CommentType });

        builder.HasIndex(tc => new { tc.TrainingId, tc.IsPinned });

        builder.HasIndex(tc => new { tc.AuthorId, tc.CommentDate });

        // Table Configuration
        builder.ToTable("TrainingComments");

        // Audit fields configuration
        builder.Property(tc => tc.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(tc => tc.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(tc => tc.LastModifiedAt);

        builder.Property(tc => tc.LastModifiedBy)
            .HasMaxLength(100);
    }
}