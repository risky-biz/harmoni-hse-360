using MediatR;
using Harmoni360.Domain.Entities;
using Harmoni360.Application.Features.Health.DTOs;

namespace Harmoni360.Application.Features.Health.Commands;

public record UpdateMedicalConditionCommand : IRequest<MedicalConditionDto>
{
    public int Id { get; init; }
    public ConditionType Type { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ConditionSeverity Severity { get; init; }
    public string? TreatmentPlan { get; init; }
    public DateTime? DiagnosedDate { get; init; }
    public bool RequiresEmergencyAction { get; init; }
    public string? EmergencyInstructions { get; init; }
}