using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class TrainingParticipantConfiguration : IEntityTypeConfiguration<TrainingParticipant>
{
    public void Configure(EntityTypeBuilder<TrainingParticipant> builder)
    {
        // Primary Key
        builder.HasKey(tp => tp.Id);

        // Basic Properties
        builder.Property(tp => tp.UserName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(tp => tp.UserDepartment)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(tp => tp.UserPosition)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(tp => tp.EnrolledBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(tp => tp.UserEmail)
            .HasMaxLength(200);

        builder.Property(tp => tp.UserPhone)
            .HasMaxLength(50);

        // Enum Conversions
        builder.Property(tp => tp.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        // Attendance Information
        builder.Property(tp => tp.AttendancePercentage)
            .HasPrecision(5, 2);

        builder.Property(tp => tp.AttendanceNotes)
            .HasMaxLength(500);

        // Assessment Results
        builder.Property(tp => tp.FinalScore)
            .HasPrecision(5, 2);

        builder.Property(tp => tp.AssessmentMethodUsed)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(tp => tp.AssessmentNotes)
            .HasMaxLength(1000);

        // Certification Information
        builder.Property(tp => tp.CertificateNumber)
            .HasMaxLength(100);

        // Feedback
        builder.Property(tp => tp.TrainingFeedback)
            .HasMaxLength(2000);

        builder.Property(tp => tp.InstructorFeedback)
            .HasMaxLength(2000);

        builder.Property(tp => tp.TrainingRating)
            .HasPrecision(3, 2);

        // Prerequisites
        builder.Property(tp => tp.PrerequisiteNotes)
            .HasMaxLength(1000);

        builder.Property(tp => tp.CompletionNotes)
            .HasMaxLength(1000);

        // Indonesian Compliance Fields
        builder.Property(tp => tp.EmployeeId)
            .HasMaxLength(50);

        builder.Property(tp => tp.K3LicenseNumber)
            .HasMaxLength(100);

        builder.Property(tp => tp.BPJSNumber)
            .HasMaxLength(50);

        builder.Property(tp => tp.WorkPermitNumber)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(tp => tp.Training)
            .WithMany(t => t.Participants)
            .HasForeignKey(tp => tp.TrainingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for Performance
        builder.HasIndex(tp => tp.TrainingId);

        builder.HasIndex(tp => tp.UserId);

        builder.HasIndex(tp => tp.Status);

        builder.HasIndex(tp => tp.EnrolledAt);

        builder.HasIndex(tp => tp.IsEligibleForCertificate);

        // Composite indexes for common queries
        builder.HasIndex(tp => new { tp.TrainingId, tp.UserId })
            .IsUnique();

        builder.HasIndex(tp => new { tp.TrainingId, tp.Status });

        builder.HasIndex(tp => new { tp.UserId, tp.Status });

        builder.HasIndex(tp => new { tp.Status, tp.IsEligibleForCertificate });

        // Table Configuration
        builder.ToTable("TrainingParticipants");

        // Audit fields configuration
        builder.Property(tp => tp.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(tp => tp.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(tp => tp.LastModifiedAt);

        builder.Property(tp => tp.LastModifiedBy)
            .HasMaxLength(100);
    }
}