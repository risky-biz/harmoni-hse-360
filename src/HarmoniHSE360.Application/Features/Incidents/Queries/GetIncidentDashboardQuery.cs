using HarmoniHSE360.Application.Features.Incidents.DTOs;
using MediatR;

namespace HarmoniHSE360.Application.Features.Incidents.Queries;

public record GetIncidentDashboardQuery : IRequest<IncidentDashboardDto>
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? Department { get; init; }
    public bool IncludeResolved { get; init; } = true;
}