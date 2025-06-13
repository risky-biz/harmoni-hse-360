using MediatR;
using Harmoni360.Domain.Entities;
using Harmoni360.Application.Features.Health.DTOs;

namespace Harmoni360.Application.Features.Health.Commands;

public record CreateHealthRecordCommand : IRequest<HealthRecordDto>
{
    public int PersonId { get; init; }
    public PersonType PersonType { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public BloodType? BloodType { get; init; }
    public string? MedicalNotes { get; init; }
}