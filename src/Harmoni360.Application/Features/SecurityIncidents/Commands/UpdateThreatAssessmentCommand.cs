using Harmoni360.Application.Features.SecurityIncidents.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;

namespace Harmoni360.Application.Features.SecurityIncidents.Commands;

public record UpdateThreatAssessmentCommand : IRequest<ThreatAssessmentDto>
{
    public int Id { get; init; }
    public ThreatLevel ThreatLevel { get; init; }
    public string AssessmentRationale { get; init; } = string.Empty;
    
    // Risk Factors (1-5 scale)
    public int ThreatCapability { get; init; }
    public int ThreatIntent { get; init; }
    public int TargetVulnerability { get; init; }
    public int ImpactPotential { get; init; }
    
    // Threat Intelligence
    public bool ExternalThreatIntelUsed { get; init; }
    public string? ThreatIntelSource { get; init; }
    public string? ThreatIntelDetails { get; init; }
}