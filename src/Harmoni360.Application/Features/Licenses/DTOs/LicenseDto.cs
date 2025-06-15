namespace Harmoni360.Application.Features.Licenses.DTOs;

public class LicenseDto
{
    public int Id { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string TypeDisplay { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusDisplay { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string PriorityDisplay { get; set; } = string.Empty;

    // Basic License Information
    public string IssuingAuthority { get; set; } = string.Empty;
    public string IssuingAuthorityContact { get; set; } = string.Empty;
    public string IssuedLocation { get; set; } = string.Empty;
    public int HolderId { get; set; }
    public string HolderName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;

    // Dates
    public DateTime IssuedDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime? ActivatedDate { get; set; }
    public DateTime? SuspendedDate { get; set; }
    public DateTime? RevokedDate { get; set; }

    // Renewal Information
    public bool RenewalRequired { get; set; }
    public int RenewalPeriodDays { get; set; }
    public DateTime? NextRenewalDate { get; set; }
    public bool AutoRenewal { get; set; }
    public string RenewalProcedure { get; set; } = string.Empty;

    // Regulatory Information
    public string RegulatoryFramework { get; set; } = string.Empty;
    public string ApplicableRegulations { get; set; } = string.Empty;
    public string ComplianceStandards { get; set; } = string.Empty;

    // Scope and Coverage
    public string Scope { get; set; } = string.Empty;
    public string CoverageAreas { get; set; } = string.Empty;
    public string Restrictions { get; set; } = string.Empty;
    public string ConditionsText { get; set; } = string.Empty;

    // Risk and Compliance
    public string RiskLevel { get; set; } = string.Empty;
    public string RiskLevelDisplay { get; set; } = string.Empty;
    public bool IsCriticalLicense { get; set; }
    public bool RequiresInsurance { get; set; }
    public decimal? RequiredInsuranceAmount { get; set; }

    // Financial Information
    public decimal? LicenseFee { get; set; }
    public string Currency { get; set; } = "USD";

    // Status Information
    public string StatusNotes { get; set; } = string.Empty;

    // Computed Properties
    public int DaysUntilExpiry { get; set; }
    public bool IsExpired { get; set; }
    public bool IsExpiringSoon { get; set; }
    public bool RequiresRenewal { get; set; }

    // Collections
    public List<LicenseAttachmentDto> Attachments { get; set; } = new();
    public List<LicenseRenewalDto> Renewals { get; set; } = new();
    public List<LicenseConditionDto> Conditions { get; set; } = new();

    // Audit Information
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public class LicenseAttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string AttachmentType { get; set; } = string.Empty;
    public string AttachmentTypeDisplay { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public DateTime? ValidUntil { get; set; }
    public bool IsExpired => ValidUntil.HasValue && ValidUntil.Value < DateTime.UtcNow;
}

public class LicenseRenewalDto
{
    public int Id { get; set; }
    public string RenewalNumber { get; set; } = string.Empty;
    public DateTime ApplicationDate { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime? RejectedDate { get; set; }
    public DateTime NewExpiryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusDisplay { get; set; } = string.Empty;
    public string RenewalNotes { get; set; } = string.Empty;
    public decimal? RenewalFee { get; set; }
    public bool DocumentsRequired { get; set; }
    public bool InspectionRequired { get; set; }
    public DateTime? InspectionDate { get; set; }
    public string ProcessedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class LicenseConditionDto
{
    public int Id { get; set; }
    public string ConditionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusDisplay { get; set; } = string.Empty;
    public string ComplianceEvidence { get; set; } = string.Empty;
    public DateTime? ComplianceDate { get; set; }
    public string VerifiedBy { get; set; } = string.Empty;
    public string ResponsiblePerson { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && Status != "Completed";
    public bool IsCompleted => Status == "Completed";
    public int DaysUntilDue => DueDate.HasValue ? Math.Max(0, (DueDate.Value.Date - DateTime.UtcNow.Date).Days) : -1;
}

public class LicenseAuditLogDto
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ActionDescription { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime PerformedAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Comments { get; set; }
}


public class LicenseDetailDto : LicenseDto
{
    public List<LicenseHistoryDto> StatusHistory { get; set; } = new();
    public List<LicenseNotificationDto> Notifications { get; set; } = new();
    public LicenseRiskAssessmentDto? RiskAssessment { get; set; }
}

public class LicenseHistoryDto
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusDisplay { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
}

public class LicenseNotificationDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public string Priority { get; set; } = string.Empty;
}

public class LicenseRiskAssessmentDto
{
    public int Id { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public decimal ComplianceScore { get; set; }
    public string BusinessImpact { get; set; } = string.Empty;
    public string RegulatoryImpact { get; set; } = string.Empty;
    public string FinancialImpact { get; set; } = string.Empty;
    public string ReputationalImpact { get; set; } = string.Empty;
    public string MitigationMeasures { get; set; } = string.Empty;
    public string RecommendedActions { get; set; } = string.Empty;
    public DateTime AssessedAt { get; set; }
    public string AssessedBy { get; set; } = string.Empty;
}