using MediatR;
using Harmoni360.Application.Features.Trainings.DTOs;

namespace Harmoni360.Application.Features.Trainings.Commands;

public record CancelTrainingCommand : IRequest<TrainingDto>
{
    public int Id { get; init; }
    public string CancellationReason { get; init; } = string.Empty;
    public DateTime CancellationDate { get; init; } = DateTime.UtcNow;
    public string CancelledBy { get; init; } = string.Empty;
}