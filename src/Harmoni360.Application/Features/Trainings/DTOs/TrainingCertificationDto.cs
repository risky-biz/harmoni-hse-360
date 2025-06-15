namespace Harmoni360.Application.Features.Trainings.DTOs;

public class TrainingCertificationDto
{
    public int Id { get; set; }
    public int TrainingId { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public int ParticipantUserId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string ParticipantDepartment { get; set; } = string.Empty;
    public string ParticipantPosition { get; set; } = string.Empty;
    
    // Certification Details
    public string CertificationType { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string CertifyingBody { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    
    // Training Results
    public decimal? Score { get; set; }
    public string Grade { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public DateTime CompletionDate { get; set; }
    public int TrainingDurationHours { get; set; }
    
    // Digital Certificate
    public string? DigitalCertificateUrl { get; set; }
    public string? DigitalCertificatePath { get; set; }
    public string VerificationCode { get; set; } = string.Empty;
    public string QRCodeData { get; set; } = string.Empty;
    
    // Competency and Standards
    public string CompetencyAchieved { get; set; } = string.Empty;
    public string LearningObjectivesMet { get; set; } = string.Empty;
    public string SkillsAcquired { get; set; } = string.Empty;
    public string StandardsComplied { get; set; } = string.Empty;
    
    // Indonesian Compliance
    public bool IsK3Certificate { get; set; }
    public string K3CertificationLevel { get; set; } = string.Empty;
    public string GovernmentRegistrationNumber { get; set; } = string.Empty;
    public string MinistryApprovalReference { get; set; } = string.Empty;
    public bool IsBPJSRecognized { get; set; }
    
    // Renewal and Maintenance
    public bool RequiresRenewal { get; set; }
    public DateTime? NextRenewalDate { get; set; }
    public string RenewalRequirements { get; set; } = string.Empty;
    public int ContinuingEducationHours { get; set; }
    
    // Verification and Audit
    public bool IsVerified { get; set; }
    public DateTime? VerificationDate { get; set; }
    public string? VerifiedBy { get; set; }
    public string? VerificationNotes { get; set; }
    public string? AuditTrail { get; set; }
    
    // Printing and Distribution
    public bool IsPrinted { get; set; }
    public DateTime? PrintedDate { get; set; }
    public string? PrintedBy { get; set; }
    public bool IsDistributed { get; set; }
    public DateTime? DistributedDate { get; set; }
    public string DeliveryMethod { get; set; } = string.Empty;
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    
    // Computed Properties
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate < DateTime.Now;
    public bool IsExpiring => ExpiryDate.HasValue && ExpiryDate < DateTime.Now.AddDays(30) && !IsExpired;
    public bool IsValid => Status == "Valid" && !IsExpired;
    public int DaysUntilExpiry => ExpiryDate.HasValue ? (int)(ExpiryDate.Value - DateTime.Now).TotalDays : 0;
    public string StatusBadgeColor => GetStatusBadgeColor(Status, IsExpired, IsExpiring);
    public string CertificationLevel => GetCertificationLevel(Score);
    public bool CanRenew => RequiresRenewal && (IsExpired || IsExpiring);
    public bool CanDownload => !string.IsNullOrEmpty(DigitalCertificateUrl);
    public bool CanPrint => IsValid && !IsPrinted;
    public bool CanVerify => !IsVerified && IsValid;
    
    private static string GetStatusBadgeColor(string status, bool isExpired, bool isExpiring)
    {
        if (isExpired) return "danger";
        if (isExpiring) return "warning";
        
        return status switch
        {
            "Valid" => "success",
            "Expired" => "danger",
            "Revoked" => "dark",
            "Suspended" => "secondary",
            "Pending" => "info",
            _ => "secondary"
        };
    }
    
    private static string GetCertificationLevel(decimal? score)
    {
        if (!score.HasValue) return "Pass";
        
        return score.Value switch
        {
            >= 95 => "Distinction",
            >= 85 => "Merit",
            >= 75 => "Credit",
            >= 65 => "Pass",
            _ => "Fail"
        };
    }
}