namespace Harmoni360.Application.Features.RiskAssessments.DTOs;

public class RiskAssessmentDto
{
    public int Id { get; set; }
    public int HazardId { get; set; }
    public string HazardTitle { get; set; } = string.Empty;
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

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}

public class RiskAssessmentDetailDto : RiskAssessmentDto
{
    // Additional details for expanded views
    public HazardSummaryDto Hazard { get; set; } = new();
    public UserSummaryDto Assessor { get; set; } = new();
    public UserSummaryDto? ApprovedBy { get; set; }
}

public class HazardSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime IdentifiedDate { get; set; }
    public string ReporterName { get; set; } = string.Empty;
}

public class UserSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Position { get; set; }
}