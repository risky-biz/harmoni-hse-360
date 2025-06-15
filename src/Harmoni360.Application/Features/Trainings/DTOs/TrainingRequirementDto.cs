namespace Harmoni360.Application.Features.Trainings.DTOs;

public class TrainingRequirementDto
{
    public int Id { get; set; }
    public int TrainingId { get; set; }
    public string RequirementDescription { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
    public string? CompletionNotes { get; set; }
    
    // Competency Details
    public string CompetencyLevel { get; set; } = string.Empty;
    public string VerificationMethod { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedBy { get; set; }
    public string? VerificationNotes { get; set; }
    
    // Supporting Documentation
    public string? SupportingDocuments { get; set; }
    public string? EvidenceDescription { get; set; }
    
    // Assessment Details
    public string? AssessmentCriteria { get; set; }
    public decimal? RequiredScore { get; set; }
    public decimal? ActualScore { get; set; }
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    
    // Computed Properties
    public bool IsOverdue => DueDate.HasValue && DueDate < DateTime.Now && Status != "Completed";
    public bool CanComplete => Status == "Pending" || Status == "InProgress";
    public bool CanVerify => Status == "Completed" && !IsVerified;
    public string StatusBadgeColor => Status switch
    {
        "Pending" => "secondary",
        "InProgress" => "warning",
        "Completed" => "success",
        "Overdue" => "danger",
        "Waived" => "info",
        "NotApplicable" => "light",
        _ => "secondary"
    };
    public int DaysUntilDue => DueDate.HasValue ? (int)(DueDate.Value - DateTime.Now).TotalDays : 0;
}