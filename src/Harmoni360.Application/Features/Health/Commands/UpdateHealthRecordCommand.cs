using MediatR;
using Harmoni360.Domain.Entities;
using Harmoni360.Application.Features.Health.DTOs;

namespace Harmoni360.Application.Features.Health.Commands;

public record UpdateHealthRecordCommand : IRequest<HealthRecordDto>
{
    public int Id { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public BloodType? BloodType { get; init; }
    public string? MedicalNotes { get; init; }
}