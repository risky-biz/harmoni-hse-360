using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Events;

public record InspectionCreatedEvent(int InspectionId, string Title, InspectionType Type) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record InspectionUpdatedEvent(int InspectionId, string Title) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record InspectionScheduledEvent(int InspectionId, string Title, DateTime ScheduledDate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record InspectionStartedEvent(int InspectionId, string Title) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record InspectionCompletedEvent(int InspectionId, string Title, DateTime CompletedDate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record InspectionCancelledEvent(int InspectionId, string Title, string Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record InspectionOverdueEvent(int InspectionId, string Title, DateTime ScheduledDate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record InspectionFindingAddedEvent(int InspectionId, int FindingId, FindingType Type, FindingSeverity Severity) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}