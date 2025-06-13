using Harmoni360.Domain.Common;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Domain.Events;

public record IncidentCreatedEvent(Incident Incident) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record IncidentUpdatedEvent(Incident Incident) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record SeriousIncidentReportedEvent(Incident Incident) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record InvestigatorAssignedEvent(Incident Incident, int InvestigatorId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record CorrectiveActionAddedEvent(Incident Incident, CorrectiveAction Action) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record IncidentClosedEvent(Incident Incident, string ClosureNotes) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}