using MediatR;
using Harmoni360.Application.Features.Trainings.DTOs;

namespace Harmoni360.Application.Features.Trainings.Commands;

public record CompleteTrainingCommand : IRequest<TrainingDto>
{
    public int Id { get; init; }
    public DateTime? ActualEndDate { get; init; }
    public string CompletionNotes { get; init; } = string.Empty;
    public decimal? OverallEffectivenessScore { get; init; }
    public string InstructorFeedback { get; init; } = string.Empty;
}