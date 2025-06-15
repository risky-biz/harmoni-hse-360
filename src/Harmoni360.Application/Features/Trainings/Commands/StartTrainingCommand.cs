using MediatR;
using Harmoni360.Application.Features.Trainings.DTOs;

namespace Harmoni360.Application.Features.Trainings.Commands;

public record StartTrainingCommand : IRequest<TrainingDto>
{
    public int Id { get; init; }
    public DateTime? ActualStartDate { get; init; }
    public string StartNotes { get; init; } = string.Empty;
}