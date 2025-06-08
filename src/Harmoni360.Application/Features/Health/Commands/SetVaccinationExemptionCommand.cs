using MediatR;
using Harmoni360.Application.Features.Health.DTOs;

namespace Harmoni360.Application.Features.Health.Commands;

public record SetVaccinationExemptionCommand : IRequest<VaccinationRecordDto>
{
    public int Id { get; init; }
    public string ExemptionReason { get; init; } = string.Empty;
    public bool RemoveExemption { get; init; } = false;
}