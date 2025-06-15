using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Inspections.DTOs;

public class InspectionFindingDto
{
    public int Id { get; set; }
    public int InspectionId { get; set; }
    public string FindingNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public FindingType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public FindingSeverity Severity { get; set; }
    public string SeverityName { get; set; } = string.Empty;
    public RiskLevel RiskLevel { get; set; }
    public string RiskLevelName { get; set; } = string.Empty;
    public string? RootCause { get; set; }
    public string? ImmediateAction { get; set; }
    public string? CorrectiveAction { get; set; }
    public DateTime? DueDate { get; set; }
    public int? ResponsiblePersonId { get; set; }
    public string ResponsiblePersonName { get; set; } = string.Empty;
    public FindingStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Equipment { get; set; }
    public string? Regulation { get; set; }
    public DateTime? ClosedDate { get; set; }
    public string? ClosureNotes { get; set; }
    public bool IsOverdue { get; set; }
    public bool CanEdit { get; set; }
    public bool CanClose { get; set; }
    public bool HasCorrectiveAction { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? LastModifiedBy { get; set; }

    public List<FindingAttachmentDto> Attachments { get; set; } = new();
}