using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Inspections.DTOs;

public class InspectionDto
{
    public int Id { get; set; }
    public string InspectionNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InspectionType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public InspectionCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public InspectionStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public InspectionPriority Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int InspectorId { get; set; }
    public string InspectorName { get; set; } = string.Empty;
    public int? LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int? FacilityId { get; set; }
    public string FacilityName { get; set; } = string.Empty;
    public RiskLevel RiskLevel { get; set; }
    public string RiskLevelName { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Recommendations { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public int? ActualDurationMinutes { get; set; }
    public int ItemsCount { get; set; }
    public int CompletedItemsCount { get; set; }
    public int FindingsCount { get; set; }
    public int CriticalFindingsCount { get; set; }
    public int AttachmentsCount { get; set; }
    public bool CanEdit { get; set; }
    public bool CanStart { get; set; }
    public bool CanComplete { get; set; }
    public bool CanCancel { get; set; }
    public bool IsOverdue { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? LastModifiedBy { get; set; }
}