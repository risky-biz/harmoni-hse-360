using MediatR;
using HarmoniHSE360.Domain.Entities;
using HarmoniHSE360.Application.Features.Hazards.DTOs;

namespace HarmoniHSE360.Application.Features.Hazards.Commands;

public record CreateRiskAssessmentCommand : IRequest<RiskAssessmentDto>
{
    public int HazardId { get; init; }
    public RiskAssessmentType Type { get; init; }
    public int AssessorId { get; init; }
    
    // Risk scoring
    public int ProbabilityScore { get; init; } // 1-5
    public int SeverityScore { get; init; } // 1-5
    
    // Assessment details
    public string PotentialConsequences { get; init; } = string.Empty;
    public string ExistingControls { get; init; } = string.Empty;
    public string RecommendedActions { get; init; } = string.Empty;
    public string AdditionalNotes { get; init; } = string.Empty;
    
    // Review cycle
    public DateTime NextReviewDate { get; init; }
    
    // Set as current assessment for the hazard
    public bool SetAsCurrent { get; init; } = true;
}