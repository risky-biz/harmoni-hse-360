using MediatR;
using Harmoni360.Application.Features.Audits.DTOs;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Audits.Commands;

public record CreateAuditCommand : IRequest<AuditDto>
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public AuditType Type { get; init; }
    public AuditCategory Category { get; init; }
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
    
    // Audit Items
    public List<CreateAuditItemCommand> Items { get; init; } = new();
}

public record CreateAuditItemCommand
{
    public string Description { get; init; } = string.Empty;
    public AuditItemType Type { get; init; }
    public bool IsRequired { get; init; } = true;
    public string? Category { get; init; }
    public int SortOrder { get; init; }
    public string? ExpectedResult { get; init; }
    public int? MaxPoints { get; init; }
    public string? ValidationCriteria { get; init; }
    public string? AcceptanceCriteria { get; init; }
}