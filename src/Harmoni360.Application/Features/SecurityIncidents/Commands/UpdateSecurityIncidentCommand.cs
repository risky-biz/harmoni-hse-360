using Harmoni360.Application.Features.SecurityIncidents.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;

namespace Harmoni360.Application.Features.SecurityIncidents.Commands;

public record UpdateSecurityIncidentCommand : IRequest<SecurityIncidentDto>
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public SecuritySeverity Severity { get; init; }
    public SecurityIncidentStatus Status { get; init; }
    public DateTime IncidentDateTime { get; init; }
    public string Location { get; init; } = string.Empty;
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    
    // Security-specific fields
    public ThreatActorType? ThreatActorType { get; init; }
    public string? ThreatActorDescription { get; init; }
    public bool IsInternalThreat { get; init; }
    public bool DataBreachOccurred { get; init; }
    public int? AffectedPersonsCount { get; init; }
    public decimal? EstimatedLoss { get; init; }
    public SecurityImpact Impact { get; init; }
    
    // Response information
    public string? ContainmentActions { get; init; }
    public string? RootCause { get; init; }
    public DateTime? DetectionDateTime { get; init; }
    public DateTime? ContainmentDateTime { get; init; }
    public DateTime? ResolutionDateTime { get; init; }
    
    // Assignments
    public int? AssignedToId { get; init; }
    public int? InvestigatorId { get; init; }
}