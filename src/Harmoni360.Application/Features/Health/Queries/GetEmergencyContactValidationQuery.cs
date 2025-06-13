using MediatR;
using Harmoni360.Domain.Entities;
using Harmoni360.Application.Features.Health.DTOs;

namespace Harmoni360.Application.Features.Health.Queries;

public record GetEmergencyContactValidationQuery : IRequest<EmergencyContactValidationDto>
{
    public string? Department { get; init; }
    public PersonType? PersonType { get; init; }
    public bool IncludeInactive { get; init; } = false;
    public ValidationLevel Level { get; init; } = ValidationLevel.Standard;
}

public enum ValidationLevel
{
    Basic,      // Just check if contacts exist
    Standard,   // Check completeness and validity
    Comprehensive // Full validation including reachability patterns
}