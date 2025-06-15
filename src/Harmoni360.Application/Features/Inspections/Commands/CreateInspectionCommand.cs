using Harmoni360.Application.Features.Inspections.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;

namespace Harmoni360.Application.Features.Inspections.Commands;

public record CreateInspectionCommand : IRequest<InspectionDto>
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public InspectionType Type { get; init; }
    public InspectionCategory Category { get; init; }
    public InspectionPriority Priority { get; init; }
    public DateTime ScheduledDate { get; init; }
    public int InspectorId { get; init; }
    public int? LocationId { get; init; }
    public int? DepartmentId { get; init; }
    public int? FacilityId { get; init; }
    public int? EstimatedDurationMinutes { get; init; }
    public List<CreateInspectionItemCommand> Items { get; init; } = new();
}

public record CreateInspectionItemCommand
{
    public int? ChecklistItemId { get; init; }
    public string Question { get; init; } = string.Empty;
    public string? Description { get; init; }
    public InspectionItemType Type { get; init; }
    public bool IsRequired { get; init; }
    public int SortOrder { get; init; }
    public string? ExpectedValue { get; init; }
    public string? Unit { get; init; }
    public decimal? MinValue { get; init; }
    public decimal? MaxValue { get; init; }
    public List<string> Options { get; init; } = new();
}