using HarmoniHSE360.Domain.Common;

namespace HarmoniHSE360.Domain.Entities;

public class CorrectiveAction : BaseEntity, IAuditableEntity
{
    public int IncidentId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public int AssignedToId { get; private set; }
    public User AssignedTo { get; private set; } = null!;
    public DateTime DueDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public ActionStatus Status { get; private set; }
    public string? CompletionNotes { get; private set; }

    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    protected CorrectiveAction() { } // For EF Core

    public static CorrectiveAction Create(
        int incidentId,
        string description,
        int assignedToId,
        DateTime dueDate,
        string createdBy)
    {
        return new CorrectiveAction
        {
            IncidentId = incidentId,
            Description = description,
            AssignedToId = assignedToId,
            DueDate = dueDate,
            Status = ActionStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public void StartProgress(string modifiedBy)
    {
        Status = ActionStatus.InProgress;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void Complete(string completionNotes, string modifiedBy)
    {
        Status = ActionStatus.Completed;
        CompletedDate = DateTime.UtcNow;
        CompletionNotes = completionNotes;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void UpdateDueDate(DateTime newDueDate, string modifiedBy)
    {
        DueDate = newDueDate;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        if (DateTime.UtcNow > DueDate && Status != ActionStatus.Completed)
        {
            Status = ActionStatus.Overdue;
        }
    }
}