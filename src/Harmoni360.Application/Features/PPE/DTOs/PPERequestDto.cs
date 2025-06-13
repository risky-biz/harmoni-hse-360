namespace Harmoni360.Application.Features.PPE.DTOs;

public class PPERequestDto
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public int RequesterId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public string RequesterEmail { get; set; } = string.Empty;
    public string RequesterDepartment { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Justification { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public int? ReviewerId { get; set; }
    public string? ReviewerName { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? FulfilledDate { get; set; }
    public string? FulfilledBy { get; set; }
    public int? FulfilledPPEItemId { get; set; }
    public string? FulfilledPPEItemCode { get; set; }
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
    public List<PPERequestItemDto> RequestItems { get; set; } = new();
    
    // Computed Properties
    public bool IsOverdue { get; set; }
    public bool IsUrgent { get; set; }
    public int? DaysUntilRequired { get; set; }
    public string ProcessingTimeDisplay { get; set; } = string.Empty;
    
    // Audit Info
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}

public class PPERequestItemDto
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public string ItemDescription { get; set; } = string.Empty;
    public string? Size { get; set; }
    public int Quantity { get; set; }
    public string? SpecialRequirements { get; set; }
}

public class PPERequestSummaryDto
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string RequesterName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public bool IsOverdue { get; set; }
    public bool IsUrgent { get; set; }
    public int ItemCount { get; set; }
}