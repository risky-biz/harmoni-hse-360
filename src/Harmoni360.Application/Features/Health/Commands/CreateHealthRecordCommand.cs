using MediatR;
using Harmoni360.Domain.Entities;
using Harmoni360.Application.Features.Health.DTOs;

namespace Harmoni360.Application.Features.Health.Commands;

public record CreateHealthRecordCommand : IRequest<HealthRecordDto>
{
    // Existing person ID (if person already exists)
    public int? PersonId { get; init; }
    
    // Person details (for creating new person)
    public string? PersonName { get; init; }
    public string? PersonEmail { get; init; }
    public string? PersonPhoneNumber { get; init; }
    public string? PersonDepartment { get; init; }
    public string? PersonPosition { get; init; }
    public string? PersonEmployeeId { get; init; }
    public PersonType PersonType { get; init; }
    
    // Health record details
    public DateTime? DateOfBirth { get; init; }
    public BloodType? BloodType { get; init; }
    public string? MedicalNotes { get; init; }
}