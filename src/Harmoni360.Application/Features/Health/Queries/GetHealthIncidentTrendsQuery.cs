using MediatR;
using Harmoni360.Domain.Entities;
using Harmoni360.Application.Features.Health.DTOs;

namespace Harmoni360.Application.Features.Health.Queries;

public record GetHealthIncidentTrendsQuery : IRequest<HealthIncidentTrendsDto>
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? Department { get; init; }
    public PersonType? PersonType { get; init; }
    public HealthIncidentType? IncidentType { get; init; }
    public HealthIncidentSeverity? Severity { get; init; }
    public TrendPeriod Period { get; init; } = TrendPeriod.Monthly;
    public bool IncludeResolved { get; init; } = true;
}

public enum TrendPeriod
{
    Daily,
    Weekly,
    Monthly,
    Quarterly
}