using Harmoni360.Application.Features.SecurityIncidents.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.SecurityIncidents.Queries;

public record GetSecurityDashboardQuery : IRequest<SecurityDashboardDto>
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public bool IncludeThreatIntel { get; init; } = true;
    public bool IncludeTrends { get; init; } = true;
    public bool IncludeMetrics { get; init; } = true;
}