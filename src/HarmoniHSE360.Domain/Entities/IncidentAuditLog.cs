using HarmoniHSE360.Domain.Common;

namespace HarmoniHSE360.Domain.Entities;

public class IncidentAuditLog : BaseEntity
{
    public int IncidentId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string FieldName { get; private set; } = string.Empty;
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }
    public string ChangedBy { get; private set; } = string.Empty;
    public DateTime ChangedAt { get; private set; }
    public string? ChangeDescription { get; private set; }

    // Navigation property
    public virtual Incident Incident { get; private set; } = null!;

    private IncidentAuditLog() { } // For EF Core

    public static IncidentAuditLog Create(
        int incidentId,
        string action,
        string fieldName,
        string? oldValue,
        string? newValue,
        string changedBy,
        string? changeDescription = null)
    {
        return new IncidentAuditLog
        {
            IncidentId = incidentId,
            Action = action,
            FieldName = fieldName,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedBy = changedBy,
            ChangedAt = DateTime.UtcNow,
            ChangeDescription = changeDescription
        };
    }

    public static IncidentAuditLog CreateAction(
        int incidentId,
        string action,
        string changedBy,
        string? changeDescription = null)
    {
        return new IncidentAuditLog
        {
            IncidentId = incidentId,
            Action = action,
            FieldName = "System",
            OldValue = null,
            NewValue = null,
            ChangedBy = changedBy,
            ChangedAt = DateTime.UtcNow,
            ChangeDescription = changeDescription
        };
    }
}

public static class AuditActions
{
    public const string Created = "Created";
    public const string Updated = "Updated";
    public const string StatusChanged = "Status Changed";
    public const string SeverityChanged = "Severity Changed";
    public const string AssigneeChanged = "Assignee Changed";
    public const string AttachmentAdded = "Attachment Added";
    public const string AttachmentRemoved = "Attachment Removed";
    public const string CorrectiveActionAdded = "Corrective Action Added";
    public const string CorrectiveActionUpdated = "Corrective Action Updated";
    public const string CorrectiveActionRemoved = "Corrective Action Removed";
    public const string InvolvedPersonAdded = "Involved Person Added";
    public const string InvolvedPersonRemoved = "Involved Person Removed";
    public const string CommentAdded = "Comment Added";
    public const string Viewed = "Viewed";
    public const string Exported = "Exported";
}