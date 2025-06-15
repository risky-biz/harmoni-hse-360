using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Inspections.DTOs;

public class InspectionItemDto
{
    public int Id { get; set; }
    public int InspectionId { get; set; }
    public int? ChecklistItemId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string? Description { get; set; }
    public InspectionItemType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string? Response { get; set; }
    public InspectionItemStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int SortOrder { get; set; }
    public string? ExpectedValue { get; set; }
    public string? Unit { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public List<string> Options { get; set; } = new();
    public bool IsCompliant { get; set; }
    public bool IsCompleted { get; set; }
    public bool HasResponse { get; set; }
}