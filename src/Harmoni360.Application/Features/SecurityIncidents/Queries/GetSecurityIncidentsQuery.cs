using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.SecurityIncidents.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;

namespace Harmoni360.Application.Features.SecurityIncidents.Queries;

public record GetSecurityIncidentsQuery : IRequest<PagedList<SecurityIncidentListDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public SecurityIncidentType? Type { get; init; }
    public SecurityIncidentCategory? Category { get; init; }
    public SecuritySeverity? MinSeverity { get; init; }
    public SecurityIncidentStatus? Status { get; init; }
    public ThreatLevel? MinThreatLevel { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Location { get; init; }
    public string? SearchTerm { get; init; }
    public bool? IsDataBreach { get; init; }
    public bool? IsInternalThreat { get; init; }
    public int? ReporterId { get; init; }
    public int? AssignedToId { get; init; }
    public int? InvestigatorId { get; init; }
    public bool OnlyMyIncidents { get; init; }
    public bool OnlyOpenIncidents { get; init; }
    public bool OnlyOverdueIncidents { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = true;
}