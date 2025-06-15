using MediatR;

namespace Harmoni360.Application.Features.Trainings.Commands;

public record DeleteTrainingCommand : IRequest
{
    public int Id { get; init; }
}