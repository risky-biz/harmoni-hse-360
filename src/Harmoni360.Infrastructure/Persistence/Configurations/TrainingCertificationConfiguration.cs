using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class TrainingCertificationConfiguration : IEntityTypeConfiguration<TrainingCertification>
{
    public void Configure(EntityTypeBuilder<TrainingCertification> builder)
    {
        // Primary Key
        builder.HasKey(tc => tc.Id);

        // Basic Properties
        builder.Property(tc => tc.UserName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(tc => tc.CertificateNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(tc => tc.CertificateTitle)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(tc => tc.CertifyingBody)
            .IsRequired()
            .HasMaxLength(200);

        // Enum Conversions
        builder.Property(tc => tc.CertificationType)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        // Performance Information
        builder.Property(tc => tc.FinalScore)
            .HasPrecision(5, 2);

        builder.Property(tc => tc.PassingScore)
            .HasPrecision(5, 2);

        builder.Property(tc => tc.Grade)
            .HasMaxLength(20);

        builder.Property(tc => tc.PerformanceNotes)
            .HasMaxLength(1000);

        // Issuer Information
        builder.Property(tc => tc.IssuedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(tc => tc.IssuedByTitle)
            .HasMaxLength(100);

        builder.Property(tc => tc.IssuedByOrganization)
            .HasMaxLength(200);

        builder.Property(tc => tc.IssuerLicenseNumber)
            .HasMaxLength(100);

        builder.Property(tc => tc.DigitalSignature)
            .HasMaxLength(500);

        builder.Property(tc => tc.RevokedBy)
            .HasMaxLength(100);

        builder.Property(tc => tc.RevocationReason)
            .HasMaxLength(500);

        // Indonesian Compliance Fields
        builder.Property(tc => tc.K3CertificateType)
            .HasMaxLength(100);

        builder.Property(tc => tc.K3LicenseClass)
            .HasMaxLength(50);

        builder.Property(tc => tc.MinistryApprovalNumber)
            .HasMaxLength(100);

        builder.Property(tc => tc.IndonesianStandardReference)
            .HasMaxLength(100);

        builder.Property(tc => tc.BPJSReference)
            .HasMaxLength(100);

        // Certificate File Information
        builder.Property(tc => tc.CertificateFilePath)
            .HasMaxLength(500);

        builder.Property(tc => tc.CertificateFileHash)
            .HasMaxLength(64);

        builder.Property(tc => tc.QRCodeData)
            .HasMaxLength(500);

        builder.Property(tc => tc.VerificationUrl)
            .HasMaxLength(500);

        // Renewal Information
        builder.Property(tc => tc.RenewalRequirements)
            .HasMaxLength(1000);

        // Verification Information
        builder.Property(tc => tc.VerifiedBy)
            .HasMaxLength(100);

        builder.Property(tc => tc.VerificationMethod)
            .HasMaxLength(200);

        // CPD Information
        builder.Property(tc => tc.CPDCreditsEarned)
            .HasPrecision(5, 2);

        builder.Property(tc => tc.CPDCategory)
            .HasMaxLength(100);

        builder.Property(tc => tc.ProfessionalBodyReference)
            .HasMaxLength(200);

        // Usage and Recognition
        builder.Property(tc => tc.UsageRestrictions)
            .HasMaxLength(1000);

        builder.Property(tc => tc.GeographicScope)
            .HasMaxLength(100)
            .HasDefaultValue("Indonesia");

        builder.Property(tc => tc.IndustryScope)
            .HasMaxLength(500);

        // Audit and Compliance
        builder.Property(tc => tc.LastAuditResult)
            .HasMaxLength(500);

        builder.Property(tc => tc.ComplianceStatus)
            .HasMaxLength(50)
            .HasDefaultValue("Compliant");

        // Boolean defaults
        builder.Property(tc => tc.IsValid)
            .HasDefaultValue(true);

        builder.Property(tc => tc.IsRevoked)
            .HasDefaultValue(false);

        builder.Property(tc => tc.IsK3Certificate)
            .HasDefaultValue(false);

        builder.Property(tc => tc.IsGovernmentRecognized)
            .HasDefaultValue(false);

        builder.Property(tc => tc.IsBPJSCompliant)
            .HasDefaultValue(false);

        builder.Property(tc => tc.HasWatermark)
            .HasDefaultValue(false);

        builder.Property(tc => tc.RequiresRenewal)
            .HasDefaultValue(false);

        builder.Property(tc => tc.RenewalReminderSent)
            .HasDefaultValue(false);

        builder.Property(tc => tc.IsRenewal)
            .HasDefaultValue(false);

        builder.Property(tc => tc.IsVerified)
            .HasDefaultValue(true);

        builder.Property(tc => tc.CountsTowardsCPD)
            .HasDefaultValue(false);

        builder.Property(tc => tc.IsActiveCredential)
            .HasDefaultValue(true);

        builder.Property(tc => tc.RequiresPeriodicAudit)
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(tc => tc.Training)
            .WithMany(t => t.Certifications)
            .HasForeignKey(tc => tc.TrainingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tc => tc.RenewedFromCertificate)
            .WithMany()
            .HasForeignKey(tc => tc.RenewedFromCertificateId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes for Performance
        builder.HasIndex(tc => tc.TrainingId);

        builder.HasIndex(tc => tc.UserId);

        builder.HasIndex(tc => tc.CertificateNumber)
            .IsUnique();

        builder.HasIndex(tc => tc.IssuedDate);

        builder.HasIndex(tc => tc.ValidUntil);

        builder.HasIndex(tc => tc.IsValid);

        builder.HasIndex(tc => tc.IsRevoked);

        builder.HasIndex(tc => tc.IsK3Certificate);

        builder.HasIndex(tc => tc.RequiresRenewal);

        builder.HasIndex(tc => tc.RenewalDueDate);

        // Composite indexes for common queries
        builder.HasIndex(tc => new { tc.TrainingId, tc.UserId });

        builder.HasIndex(tc => new { tc.UserId, tc.IsValid });

        builder.HasIndex(tc => new { tc.IsValid, tc.ValidUntil });

        builder.HasIndex(tc => new { tc.RequiresRenewal, tc.RenewalDueDate });

        builder.HasIndex(tc => new { tc.IsK3Certificate, tc.K3CertificateType });

        // Table Configuration
        builder.ToTable("TrainingCertifications");

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