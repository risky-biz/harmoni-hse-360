using MediatR;
using Harmoni360.Application.Features.Hazards.DTOs;

namespace Harmoni360.Application.Features.Hazards.Queries;

public record GetHazardByIdQuery : IRequest<HazardDetailDto?>
{
    public int Id { get; init; }
    
    // Include options for optimizing queries
    public bool IncludeAttachments { get; init; } = true;
    public bool IncludeRiskAssessments { get; init; } = true;
    public bool IncludeMitigationActions { get; init; } = true;
    public bool IncludeReassessments { get; init; } = true;
    public bool IncludeAuditTrail { get; init; } = false;
    public bool IncludeRelatedIncidents { get; init; } = false;
}