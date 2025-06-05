using HarmoniHSE360.Domain.Entities;

namespace HarmoniHSE360.Application.Features.Hazards.DTOs;

public class HazardDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime IdentifiedDate { get; set; }
    public DateTime? ExpectedResolutionDate { get; set; }

    // Reporter info
    public string ReporterName { get; set; } = string.Empty;
    public string? ReporterEmail { get; set; }
    public string ReporterDepartment { get; set; } = string.Empty;

    // Current risk assessment summary
    public string? CurrentRiskLevel { get; set; }
    public int? CurrentRiskScore { get; set; }
    public DateTime? LastAssessmentDate { get; set; }

    // Related counts
    public int AttachmentsCount { get; set; }
    public int RiskAssessmentsCount { get; set; }
    public int MitigationActionsCount { get; set; }
    public int PendingActionsCount { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    // Optional detailed objects for expanded views
    public UserDto? Reporter { get; set; }
    public RiskAssessmentDto? CurrentRiskAssessment { get; set; }
}

public class HazardDetailDto : HazardDto
{
    public List<HazardAttachmentDto> Attachments { get; set; } = new();
    public List<RiskAssessmentDto> RiskAssessments { get; set; } = new();
    public List<HazardMitigationActionDto> MitigationActions { get; set; } = new();
    public List<HazardReassessmentDto> Reassessments { get; set; } = new();
}

public class HazardAttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string? Description { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
}

public class RiskAssessmentDto
{
    public int Id { get; set; }
    public int HazardId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string AssessorName { get; set; } = string.Empty;
    public DateTime AssessmentDate { get; set; }
    
    // Risk scoring
    public int ProbabilityScore { get; set; }
    public int SeverityScore { get; set; }
    public int RiskScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    
    // Assessment details
    public string PotentialConsequences { get; set; } = string.Empty;
    public string ExistingControls { get; set; } = string.Empty;
    public string RecommendedActions { get; set; } = string.Empty;
    public string AdditionalNotes { get; set; } = string.Empty;
    
    // Review cycle
    public DateTime NextReviewDate { get; set; }
    public bool IsActive { get; set; }
    
    // Approval
    public bool IsApproved { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalNotes { get; set; }

    // Optional detailed objects
    public UserDto? Assessor { get; set; }
    public UserDto? ApprovedBy { get; set; }
}

public class HazardMitigationActionDto
{
    public int Id { get; set; }
    public int HazardId { get; set; }
    public string ActionDescription { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string AssignedToName { get; set; } = string.Empty;
    public string? CompletionNotes { get; set; }
    
    // Cost tracking
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    
    // Effectiveness
    public int? EffectivenessRating { get; set; }
    public string? EffectivenessNotes { get; set; }
    
    // Verification
    public bool RequiresVerification { get; set; }
    public string? VerifiedByName { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerificationNotes { get; set; }

    // Optional detailed objects
    public UserDto? AssignedTo { get; set; }
    public UserDto? VerifiedBy { get; set; }
}

public class HazardReassessmentDto
{
    public int Id { get; set; }
    public int HazardId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedByName { get; set; }
    public string? CompletionNotes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Optional detailed objects
    public UserDto? CompletedBy { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? EmployeeId { get; set; }
}

public class HazardDashboardDto
{
    // Overview metrics
    public HazardOverviewMetrics Overview { get; set; } = new();
    
    // Risk metrics
    public RiskMetrics RiskAnalysis { get; set; } = new();
    
    // Performance metrics
    public PerformanceMetrics Performance { get; set; } = new();
    
    // Trends and analytics
    public TrendAnalytics Trends { get; set; } = new();
    
    // Location analytics
    public LocationAnalytics LocationData { get; set; } = new();
    
    // Compliance status
    public ComplianceMetrics Compliance { get; set; } = new();
    
    // Recent activities
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
    
    // Alerts and notifications
    public List<HazardAlertDto> Alerts { get; set; } = new();
}

public class HazardOverviewMetrics
{
    public int TotalHazards { get; set; }
    public int OpenHazards { get; set; }
    public int ResolvedHazards { get; set; }
    public int HighRiskHazards { get; set; }
    public int CriticalRiskHazards { get; set; }
    public int OverdueActions { get; set; }
    public int UnassessedHazards { get; set; }
    public int NewHazardsThisMonth { get; set; }
    
    // Percentage changes from previous period
    public double TotalHazardsChange { get; set; }
    public double HighRiskChange { get; set; }
    public double ResolutionRateChange { get; set; }
}

public class RiskMetrics
{
    public Dictionary<string, int> RiskLevelDistribution { get; set; } = new();
    public Dictionary<string, int> CategoryDistribution { get; set; } = new();
    public Dictionary<string, int> SeverityDistribution { get; set; } = new();
    public double AverageRiskScore { get; set; }
    public int RiskAssessmentsCompleted { get; set; }
    public int RiskAssessmentsPending { get; set; }
    public double RiskAssessmentCompletionRate { get; set; }
}

public class PerformanceMetrics
{
    public double AverageResolutionTime { get; set; } // in days
    public double MitigationActionCompletionRate { get; set; }
    public int TotalMitigationActions { get; set; }
    public int CompletedMitigationActions { get; set; }
    public int OverdueMitigationActions { get; set; }
    public double AverageActionEffectiveness { get; set; } // 1-5 scale
    public double CostSavingsFromMitigation { get; set; }
    public Dictionary<string, int> ActionTypeDistribution { get; set; } = new();
}

public class TrendAnalytics
{
    public List<DataPointDto> HazardReportingTrend { get; set; } = new(); // Monthly
    public List<DataPointDto> RiskLevelTrend { get; set; } = new(); // Monthly
    public List<DataPointDto> ResolutionTimeTrend { get; set; } = new(); // Monthly
    public List<DataPointDto> CategoryTrend { get; set; } = new(); // Top categories over time
    public string TrendDirection { get; set; } = string.Empty; // "Improving", "Declining", "Stable"
    public List<string> KeyInsights { get; set; } = new();
}

public class LocationAnalytics
{
    public List<HazardLocationDto> HotspotLocations { get; set; } = new();
    public Dictionary<string, int> DepartmentDistribution { get; set; } = new();
    public List<GeographicClusterDto> GeographicClusters { get; set; } = new();
    public string MostAffectedArea { get; set; } = string.Empty;
    public int LocationsWithHazards { get; set; }
}

public class ComplianceMetrics
{
    public double OverallComplianceScore { get; set; } // Percentage
    public int ComplianceViolations { get; set; }
    public int AuditFindings { get; set; }
    public double RegulatoryReportingCompliance { get; set; }
    public List<string> NonComplianceAreas { get; set; } = new();
    public DateTime? LastComplianceReview { get; set; }
    public DateTime? NextComplianceReview { get; set; }
}

public class RecentActivityDto
{
    public string ActivityType { get; set; } = string.Empty; // "HazardReported", "RiskAssessed", "ActionCompleted"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public int RelatedEntityId { get; set; }
}

public class HazardAlertDto
{
    public string AlertType { get; set; } = string.Empty; // "HighRisk", "Overdue", "Review"
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // "Critical", "High", "Medium", "Low"
    public DateTime CreatedAt { get; set; }
    public int HazardId { get; set; }
    public string HazardTitle { get; set; } = string.Empty;
    public bool IsAcknowledged { get; set; }
}

public class DataPointDto
{
    public string Label { get; set; } = string.Empty; // Date, category, etc.
    public double Value { get; set; }
    public string? SecondaryValue { get; set; } // For additional metrics
}

public class HazardLocationDto
{
    public string Location { get; set; } = string.Empty;
    public int HazardCount { get; set; }
    public int HighRiskCount { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string Department { get; set; } = string.Empty;
}

public class GeographicClusterDto
{
    public double CenterLatitude { get; set; }
    public double CenterLongitude { get; set; }
    public int HazardCount { get; set; }
    public double RadiusMeters { get; set; }
    public string PrimaryCategory { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
}