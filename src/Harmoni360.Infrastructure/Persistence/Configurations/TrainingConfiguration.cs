using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.ValueObjects;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class TrainingConfiguration : IEntityTypeConfiguration<Training>
{
    public void Configure(EntityTypeBuilder<Training> builder)
    {
        // Primary Key
        builder.HasKey(t => t.Id);

        // Basic Properties
        builder.Property(t => t.TrainingCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(2000);

        builder.Property(t => t.Venue)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(t => t.VenueAddress)
            .HasMaxLength(500);

        builder.Property(t => t.OnlineLink)
            .HasMaxLength(500);

        builder.Property(t => t.OnlinePlatform)
            .HasMaxLength(100);

        // Training Content
        builder.Property(t => t.LearningObjectives)
            .HasMaxLength(2000);

        builder.Property(t => t.CourseOutline)
            .HasMaxLength(4000);

        builder.Property(t => t.Prerequisites)
            .HasMaxLength(2000);

        builder.Property(t => t.Materials)
            .HasMaxLength(2000);

        // Enum Conversions
        builder.Property(t => t.Type)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Category)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Priority)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.DeliveryMethod)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.AssessmentMethod)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.CertificationType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.CertificateValidityPeriod)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Instructor Information
        builder.Property(t => t.InstructorName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.InstructorQualifications)
            .HasMaxLength(500);

        builder.Property(t => t.InstructorContact)
            .HasMaxLength(100);

        // Cost Information
        builder.Property(t => t.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("IDR");

        builder.Property(t => t.CostPerParticipant)
            .HasPrecision(18, 2);

        builder.Property(t => t.TotalBudget)
            .HasPrecision(18, 2);

        // Indonesian Compliance Fields
        builder.Property(t => t.K3RegulationReference)
            .HasMaxLength(200);

        builder.Property(t => t.MinistryApprovalNumber)
            .HasMaxLength(100);

        builder.Property(t => t.IndonesianTrainingStandard)
            .HasMaxLength(200);

        // Certification Details
        builder.Property(t => t.CertifyingBody)
            .HasMaxLength(200);

        // Remove CompetencyStandard as it doesn't exist in entity

        // Evaluation Fields
        builder.Property(t => t.EvaluationSummary)
            .HasMaxLength(2000);

        builder.Property(t => t.ImprovementActions)
            .HasMaxLength(2000);

        builder.Property(t => t.PassingScore)
            .HasPrecision(5, 2);

        builder.Property(t => t.AverageRating)
            .HasPrecision(3, 2);

        // Remove OverallEffectivenessScore as it doesn't exist in entity

        // Value Object Configuration - GeoLocation
        builder.OwnsOne(t => t.GeoLocation, geo =>
        {
            geo.Property(g => g.Latitude)
                .HasColumnName("Latitude")
                .HasPrecision(10, 8);

            geo.Property(g => g.Longitude)
                .HasColumnName("Longitude")
                .HasPrecision(11, 8);
        });

        // Relationships
        builder.HasMany(t => t.Participants)
            .WithOne(p => p.Training)
            .HasForeignKey(p => p.TrainingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Requirements)
            .WithOne(r => r.Training)
            .HasForeignKey(r => r.TrainingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Attachments)
            .WithOne(a => a.Training)
            .HasForeignKey(a => a.TrainingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Comments)
            .WithOne(c => c.Training)
            .HasForeignKey(c => c.TrainingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Certifications)
            .WithOne(c => c.Training)
            .HasForeignKey(c => c.TrainingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for Performance
        builder.HasIndex(t => t.TrainingCode)
            .IsUnique();

        builder.HasIndex(t => t.Type);

        builder.HasIndex(t => t.Category);

        builder.HasIndex(t => t.Status);

        builder.HasIndex(t => t.Priority);

        builder.HasIndex(t => t.DeliveryMethod);

        builder.HasIndex(t => t.ScheduledStartDate);

        builder.HasIndex(t => t.ScheduledEndDate);

        builder.HasIndex(t => t.CreatedAt);

        // Composite indexes for common queries
        builder.HasIndex(t => new { t.Status, t.Type });
        builder.HasIndex(t => new { t.Status, t.ScheduledStartDate });
        builder.HasIndex(t => new { t.Type, t.Category });
        builder.HasIndex(t => new { t.IsK3MandatoryTraining, t.Status });
        
        // Additional performance indexes for common search patterns
        builder.HasIndex(t => new { t.Status, t.Priority, t.ScheduledStartDate });
        builder.HasIndex(t => new { t.Type, t.Status, t.CreatedAt });
        builder.HasIndex(t => new { t.DeliveryMethod, t.Status });
        builder.HasIndex(t => new { t.Category, t.Priority });
        
        // Text search optimization indexes
        builder.HasIndex(t => t.Title)
            .HasDatabaseName("IX_Trainings_Title_Search");
        builder.HasIndex(t => t.InstructorName)
            .HasDatabaseName("IX_Trainings_InstructorName_Search");
        builder.HasIndex(t => t.Venue)
            .HasDatabaseName("IX_Trainings_Venue_Search");
            
        // Performance index for overdue training queries
        builder.HasIndex(t => new { t.Status, t.ScheduledStartDate })
            .HasDatabaseName("IX_Trainings_Overdue_Query");
            
        // Performance index for available spots queries
        builder.HasIndex(t => new { t.Status, t.MaxParticipants })
            .HasDatabaseName("IX_Trainings_AvailableSpots");

        // Table Configuration
        builder.ToTable("Trainings");

        // Audit fields configuration
        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(t => t.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.LastModifiedAt);

        builder.Property(t => t.LastModifiedBy)
            .HasMaxLength(100);
    }
}