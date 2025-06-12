using MediatR;
using Harmoni360.Application.Features.RiskAssessments.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Harmoni360.Application.Features.RiskAssessments.Commands;

public record UpdateRiskAssessmentCommand : IRequest<RiskAssessmentDto>
{
    [Required]
    public int Id { get; init; }
    
    [Required]
    public int HazardId { get; init; }
    
    [Required]
    [MaxLength(50)]
    public string Type { get; init; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string AssessorName { get; init; } = string.Empty;
    
    [Required]
    public DateTime AssessmentDate { get; init; }
    
    [Required]
    [Range(1, 5)]
    public int ProbabilityScore { get; init; }
    
    [Required]
    [Range(1, 5)]
    public int SeverityScore { get; init; }
    
    [Required]
    [MaxLength(2000)]
    public string PotentialConsequences { get; init; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string ExistingControls { get; init; } = string.Empty;
    
    [MaxLength(2000)]
    public string? RecommendedActions { get; init; }
    
    [MaxLength(1000)]
    public string? AdditionalNotes { get; init; }
    
    [Required]
    public DateTime NextReviewDate { get; init; }
}