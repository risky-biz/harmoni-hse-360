using Harmoni360.Domain.Entities.Audits;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Audits.DTOs;

public class AuditDto
{
    public int Id { get; set; }
    public string AuditNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string TypeDisplay { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CategoryDisplay { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusDisplay { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string PriorityDisplay { get; set; } = string.Empty;
    
    // Schedule & Execution
    public DateTime ScheduledDate { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int AuditorId { get; set; }
    public string AuditorName { get; set; } = string.Empty;
    public int? LocationId { get; set; }
    public string? LocationName { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? FacilityId { get; set; }
    public string? FacilityName { get; set; }
    
    // Assessment Results
    public string RiskLevel { get; set; } = string.Empty;
    public string RiskLevelDisplay { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Recommendations { get; set; }
    public string? OverallScore { get; set; }
    public decimal? ScorePercentage { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public int? ActualDurationMinutes { get; set; }
    
    // Compliance & Standards
    public string? StandardsApplied { get; set; }
    public bool IsRegulatory { get; set; }
    public string? RegulatoryReference { get; set; }
    
    // Scoring and Performance
    public int? TotalPossiblePoints { get; set; }
    public int? AchievedPoints { get; set; }
    
    // Collections
    public List<AuditItemDto> Items { get; set; } = new();
    public List<AuditAttachmentDto> Attachments { get; set; } = new();
    public List<AuditFindingDto> Findings { get; set; } = new();
    public List<AuditCommentDto> Comments { get; set; } = new();
    
    // Audit Information
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Computed Properties
    public bool CanEdit => Status is "Draft" or "Scheduled";
    public bool CanStart => Status == "Scheduled";
    public bool CanComplete => Status == "InProgress";
    public bool CanCancel => Status != "Completed" && Status != "Archived";
    public bool CanArchive => Status == "Completed" || Status == "Cancelled";
    public bool IsOverdue => Status == "Scheduled" && ScheduledDate < DateTime.UtcNow;
    public bool IsHighRisk => RiskLevel == "High" || RiskLevel == "Critical";
    public bool HasFindings => Findings.Any();
    public bool HasCriticalFindings => Findings.Any(f => f.Severity == "Critical");
    public int FindingsCount => Findings.Count;
    public int ItemsCount => Items.Count;
    public int CompletionPercentage => CalculateCompletionPercentage();
    public int DaysUntilScheduled => (ScheduledDate.Date - DateTime.UtcNow.Date).Days;
    public int DaysOverdue => IsOverdue ? (DateTime.UtcNow.Date - ScheduledDate.Date).Days : 0;
    
    private int CalculateCompletionPercentage()
    {
        if (!Items.Any()) return 0;
        var completedItems = Items.Count(i => i.Status == "Completed" || i.Status == "NotApplicable");
        return (int)Math.Round((double)completedItems / Items.Count * 100);
    }
}

public class AuditSummaryDto
{
    public int Id { get; set; }
    public string AuditNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public string AuditorName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public decimal? ScorePercentage { get; set; }
    public int FindingsCount { get; set; }
    public int CompletionPercentage { get; set; }
    public bool IsOverdue { get; set; }
    public bool CanEdit { get; set; }
    public bool CanStart { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AuditItemDto
{
    public int Id { get; set; }
    public int AuditId { get; set; }
    public string ItemNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
    public string? ExpectedResult { get; set; }
    public string? ActualResult { get; set; }
    public string? Comments { get; set; }
    public bool? IsCompliant { get; set; }
    public int? MaxPoints { get; set; }
    public int? ActualPoints { get; set; }
    public string? AssessedBy { get; set; }
    public DateTime? AssessedAt { get; set; }
    public string? Evidence { get; set; }
    public string? CorrectiveAction { get; set; }
    public DateTime? DueDate { get; set; }
    public int? ResponsiblePersonId { get; set; }
    public string? ResponsiblePersonName { get; set; }
    public string? ValidationCriteria { get; set; }
    public string? AcceptanceCriteria { get; set; }
    public bool RequiresFollowUp { get; set; }
    public bool IsOverdue { get; set; }
    public double? ScorePercentage { get; set; }
}

public class AuditFindingDto
{
    public int Id { get; set; }
    public int AuditId { get; set; }
    public string FindingNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string TypeDisplay { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string SeverityDisplay { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusDisplay { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Equipment { get; set; }
    public string? Standard { get; set; }
    public string? Regulation { get; set; }
    public int? AuditItemId { get; set; }
    public string? RootCause { get; set; }
    public string? ImmediateAction { get; set; }
    public string? CorrectiveAction { get; set; }
    public string? PreventiveAction { get; set; }
    public DateTime? DueDate { get; set; }
    public int? ResponsiblePersonId { get; set; }
    public string? ResponsiblePersonName { get; set; }
    public DateTime? ClosedDate { get; set; }
    public string? ClosureNotes { get; set; }
    public string? ClosedBy { get; set; }
    public string? VerificationMethod { get; set; }
    public bool RequiresVerification { get; set; }
    public DateTime? VerificationDate { get; set; }
    public string? VerifiedBy { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public string? BusinessImpact { get; set; }
    public List<FindingAttachmentDto> Attachments { get; set; } = new();
    public bool IsOverdue { get; set; }
    public bool CanEdit { get; set; }
    public bool CanClose { get; set; }
    public bool IsCritical { get; set; }
    public int DaysOverdue { get; set; }
}

public class AuditAttachmentDto
{
    public int Id { get; set; }
    public int AuditId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string AttachmentType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool IsEvidence { get; set; }
    public int? AuditItemId { get; set; }
}

public class FindingAttachmentDto
{
    public int Id { get; set; }
    public int AuditFindingId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string AttachmentType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEvidence { get; set; }
}

public class AuditCommentDto
{
    public int Id { get; set; }
    public int AuditId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string CommentedBy { get; set; } = string.Empty;
    public DateTime CommentedAt { get; set; }
    public int? AuditItemId { get; set; }
    public int? AuditFindingId { get; set; }
    public string? Category { get; set; }
    public bool IsInternal { get; set; }
}

public class AuditDashboardDto
{
    public int TotalAudits { get; set; }
    public int DraftAudits { get; set; }
    public int ScheduledAudits { get; set; }
    public int InProgressAudits { get; set; }
    public int CompletedAudits { get; set; }
    public int OverdueAudits { get; set; }
    public int CancelledAudits { get; set; }
    
    public int HighRiskAudits { get; set; }
    public int CriticalRiskAudits { get; set; }
    public int AuditsDueToday { get; set; }
    public int AuditsDueThisWeek { get; set; }
    
    public decimal AverageScore { get; set; }
    public decimal CompletionRate { get; set; }
    public int TotalFindings { get; set; }
    public int OpenFindings { get; set; }
    public int CriticalFindings { get; set; }
    
    public List<AuditTypeStatDto> AuditsByType { get; set; } = new();
    public List<AuditMonthlyTrendDto> MonthlyTrends { get; set; } = new();
    public List<AuditSummaryDto> RecentAudits { get; set; } = new();
    public List<AuditSummaryDto> HighPriorityAudits { get; set; } = new();
}

public class AuditTypeStatDto
{
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
    public int Percentage { get; set; }
    public decimal AverageScore { get; set; }
}

public class AuditMonthlyTrendDto
{
    public string Month { get; set; } = string.Empty;
    public int TotalAudits { get; set; }
    public int CompletedAudits { get; set; }
    public decimal AverageScore { get; set; }
    public int TotalFindings { get; set; }
}