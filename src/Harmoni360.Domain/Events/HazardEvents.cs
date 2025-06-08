using Harmoni360.Domain.Common;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Domain.Events;

// Hazard Events
public record HazardIdentifiedEvent(Hazard Hazard) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record HazardUpdatedEvent(Hazard Hazard) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record HighSeverityHazardIdentifiedEvent(Hazard Hazard) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record HazardStatusChangedEvent(
    Hazard Hazard,
    HazardStatus PreviousStatus,
    HazardStatus NewStatus) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record HazardSeverityChangedEvent(
    Hazard Hazard,
    HazardSeverity PreviousSeverity,
    HazardSeverity NewSeverity) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record HazardResolvedEvent(Hazard Hazard) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record HazardClosedEvent(Hazard Hazard, string ClosureNotes) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record HazardReassessmentScheduledEvent(
    Hazard Hazard,
    DateTime NextAssessmentDate,
    string Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

// Risk Assessment Events
public record RiskAssessmentCreatedEvent(RiskAssessment RiskAssessment) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record RiskAssessmentUpdatedEvent(
    RiskAssessment RiskAssessment,
    RiskLevel PreviousRiskLevel,
    RiskLevel NewRiskLevel) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record HighRiskAssessmentCreatedEvent(RiskAssessment RiskAssessment) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record RiskAssessmentApprovedEvent(RiskAssessment RiskAssessment) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record RiskAssessmentCompletedEvent(
    Hazard Hazard,
    RiskAssessment RiskAssessment) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record RiskAssessmentDeactivatedEvent(
    RiskAssessment RiskAssessment,
    string Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

// Mitigation Action Events
public record MitigationActionCreatedEvent(HazardMitigationAction Action) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record MitigationActionAddedEvent(
    Hazard Hazard,
    HazardMitigationAction Action) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record MitigationActionUpdatedEvent(HazardMitigationAction Action) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record MitigationActionStartedEvent(HazardMitigationAction Action) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record MitigationActionCompletedEvent(HazardMitigationAction Action) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record MitigationActionVerifiedEvent(
    HazardMitigationAction Action,
    int EffectivenessRating) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record MitigationActionFullyCompletedEvent(HazardMitigationAction Action) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record MitigationActionOverdueEvent(HazardMitigationAction Action) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record MitigationActionCancelledEvent(
    HazardMitigationAction Action,
    string Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record MitigationActionReassignedEvent(
    HazardMitigationAction Action,
    int PreviousAssignedToId,
    int NewAssignedToId,
    string Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record MitigationActionDeadlineExtendedEvent(
    HazardMitigationAction Action,
    DateTime PreviousTargetDate,
    DateTime NewTargetDate,
    string Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}