using MediatR;
using Harmoni360.Application.Features.Trainings.DTOs;

namespace Harmoni360.Application.Features.Trainings.Commands;

public record UpdateTrainingRequirementCommand : IRequest<TrainingRequirementDto>
{
    public int TrainingId { get; init; }
    public int RequirementId { get; init; }
    public string RequirementDescription { get; init; } = string.Empty;
    public bool IsMandatory { get; init; } = true;
    public DateTime? DueDate { get; init; }
    public string CompetencyLevel { get; init; } = string.Empty;
    public string VerificationMethod { get; init; } = string.Empty;
    public bool DocumentationRequired { get; init; }
    public string RegulatoryReference { get; init; } = string.Empty;
    public bool IsK3Requirement { get; init; }
}