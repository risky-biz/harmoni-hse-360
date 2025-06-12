using Harmoni360.Domain.Common;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Events;

// Work Permit Lifecycle Events
public record WorkPermitCreatedEvent(int WorkPermitId, string PermitNumber, WorkPermitType Type) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record WorkPermitUpdatedEvent(int WorkPermitId, string PermitNumber) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record WorkPermitSubmittedEvent(int WorkPermitId, string PermitNumber, string SubmittedBy) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record WorkPermitApprovedEvent(int WorkPermitId, string PermitNumber, string ApprovedBy) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record WorkPermitRejectedEvent(int WorkPermitId, string PermitNumber, string RejectedBy, string Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record WorkPermitStartedEvent(int WorkPermitId, string PermitNumber, string StartedBy) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record WorkPermitCompletedEvent(int WorkPermitId, string PermitNumber, string CompletedBy, bool IsCompletedSafely) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record WorkPermitCancelledEvent(int WorkPermitId, string PermitNumber, string CancelledBy, string Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record WorkPermitExpiredEvent(int WorkPermitId, string PermitNumber) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

// Work Permit Component Events
public record WorkPermitHazardAddedEvent(int WorkPermitId, int HazardId, RiskLevel RiskLevel) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record WorkPermitPrecautionAddedEvent(int WorkPermitId, int PrecautionId, bool IsRequired) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record WorkPermitPrecautionCompletedEvent(int WorkPermitId, int PrecautionId, string CompletedBy) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record WorkPermitAttachmentAddedEvent(int WorkPermitId, int AttachmentId, string FileName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

// Work Permit Approval Workflow Events
public record WorkPermitApprovalRequestedEvent(int WorkPermitId, string PermitNumber, string ApprovalLevel, Guid RequestedFromUserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record WorkPermitApprovalCompletedEvent(int WorkPermitId, string PermitNumber, string ApprovalLevel, bool IsApproved) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

// Work Permit Safety Events
public record WorkPermitRiskLevelChangedEvent(int WorkPermitId, RiskLevel OldRiskLevel, RiskLevel NewRiskLevel) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record WorkPermitSafetyViolationEvent(int WorkPermitId, string ViolationDescription, string ReportedBy) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record WorkPermitEmergencyEvent(int WorkPermitId, string EmergencyType, string Description) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

// Work Permit Compliance Events
public record WorkPermitK3ComplianceUpdatedEvent(int WorkPermitId, bool IsCompliant, string ComplianceNotes) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record WorkPermitEnvironmentalComplianceUpdatedEvent(int WorkPermitId, bool IsCompliant, string PermitNumber) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}