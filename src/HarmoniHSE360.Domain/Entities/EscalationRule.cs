using HarmoniHSE360.Domain.Common;

namespace HarmoniHSE360.Domain.Entities;

public class EscalationRule : BaseEntity, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    // Trigger conditions
    public List<IncidentSeverity> TriggerSeverities { get; set; } = new();
    public List<IncidentStatus> TriggerStatuses { get; set; } = new();
    public TimeSpan? TriggerAfterDuration { get; set; }
    public List<string> TriggerDepartments { get; set; } = new();
    public List<string> TriggerLocations { get; set; } = new();
    
    // Escalation actions
    public List<EscalationAction> Actions { get; set; } = new();
    
    // Timing and priority
    public int Priority { get; set; } = 100; // Lower number = higher priority
    
    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}

public class EscalationAction
{
    public int Id { get; set; }
    public int EscalationRuleId { get; set; }
    public EscalationActionType Type { get; set; }
    public string Target { get; set; } = string.Empty; // Email, user ID, role, etc.
    public string? TemplateId { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
    public TimeSpan? Delay { get; set; }
    public List<NotificationChannel> Channels { get; set; } = new();
    
    // Navigation property
    public EscalationRule EscalationRule { get; set; } = null!;
}

public class EscalationHistory : BaseEntity, IAuditableEntity
{
    public int IncidentId { get; set; }
    public int? EscalationRuleId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public EscalationActionType ActionType { get; set; }
    public string ActionTarget { get; set; } = string.Empty;
    public string ActionDetails { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public string? ExecutedBy { get; set; }
    
    // Navigation properties
    public Incident Incident { get; set; } = null!;
    public EscalationRule? EscalationRule { get; set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}

public class NotificationHistory : BaseEntity, IAuditableEntity
{
    public int IncidentId { get; set; }
    public string RecipientId { get; set; } = string.Empty;
    public string RecipientType { get; set; } = string.Empty; // User, Role, Email, Phone
    public string TemplateId { get; set; } = string.Empty;
    public NotificationChannel Channel { get; set; }
    public NotificationPriority Priority { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    
    // Navigation properties
    public Incident Incident { get; set; } = null!;
    
    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}

public enum EscalationActionType
{
    NotifyUser = 1,
    NotifyRole = 2,
    NotifyDepartment = 3,
    NotifyExternal = 4,
    ChangeStatus = 5,
    AssignInvestigator = 6,
    CreateTask = 7,
    SendRegulatory = 8,
    EscalateToManager = 9,
    SendEmergencyAlert = 10
}

public enum NotificationStatus
{
    Pending = 1,
    Sent = 2,
    Delivered = 3,
    Failed = 4,
    Read = 5
}