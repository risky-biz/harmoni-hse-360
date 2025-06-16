using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Harmoni360.Domain.ValueObjects;

namespace Harmoni360.Application.Features.WorkPermits.DTOs;

public class WorkPermitDto
{
    public int Id { get; set; }
    public string PermitNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string TypeDisplay { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusDisplay { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string PriorityDisplay { get; set; } = string.Empty;
    
    // Work Details
    public string WorkLocation { get; set; } = string.Empty;
    public GeoLocationDto? GeoLocation { get; set; }
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public int EstimatedDuration { get; set; }
    
    // Personnel
    public int RequestedById { get; set; }
    public string RequestedByName { get; set; } = string.Empty;
    public string RequestedByDepartment { get; set; } = string.Empty;
    public string RequestedByPosition { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string WorkSupervisor { get; set; } = string.Empty;
    public string SafetyOfficer { get; set; } = string.Empty;
    
    // Work Scope
    public string WorkScope { get; set; } = string.Empty;
    public string EquipmentToBeUsed { get; set; } = string.Empty;
    public string MaterialsInvolved { get; set; } = string.Empty;
    public int NumberOfWorkers { get; set; }
    public string ContractorCompany { get; set; } = string.Empty;
    
    // Safety Requirements
    public bool RequiresHotWorkPermit { get; set; }
    public bool RequiresConfinedSpaceEntry { get; set; }
    public bool RequiresElectricalIsolation { get; set; }
    public bool RequiresHeightWork { get; set; }
    public bool RequiresRadiationWork { get; set; }
    public bool RequiresExcavation { get; set; }
    public bool RequiresFireWatch { get; set; }
    public bool RequiresGasMonitoring { get; set; }
    
    // Indonesian Compliance
    public string K3LicenseNumber { get; set; } = string.Empty;
    public string CompanyWorkPermitNumber { get; set; } = string.Empty;
    public bool IsJamsostekCompliant { get; set; }
    public bool HasSMK3Compliance { get; set; }
    public string EnvironmentalPermitNumber { get; set; } = string.Empty;
    
    // Risk Assessment
    public string RiskLevel { get; set; } = string.Empty;
    public string RiskLevelDisplay { get; set; } = string.Empty;
    public string RiskAssessmentSummary { get; set; } = string.Empty;
    public string EmergencyProcedures { get; set; } = string.Empty;
    
    // Completion
    public string CompletionNotes { get; set; } = string.Empty;
    public bool IsCompletedSafely { get; set; }
    public string LessonsLearned { get; set; } = string.Empty;
    
    // Collections
    public List<WorkPermitAttachmentDto> Attachments { get; set; } = new();
    public List<WorkPermitApprovalDto> Approvals { get; set; } = new();
    public List<WorkPermitHazardDto> Hazards { get; set; } = new();
    public List<WorkPermitPrecautionDto> Precautions { get; set; } = new();
    
    // Approval Progress Information
    public string[] RequiredApprovalLevels { get; set; } = Array.Empty<string>();
    public string[] ReceivedApprovalLevels { get; set; } = Array.Empty<string>();
    public string[] MissingApprovalLevels { get; set; } = Array.Empty<string>();
    public int ApprovalProgress { get; set; } // Percentage: (received / required) * 100
    
    // Audit Information
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Computed Properties
    public bool CanEdit => Status is "Draft" or "Rejected";
    public bool CanSubmit => Status == "Draft" && HasRequiredInfo;
    public bool CanApprove => Status == "PendingApproval";
    public bool CanStart => Status == "Approved";
    public bool CanComplete => Status == "InProgress";
    public bool IsExpired => Status != "Completed" && Status != "Cancelled" && PlannedEndDate < DateTime.UtcNow;
    public bool IsOverdue => Status != "Completed" && Status != "Cancelled" && PlannedEndDate < DateTime.UtcNow;
    public bool IsHighRisk => RiskLevel == "High" || RiskLevel == "Critical";
    public bool HasRequiredInfo => !string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(WorkScope) && NumberOfWorkers > 0;
    public int DaysUntilStart => (PlannedStartDate.Date - DateTime.UtcNow.Date).Days;
    public int DaysUntilEnd => (PlannedEndDate.Date - DateTime.UtcNow.Date).Days;
    public int DaysUntilExpiry => DaysUntilEnd;
    public double ProgressPercentage => CalculateProgress();
    public double CompletionPercentage => CalculateProgress();
    public double PrecautionCompletionPercentage => CalculatePrecautionProgress();
    
    private double CalculateProgress()
    {
        return Status switch
        {
            "Draft" => 0,
            "PendingApproval" => 20,
            "Approved" => 40,
            "InProgress" => 70,
            "Completed" => 100,
            "Cancelled" => 0,
            _ => 0
        };
    }
    
    private double CalculatePrecautionProgress()
    {
        if (Precautions == null || Precautions.Count == 0) return 100;
        var completed = Precautions.Count(p => p.IsCompleted);
        return (double)completed / Precautions.Count * 100;
    }
}

public class GeoLocationDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; } = string.Empty;
    public string LocationDescription { get; set; } = string.Empty;
}

public class WorkPermitAttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string AttachmentType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class WorkPermitApprovalDto
{
    public int Id { get; set; }
    public int ApprovedById { get; set; }
    public string ApprovedByName { get; set; } = string.Empty;
    public string ApprovalLevel { get; set; } = string.Empty;
    public DateTime ApprovedAt { get; set; }
    public bool IsApproved { get; set; }
    public string Comments { get; set; } = string.Empty;
    public int ApprovalOrder { get; set; }
    public string K3CertificateNumber { get; set; } = string.Empty;
    public string AuthorityLevel { get; set; } = string.Empty;
}

public class WorkPermitHazardDto
{
    public int Id { get; set; }
    public string HazardDescription { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public int Likelihood { get; set; }
    public int Severity { get; set; }
    public string ControlMeasures { get; set; } = string.Empty;
    public string ResidualRiskLevel { get; set; } = string.Empty;
    public string ResponsiblePerson { get; set; } = string.Empty;
    public bool IsControlImplemented { get; set; }
    public DateTime? ControlImplementedDate { get; set; }
    public string ImplementationNotes { get; set; } = string.Empty;
}

public class WorkPermitPrecautionDto
{
    public int Id { get; set; }
    public string PrecautionDescription { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string CompletedBy { get; set; } = string.Empty;
    public string CompletionNotes { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string ResponsiblePerson { get; set; } = string.Empty;
    public string VerificationMethod { get; set; } = string.Empty;
    public bool RequiresVerification { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string VerifiedBy { get; set; } = string.Empty;
    public bool IsK3Requirement { get; set; }
    public string K3StandardReference { get; set; } = string.Empty;
    public bool IsMandatoryByLaw { get; set; }
}

public class WorkPermitDashboardDto
{
    public int TotalPermits { get; set; }
    public int DraftPermits { get; set; }
    public int PendingApprovalPermits { get; set; }
    public int ApprovedPermits { get; set; }
    public int InProgressPermits { get; set; }
    public int CompletedPermits { get; set; }
    public int RejectedPermits { get; set; }
    public int CancelledPermits { get; set; }
    public int ExpiredPermits { get; set; }
    
    public int HighRiskPermits { get; set; }
    public int CriticalRiskPermits { get; set; }
    public int OverduePermits { get; set; }
    public int PermitsDueToday { get; set; }
    public int PermitsDueThisWeek { get; set; }
    
    public List<WorkPermitTypeStatDto> PermitsByType { get; set; } = new();
    public List<WorkPermitMonthlyTrendDto> MonthlyTrends { get; set; } = new();
    public List<WorkPermitDto> RecentPermits { get; set; } = new();
    public List<WorkPermitDto> HighPriorityPermits { get; set; } = new();
}

public class WorkPermitTypeStatDto
{
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
    public int Percentage { get; set; }
}

public class WorkPermitMonthlyTrendDto
{
    public string Month { get; set; } = string.Empty;
    public int TotalPermits { get; set; }
    public int CompletedPermits { get; set; }
    public int SafelyCompletedPermits { get; set; }
}