using MediatR;
using HarmoniHSE360.Domain.Entities;
using HarmoniHSE360.Application.Features.Hazards.DTOs;

namespace HarmoniHSE360.Application.Features.Hazards.Commands;

public record CreateMitigationActionCommand : IRequest<HazardMitigationActionDto>
{
    public int HazardId { get; init; }
    public string ActionDescription { get; init; } = string.Empty;
    public MitigationActionType Type { get; init; }
    public MitigationPriority Priority { get; init; }
    public DateTime TargetDate { get; init; }
    public int AssignedToId { get; init; }
    public decimal? EstimatedCost { get; init; }
    public bool RequiresVerification { get; init; } = true;
    public string? Notes { get; init; }
}