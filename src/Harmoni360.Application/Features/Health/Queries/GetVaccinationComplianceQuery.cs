using MediatR;
using Harmoni360.Domain.Entities;
using Harmoni360.Application.Features.Health.DTOs;

namespace Harmoni360.Application.Features.Health.Queries;

public record GetVaccinationComplianceQuery : IRequest<VaccinationComplianceDto>
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? Department { get; init; }
    public PersonType? PersonType { get; init; }
    public string? VaccineName { get; init; }
    public bool IncludeInactive { get; init; } = false;
    public bool IncludeExemptions { get; init; } = true;
}