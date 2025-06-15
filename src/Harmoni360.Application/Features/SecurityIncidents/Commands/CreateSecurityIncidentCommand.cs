using Harmoni360.Application.Features.SecurityIncidents.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Harmoni360.Application.Features.SecurityIncidents.Commands;

public record CreateSecurityIncidentCommand : IRequest<SecurityIncidentDto>
{
    public SecurityIncidentType IncidentType { get; init; }
    public SecurityIncidentCategory Category { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public SecuritySeverity Severity { get; init; }
    public DateTime IncidentDateTime { get; init; }
    public string Location { get; init; } = string.Empty;
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    
    // Security-specific fields
    public ThreatActorType? ThreatActorType { get; init; }
    public string? ThreatActorDescription { get; init; }
    public bool IsInternalThreat { get; init; }
    public bool DataBreachSuspected { get; init; }
    public int? AffectedPersonsCount { get; init; }
    public decimal? EstimatedLoss { get; init; }
    
    // Optional assignments
    public int? AssignedToId { get; init; }
    public int? InvestigatorId { get; init; }
    
    // Collections
    public List<IFormFile>? Attachments { get; init; }
    public List<int>? InvolvedPersonIds { get; init; }
    public List<string>? CompromisedAssets { get; init; }
    public List<string>? ThreatIndicators { get; init; }
    
    // Additional context
    public string? ContainmentActions { get; init; }
    public DateTime? DetectionDateTime { get; init; }
    public SecurityImpact Impact { get; init; } = SecurityImpact.None;
}