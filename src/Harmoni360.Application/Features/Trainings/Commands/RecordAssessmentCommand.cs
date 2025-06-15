using MediatR;
using Harmoni360.Application.Features.Trainings.DTOs;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Trainings.Commands;

public record RecordAssessmentCommand : IRequest<TrainingParticipantDto>
{
    public int TrainingId { get; init; }
    public int ParticipantId { get; init; }
    public decimal Score { get; init; }
    public decimal MaxScore { get; init; } = 100.0m;
    public AssessmentMethod AssessmentMethod { get; init; }
    public DateTime AssessmentDate { get; init; }
    public string AssessmentNotes { get; init; } = string.Empty;
    public string AssessedBy { get; init; } = string.Empty;
    public bool IsPassed { get; init; }
}