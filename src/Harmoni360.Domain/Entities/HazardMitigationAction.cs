using Harmoni360.Domain.Common;
using Harmoni360.Domain.Events;

namespace Harmoni360.Domain.Entities;

public class HazardMitigationAction : BaseEntity, IAuditableEntity
{
    public int HazardId { get; private set; }
    public Hazard Hazard { get; private set; } = null!;
    public string ActionDescription { get; private set; } = string.Empty;
    public MitigationActionType Type { get; private set; }
    public MitigationActionStatus Status { get; private set; }
    public DateTime TargetDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public int AssignedToId { get; private set; }
    public User AssignedTo { get; private set; } = null!;
    public string? CompletionNotes { get; private set; }
    
    // Priority and effectiveness tracking
    public MitigationPriority Priority { get; private set; }
    public int? EffectivenessRating { get; private set; } // 1-5 scale
    public string? EffectivenessNotes { get; private set; }
    
    // Cost tracking
    public decimal? EstimatedCost { get; private set; }
    public decimal? ActualCost { get; private set; }
    
    // Verification
    public bool RequiresVerification { get; private set; }
    public int? VerifiedById { get; private set; }
    public User? VerifiedBy { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public string? VerificationNotes { get; private set; }

    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    protected HazardMitigationAction() { } // For EF Core

    public static HazardMitigationAction Create(
        int hazardId,
        string actionDescription,
        MitigationActionType type,
        DateTime targetDate,
        int assignedToId,
        MitigationPriority priority = MitigationPriority.Medium,
        decimal? estimatedCost = null,
        bool requiresVerification = false)
    {
        var action = new HazardMitigationAction
        {
            HazardId = hazardId,
            ActionDescription = actionDescription,
            Type = type,
            Status = MitigationActionStatus.Planned,
            TargetDate = targetDate,
            AssignedToId = assignedToId,
            Priority = priority,
            EstimatedCost = estimatedCost,
            RequiresVerification = requiresVerification,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = $"System"
        };

        action.AddDomainEvent(new MitigationActionCreatedEvent(action));

        return action;
    }

    public void UpdateDetails(
        string actionDescription,
        MitigationActionType type,
        DateTime targetDate,
        MitigationPriority priority,
        decimal? estimatedCost,
        string modifiedBy)
    {
        ActionDescription = actionDescription;
        Type = type;
        TargetDate = targetDate;
        Priority = priority;
        EstimatedCost = estimatedCost;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new MitigationActionUpdatedEvent(this));
    }

    public void StartImplementation(string modifiedBy)
    {
        if (Status != MitigationActionStatus.Planned)
        {
            throw new InvalidOperationException($"Cannot start implementation. Current status: {Status}");
        }

        Status = MitigationActionStatus.InProgress;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new MitigationActionStartedEvent(this));
    }

    public void CompleteAction(string completionNotes, decimal? actualCost, string completedBy)
    {
        if (Status != MitigationActionStatus.InProgress)
        {
            throw new InvalidOperationException($"Cannot complete action. Current status: {Status}");
        }

        Status = MitigationActionStatus.Completed;
        CompletedDate = DateTime.UtcNow;
        CompletionNotes = completionNotes;
        ActualCost = actualCost;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = completedBy;

        AddDomainEvent(new MitigationActionCompletedEvent(this));

        // If verification is not required, consider it fully complete
        if (!RequiresVerification)
        {
            AddDomainEvent(new MitigationActionFullyCompletedEvent(this));
        }
    }

    public void Verify(int verifiedById, string verificationNotes, int effectivenessRating)
    {
        if (Status != MitigationActionStatus.Completed)
        {
            throw new InvalidOperationException("Cannot verify incomplete action");
        }

        if (!RequiresVerification)
        {
            throw new InvalidOperationException("This action does not require verification");
        }

        if (effectivenessRating < 1 || effectivenessRating > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(effectivenessRating), "Effectiveness rating must be between 1 and 5");
        }

        VerifiedById = verifiedById;
        VerifiedAt = DateTime.UtcNow;
        VerificationNotes = verificationNotes;
        EffectivenessRating = effectivenessRating;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = $"User_{verifiedById}";

        AddDomainEvent(new MitigationActionVerifiedEvent(this, effectivenessRating));
        AddDomainEvent(new MitigationActionFullyCompletedEvent(this));
    }

    public void MarkOverdue()
    {
        if (Status == MitigationActionStatus.Planned || Status == MitigationActionStatus.InProgress)
        {
            Status = MitigationActionStatus.Overdue;
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = "System";

            AddDomainEvent(new MitigationActionOverdueEvent(this));
        }
    }

    public void Cancel(string reason, string cancelledBy)
    {
        Status = MitigationActionStatus.Cancelled;
        CompletionNotes = $"Cancelled: {reason}";
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = cancelledBy;

        AddDomainEvent(new MitigationActionCancelledEvent(this, reason));
    }

    public void Reassign(int newAssignedToId, string reason, string modifiedBy)
    {
        var previousAssignedToId = AssignedToId;
        AssignedToId = newAssignedToId;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new MitigationActionReassignedEvent(this, previousAssignedToId, newAssignedToId, reason));
    }

    public void ExtendDeadline(DateTime newTargetDate, string reason, string modifiedBy)
    {
        var previousTargetDate = TargetDate;
        TargetDate = newTargetDate;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new MitigationActionDeadlineExtendedEvent(this, previousTargetDate, newTargetDate, reason));
    }

    public bool IsOverdue()
    {
        return (Status == MitigationActionStatus.Planned || Status == MitigationActionStatus.InProgress) 
               && DateTime.UtcNow > TargetDate;
    }

    public bool IsFullyComplete()
    {
        return Status == MitigationActionStatus.Completed && 
               (!RequiresVerification || VerifiedAt.HasValue);
    }

    public TimeSpan GetTimeToTarget()
    {
        return TargetDate - DateTime.UtcNow;
    }
}

public enum MitigationActionType
{
    Elimination = 1,        // Remove the hazard completely
    Substitution = 2,       // Replace with something safer
    Engineering = 3,        // Engineering controls (guards, ventilation, etc.)
    Administrative = 4,     // Policies, procedures, training
    PPE = 5                // Personal Protective Equipment
}

public enum MitigationActionStatus
{
    Planned = 1,
    InProgress = 2,
    Completed = 3,
    Overdue = 4,
    Cancelled = 5
}

public enum MitigationPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}