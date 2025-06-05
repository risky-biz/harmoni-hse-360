using HarmoniHSE360.Domain.Common;
using HarmoniHSE360.Domain.Entities;

namespace HarmoniHSE360.Domain.Events;

// PPE Item Events
public record PPEItemCreatedEvent(PPEItem PPEItem) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PPEItemUpdatedEvent(PPEItem PPEItem) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PPEConditionChangedEvent(
    PPEItem PPEItem,
    PPECondition PreviousCondition,
    PPECondition NewCondition) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PPEItemAssignedEvent(
    PPEItem PPEItem,
    int AssignedToId,
    string AssignedBy) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PPEItemReturnedEvent(
    PPEItem PPEItem,
    int? PreviousAssignedToId,
    string ReturnedBy) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PPEItemLostEvent(
    PPEItem PPEItem,
    string ReportedBy) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PPEItemRetiredEvent(
    PPEItem PPEItem,
    string RetiredBy) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PPECertificationUpdatedEvent(PPEItem PPEItem) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PPEMaintenancePerformedEvent(
    PPEItem PPEItem,
    string PerformedBy,
    DateTime MaintenanceDate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PPEInspectionRecordedEvent(
    PPEItem PPEItem,
    PPEInspection Inspection) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

// PPE Request Events
public record PPERequestCreatedEvent(PPERequest Request) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PPERequestSubmittedEvent(PPERequest Request) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PPERequestReviewerAssignedEvent(
    PPERequest Request,
    int ReviewerId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PPERequestApprovedEvent(PPERequest Request) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PPERequestRejectedEvent(
    PPERequest Request,
    string RejectionReason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PPERequestFulfilledEvent(
    PPERequest Request,
    int PPEItemId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PPERequestCancelledEvent(
    PPERequest Request,
    string? CancelReason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PPERequestPriorityChangedEvent(
    PPERequest Request,
    RequestPriority OldPriority,
    RequestPriority NewPriority) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

// PPE Compliance Events
public record PPEComplianceViolationDetectedEvent(
    int UserId,
    int CategoryId,
    string ViolationType,
    string Details) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PPEExpiryWarningEvent(
    PPEItem PPEItem,
    int DaysUntilExpiry) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PPEMaintenanceDueEvent(
    PPEItem PPEItem,
    DateTime DueDate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PPEInspectionDueEvent(
    PPEItem PPEItem,
    DateTime DueDate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PPECertificationExpiryWarningEvent(
    PPEItem PPEItem,
    int DaysUntilExpiry) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}