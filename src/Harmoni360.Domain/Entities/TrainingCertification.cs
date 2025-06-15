using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;
using Harmoni360.Domain.Events;

namespace Harmoni360.Domain.Entities;

public class TrainingCertification : BaseEntity, IAuditableEntity
{
    public int TrainingId { get; private set; }
    public int UserId { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public string CertificateNumber { get; private set; } = string.Empty;
    public CertificationType CertificationType { get; private set; }
    public string CertifyingBody { get; private set; } = string.Empty;
    
    // Certificate Details
    public string CertificateTitle { get; private set; } = string.Empty;
    public DateTime IssuedDate { get; private set; }
    public DateTime? ValidUntil { get; private set; }
    public bool IsValid { get; private set; } = true;
    public bool IsRevoked { get; private set; } = false;
    public DateTime? RevokedDate { get; private set; }
    public string RevokedBy { get; private set; } = string.Empty;
    public string RevocationReason { get; private set; } = string.Empty;
    
    // Performance Information
    public decimal? FinalScore { get; private set; }
    public decimal? PassingScore { get; private set; }
    public string Grade { get; private set; } = string.Empty; // A, B, C, Pass, Fail, etc.
    public string PerformanceNotes { get; private set; } = string.Empty;
    
    // Issuer Information
    public string IssuedBy { get; private set; } = string.Empty;
    public string IssuedByTitle { get; private set; } = string.Empty;
    public string IssuedByOrganization { get; private set; } = string.Empty;
    public string IssuerLicenseNumber { get; private set; } = string.Empty; // For Indonesian K3 certifiers
    public string DigitalSignature { get; private set; } = string.Empty;
    
    // Indonesian Compliance Fields
    public bool IsK3Certificate { get; private set; } = false;
    public string K3CertificateType { get; private set; } = string.Empty; // AK3 Umum, AK3 Konstruksi, etc.
    public string K3LicenseClass { get; private set; } = string.Empty; // Class A, B, C for K3 licenses
    public bool IsGovernmentRecognized { get; private set; } = false;
    public string MinistryApprovalNumber { get; private set; } = string.Empty;
    public string IndonesianStandardReference { get; private set; } = string.Empty; // SKKNI reference
    public bool IsBPJSCompliant { get; private set; } = false;
    public string BPJSReference { get; private set; } = string.Empty;
    
    // Certificate File Information
    public string CertificateFilePath { get; private set; } = string.Empty;
    public string CertificateFileHash { get; private set; } = string.Empty;
    public string QRCodeData { get; private set; } = string.Empty; // For verification
    public string VerificationUrl { get; private set; } = string.Empty;
    public bool HasWatermark { get; private set; } = false;
    
    // Renewal and Continuation
    public bool RequiresRenewal { get; private set; } = false;
    public DateTime? RenewalDueDate { get; private set; }
    public bool RenewalReminderSent { get; private set; } = false;
    public string RenewalRequirements { get; private set; } = string.Empty;
    public int? RenewedFromCertificateId { get; private set; } // If this is a renewal
    public bool IsRenewal { get; private set; } = false;
    
    // Verification and Authenticity
    public bool IsVerified { get; private set; } = true;
    public DateTime? VerificationDate { get; private set; }
    public string VerifiedBy { get; private set; } = string.Empty;
    public string VerificationMethod { get; private set; } = string.Empty;
    public int VerificationAttempts { get; private set; } = 0;
    public DateTime? LastVerificationAttempt { get; private set; }
    
    // Continuing Education and CPD
    public decimal? CPDCreditsEarned { get; private set; } // Continuing Professional Development
    public string CPDCategory { get; private set; } = string.Empty;
    public bool CountsTowardsCPD { get; private set; } = false;
    public string ProfessionalBodyReference { get; private set; } = string.Empty;
    
    // Usage and Recognition
    public bool IsActiveCredential { get; private set; } = true;
    public string UsageRestrictions { get; private set; } = string.Empty;
    public string GeographicScope { get; private set; } = "Indonesia"; // Where certificate is valid
    public string IndustryScope { get; private set; } = string.Empty; // Which industries recognize it
    
    // Audit and Compliance
    public DateTime? LastAuditDate { get; private set; }
    public string LastAuditResult { get; private set; } = string.Empty;
    public bool RequiresPeriodicAudit { get; private set; } = false;
    public string ComplianceStatus { get; private set; } = "Compliant";
    
    // Navigation Properties
    public Training? Training { get; set; }
    public TrainingCertification? RenewedFromCertificate { get; set; }
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    private TrainingCertification() { } // For EF Core

    public static TrainingCertification Create(
        int trainingId,
        int userId,
        string userName,
        CertificationType certificationType,
        string certifyingBody,
        DateTime? validUntil = null,
        decimal? finalScore = null,
        decimal? passingScore = null,
        string issuedBy = "",
        string certificateTitle = "")
    {
        var certification = new TrainingCertification
        {
            TrainingId = trainingId,
            UserId = userId,
            UserName = userName,
            CertificateNumber = GenerateCertificateNumber(certificationType),
            CertificationType = certificationType,
            CertifyingBody = certifyingBody,
            CertificateTitle = certificateTitle,
            IssuedDate = DateTime.UtcNow,
            ValidUntil = validUntil,
            FinalScore = finalScore,
            PassingScore = passingScore,
            IssuedBy = issuedBy,
            IsValid = true,
            IsActiveCredential = true,
            IsVerified = true,
            VerificationDate = DateTime.UtcNow,
            GeographicScope = "Indonesia",
            ComplianceStatus = "Compliant",
            CreatedAt = DateTime.UtcNow
        };

        // Set grade based on score
        if (finalScore.HasValue && passingScore.HasValue)
        {
            certification.Grade = CalculateGrade(finalScore.Value, passingScore.Value);
        }

        // Set renewal requirements for certain certificate types
        certification.SetRenewalRequirements();

        return certification;
    }

    public static TrainingCertification CreateK3Certificate(
        int trainingId,
        int userId,
        string userName,
        string k3CertificateType,
        string k3LicenseClass,
        string certifyingBody,
        string ministryApprovalNumber,
        DateTime validUntil,
        decimal finalScore,
        string issuedBy,
        string issuerLicenseNumber)
    {
        var certification = Create(
            trainingId, userId, userName, CertificationType.K3Certificate, 
            certifyingBody, validUntil, finalScore, 70.0m, issuedBy);

        certification.SetK3CertificateDetails(
            k3CertificateType, k3LicenseClass, ministryApprovalNumber, issuerLicenseNumber);

        return certification;
    }

    public void SetK3CertificateDetails(
        string k3CertificateType,
        string k3LicenseClass,
        string ministryApprovalNumber,
        string issuerLicenseNumber)
    {
        IsK3Certificate = true;
        K3CertificateType = k3CertificateType;
        K3LicenseClass = k3LicenseClass;
        IsGovernmentRecognized = true;
        MinistryApprovalNumber = ministryApprovalNumber;
        IssuerLicenseNumber = issuerLicenseNumber;
        RequiresRenewal = true; // K3 certificates typically require renewal
        RequiresPeriodicAudit = true;
        
        // Set renewal period based on certificate type
        RenewalDueDate = k3CertificateType.ToLower() switch
        {
            var type when type.Contains("ak3") => ValidUntil?.AddYears(-1), // Renewal required 1 year before expiry
            _ => ValidUntil?.AddMonths(-6) // Standard renewal 6 months before expiry
        };
    }

    public void SetIndonesianComplianceInfo(
        bool isGovernmentRecognized,
        string ministryApprovalNumber = "",
        string indonesianStandardReference = "",
        bool isBPJSCompliant = false,
        string bpjsReference = "")
    {
        IsGovernmentRecognized = isGovernmentRecognized;
        MinistryApprovalNumber = ministryApprovalNumber;
        IndonesianStandardReference = indonesianStandardReference;
        IsBPJSCompliant = isBPJSCompliant;
        BPJSReference = bpjsReference;
    }

    public void SetCertificateFile(string filePath, string fileHash, string qrCodeData = "", string verificationUrl = "")
    {
        CertificateFilePath = filePath;
        CertificateFileHash = fileHash;
        QRCodeData = qrCodeData;
        VerificationUrl = verificationUrl;
        HasWatermark = true; // Assume certificates have watermarks for security
    }

    public void SetCPDInformation(decimal cpdCredits, string cpdCategory, string professionalBodyReference = "")
    {
        CPDCreditsEarned = cpdCredits;
        CPDCategory = cpdCategory;
        ProfessionalBodyReference = professionalBodyReference;
        CountsTowardsCPD = cpdCredits > 0;
    }

    public void AddDigitalSignature(string signature, string signerTitle, string signerOrganization)
    {
        DigitalSignature = signature;
        IssuedByTitle = signerTitle;
        IssuedByOrganization = signerOrganization;
    }

    public void RevokeCertificate(string revokedBy, string reason)
    {
        if (IsRevoked)
            throw new InvalidOperationException("Certificate is already revoked.");

        IsRevoked = true;
        IsValid = false;
        IsActiveCredential = false;
        RevokedDate = DateTime.UtcNow;
        RevokedBy = revokedBy;
        RevocationReason = reason;
        ComplianceStatus = "Revoked";
    }

    public void RestoreCertificate(string restoredBy, string reason)
    {
        if (!IsRevoked)
            throw new InvalidOperationException("Certificate is not revoked.");

        // Check if certificate is still within validity period
        if (ValidUntil.HasValue && DateTime.UtcNow > ValidUntil.Value)
            throw new InvalidOperationException("Cannot restore expired certificate.");

        IsRevoked = false;
        IsValid = true;
        IsActiveCredential = true;
        RevokedDate = null;
        RevokedBy = string.Empty;
        RevocationReason = string.Empty;
        ComplianceStatus = "Compliant";
        
        PerformanceNotes += $"\nCertificate restored by {restoredBy} on {DateTime.UtcNow:yyyy-MM-dd}. Reason: {reason}";
    }

    public void ExtendValidity(DateTime newValidUntil, string extendedBy, string reason)
    {
        if (IsRevoked)
            throw new InvalidOperationException("Cannot extend validity of revoked certificate.");

        if (newValidUntil <= DateTime.UtcNow)
            throw new ArgumentException("New validity date must be in the future.");

        ValidUntil = newValidUntil;
        PerformanceNotes += $"\nValidity extended by {extendedBy} to {newValidUntil:yyyy-MM-dd}. Reason: {reason}";
        
        // Update renewal date if applicable
        if (RequiresRenewal)
        {
            RenewalDueDate = IsK3Certificate ? newValidUntil.AddYears(-1) : newValidUntil.AddMonths(-6);
        }
    }

    public void MarkForRenewal()
    {
        if (!RequiresRenewal)
            throw new InvalidOperationException("This certificate type does not require renewal.");

        if (RenewalDueDate.HasValue && DateTime.UtcNow >= RenewalDueDate.Value)
        {
            ComplianceStatus = "Renewal Required";
        }
    }

    public void SendRenewalReminder()
    {
        RenewalReminderSent = true;
    }

    public TrainingCertification CreateRenewal(
        int newTrainingId,
        DateTime newValidUntil,
        decimal? newFinalScore = null,
        string renewedBy = "")
    {
        if (!RequiresRenewal)
            throw new InvalidOperationException("This certificate type does not support renewal.");

        var renewedCertification = new TrainingCertification
        {
            TrainingId = newTrainingId,
            UserId = UserId,
            UserName = UserName,
            CertificateNumber = GenerateCertificateNumber(CertificationType),
            CertificationType = CertificationType,
            CertifyingBody = CertifyingBody,
            CertificateTitle = CertificateTitle,
            IssuedDate = DateTime.UtcNow,
            ValidUntil = newValidUntil,
            FinalScore = newFinalScore ?? FinalScore,
            PassingScore = PassingScore,
            IssuedBy = renewedBy,
            IsValid = true,
            IsActiveCredential = true,
            IsVerified = true,
            VerificationDate = DateTime.UtcNow,
            RenewedFromCertificateId = Id,
            IsRenewal = true,
            
            // Copy compliance information
            IsK3Certificate = IsK3Certificate,
            K3CertificateType = K3CertificateType,
            K3LicenseClass = K3LicenseClass,
            IsGovernmentRecognized = IsGovernmentRecognized,
            MinistryApprovalNumber = MinistryApprovalNumber,
            IndonesianStandardReference = IndonesianStandardReference,
            IsBPJSCompliant = IsBPJSCompliant,
            BPJSReference = BPJSReference,
            
            GeographicScope = GeographicScope,
            IndustryScope = IndustryScope,
            ComplianceStatus = "Compliant",
            CreatedAt = DateTime.UtcNow
        };

        // Set grade for renewed certificate
        if (newFinalScore.HasValue && PassingScore.HasValue)
        {
            renewedCertification.Grade = CalculateGrade(newFinalScore.Value, PassingScore.Value);
        }

        renewedCertification.SetRenewalRequirements();

        // Mark current certificate as superseded
        IsActiveCredential = false;
        ComplianceStatus = "Superseded by Renewal";

        return renewedCertification;
    }

    public void RecordVerificationAttempt(bool successful, string method, string verifiedBy = "")
    {
        VerificationAttempts++;
        LastVerificationAttempt = DateTime.UtcNow;
        
        if (successful)
        {
            IsVerified = true;
            VerificationDate = DateTime.UtcNow;
            VerifiedBy = verifiedBy;
            VerificationMethod = method;
        }
    }

    public void RecordAudit(string auditResult, bool passed)
    {
        LastAuditDate = DateTime.UtcNow;
        LastAuditResult = auditResult;
        
        if (!passed)
        {
            ComplianceStatus = "Non-Compliant";
            // Could trigger review or revocation process
        }
        else
        {
            ComplianceStatus = "Compliant";
        }
    }

    private void SetRenewalRequirements()
    {
        RequiresRenewal = CertificationType switch
        {
            CertificationType.K3Certificate => true,
            CertificationType.AK3Certificate => true,
            CertificationType.Professional => true,
            CertificationType.Regulatory => true,
            CertificationType.OHSAS_Certificate => true,
            CertificationType.ISO_Certificate => true,
            _ => false
        };

        if (RequiresRenewal && ValidUntil.HasValue)
        {
            RenewalDueDate = CertificationType switch
            {
                CertificationType.K3Certificate => ValidUntil.Value.AddYears(-1),
                CertificationType.AK3Certificate => ValidUntil.Value.AddYears(-1),
                _ => ValidUntil.Value.AddMonths(-6)
            };
        }
    }

    private static string GenerateCertificateNumber(CertificationType type)
    {
        var prefix = type switch
        {
            CertificationType.K3Certificate => "K3",
            CertificationType.AK3Certificate => "AK3",
            CertificationType.Professional => "PROF",
            CertificationType.Regulatory => "REG",
            CertificationType.ISO_Certificate => "ISO",
            CertificationType.OHSAS_Certificate => "OHSAS",
            CertificationType.Competency => "COMP",
            _ => "CERT"
        };

        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        return $"{prefix}-{year:D4}{month:D2}-{timestamp % 100000:D5}";
    }

    private static string CalculateGrade(decimal finalScore, decimal passingScore)
    {
        if (finalScore < passingScore)
            return "Fail";

        if (finalScore >= 90) return "A";
        if (finalScore >= 80) return "B";
        if (finalScore >= 70) return "C";
        if (finalScore >= passingScore) return "Pass";
        return "Fail";
    }

    public bool IsExpired()
    {
        return ValidUntil.HasValue && DateTime.UtcNow > ValidUntil.Value;
    }

    public bool IsNearingExpiry(int daysThreshold = 30)
    {
        return ValidUntil.HasValue && 
               (ValidUntil.Value - DateTime.UtcNow).TotalDays <= daysThreshold &&
               (ValidUntil.Value - DateTime.UtcNow).TotalDays > 0;
    }

    public bool IsRenewalDue()
    {
        return RequiresRenewal && 
               RenewalDueDate.HasValue && 
               DateTime.UtcNow >= RenewalDueDate.Value;
    }

    public int GetDaysUntilExpiry()
    {
        if (!ValidUntil.HasValue)
            return int.MaxValue;

        return Math.Max(0, (int)(ValidUntil.Value - DateTime.UtcNow).TotalDays);
    }

    public string GetValidityStatus()
    {
        if (IsRevoked)
            return "Revoked";

        if (!IsValid)
            return "Invalid";

        if (IsExpired())
            return "Expired";

        if (IsNearingExpiry())
            return "Expiring Soon";

        if (IsRenewalDue())
            return "Renewal Due";

        return "Valid";
    }
}