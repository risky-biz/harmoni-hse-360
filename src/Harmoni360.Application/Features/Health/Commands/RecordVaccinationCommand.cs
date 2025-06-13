using MediatR;
using Harmoni360.Application.Features.Health.DTOs;

namespace Harmoni360.Application.Features.Health.Commands;

public record RecordVaccinationCommand : IRequest<VaccinationRecordDto>
{
    public int HealthRecordId { get; init; }
    public string VaccineName { get; init; } = string.Empty;
    public DateTime DateAdministered { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public string? BatchNumber { get; init; }
    public string AdministeredBy { get; init; } = string.Empty;
    public string? AdministrationLocation { get; init; }
    public string? Notes { get; init; }
    public bool IsRequired { get; init; }
}