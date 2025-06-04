using HarmoniHSE360.Domain.Common;

namespace HarmoniHSE360.Application.Common.Interfaces;

public interface INotificationTemplateService
{
    Task<NotificationTemplate?> GetTemplateAsync(string templateId, string language = "en", CancellationToken cancellationToken = default);
    Task<string> RenderTemplateAsync(string templateId, Dictionary<string, object> data, string language = "en", CancellationToken cancellationToken = default);
    Task<NotificationContent> GenerateNotificationAsync(NotificationRequest request, CancellationToken cancellationToken = default);
}

public class NotificationTemplate
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public NotificationTemplateType Type { get; set; }
    public string SubjectTemplate { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;
    public string? HtmlBodyTemplate { get; set; }
    public string? SmsTemplate { get; set; }
    public string? WhatsAppTemplate { get; set; }
    public string? PushTitleTemplate { get; set; }
    public string? PushBodyTemplate { get; set; }
    public List<string> RequiredFields { get; set; } = new();
    public Dictionary<string, string> DefaultValues { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }
}

public class NotificationRequest
{
    public string TemplateId { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public Dictionary<string, object> Data { get; set; } = new();
    public string? RecipientUserId { get; set; }
    public string? RecipientEmail { get; set; }
    public string? RecipientPhone { get; set; }
    public List<NotificationChannel> PreferredChannels { get; set; } = new();
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
}

public class NotificationContent
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public string? SmsMessage { get; set; }
    public string? WhatsAppMessage { get; set; }
    public string? PushTitle { get; set; }
    public string? PushBody { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public enum NotificationTemplateType
{
    IncidentCreated = 1,
    IncidentUpdated = 2,
    IncidentEscalated = 3,
    IncidentClosed = 4,
    InvestigatorAssigned = 5,
    ActionRequired = 6,
    DeadlineApproaching = 7,
    RegulatoryReport = 8,
    EmergencyAlert = 9,
    WeeklyDigest = 10,
    MonthlyReport = 11,
    Custom = 99
}

// Predefined template constants
public static class NotificationTemplates
{
    // Indonesian compliance - 2x24 hour reporting
    public const string INCIDENT_CREATED_ID = "incident_created";
    public const string INCIDENT_CRITICAL_ID = "incident_critical";
    public const string INCIDENT_REGULATORY_ID = "incident_regulatory";
    public const string ESCALATION_OVERDUE_ID = "escalation_overdue";
    public const string INVESTIGATOR_ASSIGNED_ID = "investigator_assigned";
    public const string ACTION_REQUIRED_ID = "action_required";
    public const string DEADLINE_APPROACHING_ID = "deadline_approaching";
    public const string INCIDENT_CLOSED_ID = "incident_closed";
    public const string EMERGENCY_ALERT_ID = "emergency_alert";
    public const string WEEKLY_DIGEST_ID = "weekly_digest";

    // Multi-language support
    public const string LANG_ENGLISH = "en";
    public const string LANG_BAHASA = "id";
}