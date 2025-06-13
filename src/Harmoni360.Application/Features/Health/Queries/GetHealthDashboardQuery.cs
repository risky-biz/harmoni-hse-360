using MediatR;
using Harmoni360.Domain.Entities;
using Harmoni360.Application.Features.Health.DTOs;

namespace Harmoni360.Application.Features.Health.Queries;

public record GetHealthDashboardQuery : IRequest<HealthDashboardDto>
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? Department { get; init; }
    public PersonType? PersonType { get; init; }
    public bool IncludeInactive { get; init; } = false;
}