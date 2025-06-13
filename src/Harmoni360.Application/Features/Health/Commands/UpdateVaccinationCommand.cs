using MediatR;
using Harmoni360.Application.Features.Health.DTOs;

namespace Harmoni360.Application.Features.Health.Commands;

public record UpdateVaccinationCommand : IRequest<VaccinationRecordDto>
{
    public int Id { get; init; }
    public DateTime? DateAdministered { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public string? BatchNumber { get; init; }
    public string? AdministeredBy { get; init; }
    public string? AdministrationLocation { get; init; }
    public string? Notes { get; init; }
    public bool IsRequired { get; init; }
}