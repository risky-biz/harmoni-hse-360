namespace Harmoni360.Application.Features.PPE.DTOs;

public class PPEInspectionDto
{
    public int Id { get; set; }
    public int PPEItemId { get; set; }
    public string PPEItemCode { get; set; } = string.Empty;
    public string PPEItemName { get; set; } = string.Empty;
    public int InspectorId { get; set; }
    public string InspectorName { get; set; } = string.Empty;
    public DateTime InspectionDate { get; set; }
    public DateTime? NextInspectionDate { get; set; }
    public string Result { get; set; } = string.Empty;
    public string? Findings { get; set; }
    public string? CorrectiveActions { get; set; }
    public string? RecommendedCondition { get; set; }
    public bool RequiresMaintenance { get; set; }
    public string? MaintenanceNotes { get; set; }
    public List<string> PhotoPaths { get; set; } = new();
    
    // Computed Properties
    public bool IsPassed { get; set; }
    public bool IsFailed { get; set; }
    public bool HasObservations { get; set; }
    public bool IsOverdue { get; set; }
    public int? DaysUntilNextInspection { get; set; }
    
    // Audit Info
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}

public class PPEInspectionSummaryDto
{
    public int Id { get; set; }
    public string PPEItemCode { get; set; } = string.Empty;
    public string PPEItemName { get; set; } = string.Empty;
    public string InspectorName { get; set; } = string.Empty;
    public DateTime InspectionDate { get; set; }
    public DateTime? NextInspectionDate { get; set; }
    public string Result { get; set; } = string.Empty;
    public bool RequiresMaintenance { get; set; }
    public bool IsOverdue { get; set; }
}