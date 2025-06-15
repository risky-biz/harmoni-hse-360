using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Events;

public record AuditCreatedEvent(int AuditId, string Title, AuditType Type) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record AuditUpdatedEvent(int AuditId, string Title) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record AuditScheduledEvent(int AuditId, string Title, DateTime ScheduledDate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record AuditStartedEvent(int AuditId, string Title) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record AuditCompletedEvent(int AuditId, string Title, DateTime CompletedDate, AuditScore? Score) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record AuditCancelledEvent(int AuditId, string Title, string Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record AuditSubmittedForReviewEvent(int AuditId, string Title) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record AuditOverdueEvent(int AuditId, string Title, DateTime ScheduledDate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record AuditFindingAddedEvent(int AuditId, int FindingId, FindingType Type, FindingSeverity Severity) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record AuditFindingResolvedEvent(int AuditId, int FindingId, string ResolvedBy) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record AuditFindingClosedEvent(int AuditId, int FindingId, string ClosedBy) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}