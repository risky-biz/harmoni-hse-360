using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities;

public class CorrectiveAction : BaseEntity, IAuditableEntity
{
    public int IncidentId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string AssignedToDepartment { get; private set; } = string.Empty;
    public int? AssignedToId { get; private set; }
    public User? AssignedTo { get; private set; }
    
    // Configuration entity reference
    public int? DepartmentId { get; private set; }
    public Department? Department { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public ActionStatus Status { get; private set; }
    public ActionPriority Priority { get; private set; }
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
        string assignedToDepartment,
        DateTime dueDate,
        ActionPriority priority,
        string createdBy,
        int? assignedToId = null,
        int? departmentId = null)
    {
        return new CorrectiveAction
        {
            IncidentId = incidentId,
            Description = description,
            AssignedToDepartment = assignedToDepartment,
            AssignedToId = assignedToId,
            DepartmentId = departmentId,
            DueDate = dueDate,
            Priority = priority,
            Status = ActionStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public void MarkAsCompleted(DateTime completedDate, string completionNotes)
    {
        CompletedDate = completedDate;
        CompletionNotes = completionNotes;
        Status = ActionStatus.Completed;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = "System";
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

    public void Update(
        string description,
        string assignedToDepartment,
        DateTime dueDate,
        ActionPriority priority,
        string modifiedBy,
        int? assignedToId = null,
        int? departmentId = null)
    {
        Description = description;
        AssignedToDepartment = assignedToDepartment;
        AssignedToId = assignedToId;
        DepartmentId = departmentId;
        DueDate = dueDate;
        Priority = priority;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        // Check if overdue
        if (DateTime.UtcNow > DueDate && Status != ActionStatus.Completed)
        {
            Status = ActionStatus.Overdue;
        }
    }
}

public enum ActionPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}