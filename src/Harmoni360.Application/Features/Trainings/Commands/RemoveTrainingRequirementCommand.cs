using MediatR;

namespace Harmoni360.Application.Features.Trainings.Commands;

public record RemoveTrainingRequirementCommand : IRequest
{
    public int TrainingId { get; init; }
    public int RequirementId { get; init; }
}