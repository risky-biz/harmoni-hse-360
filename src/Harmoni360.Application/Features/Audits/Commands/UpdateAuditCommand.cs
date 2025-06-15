using MediatR;
using Harmoni360.Application.Features.Audits.DTOs;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Audits.Commands;

public record UpdateAuditCommand : IRequest<AuditDto>
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public AuditPriority Priority { get; init; }
    public DateTime ScheduledDate { get; init; }
    public int? LocationId { get; init; }
    public int? DepartmentId { get; init; }
    public int? FacilityId { get; init; }
    public int? EstimatedDurationMinutes { get; init; }
    
    // Compliance & Standards
    public string? StandardsApplied { get; init; }
    public bool IsRegulatory { get; init; }
    public string? RegulatoryReference { get; init; }
}