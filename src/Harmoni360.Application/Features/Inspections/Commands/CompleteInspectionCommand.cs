using Harmoni360.Domain.Enums;
using MediatR;

namespace Harmoni360.Application.Features.Inspections.Commands;

public record CompleteInspectionCommand : IRequest<Unit>
{
    public int InspectionId { get; init; }
    public string? Summary { get; init; }
    public string? Recommendations { get; init; }
    public List<CreateInspectionFindingCommand> Findings { get; init; } = new();
}

public record CreateInspectionFindingCommand
{
    public string Description { get; init; } = string.Empty;
    public FindingType Type { get; init; }
    public FindingSeverity Severity { get; init; }
    public string? Location { get; init; }
    public string? Equipment { get; init; }
    public string? RootCause { get; init; }
    public string? ImmediateAction { get; init; }
    public string? CorrectiveAction { get; init; }
    public DateTime? DueDate { get; init; }
    public int? ResponsiblePersonId { get; init; }
    public string? Regulation { get; init; }
}