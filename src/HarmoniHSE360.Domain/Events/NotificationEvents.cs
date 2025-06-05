using HarmoniHSE360.Domain.Common;
using HarmoniHSE360.Domain.Entities;

namespace HarmoniHSE360.Domain.Events;

public class NotificationRequiredEvent : IDomainEvent
{
    public NotificationRequiredEvent(
        int incidentId,
        string templateId,
        string recipientId,
        Dictionary<string, object> data,
        NotificationPriority priority = NotificationPriority.Normal)
    {
        IncidentId = incidentId;
        TemplateId = templateId;
        RecipientId = recipientId;
        Data = data;
        Priority = priority;
        OccurredOn = DateTime.UtcNow;
    }

    public int IncidentId { get; }
    public string TemplateId { get; }
    public string RecipientId { get; }
    public Dictionary<string, object> Data { get; }
    public NotificationPriority Priority { get; }
    public DateTime OccurredOn { get; }
}

public class EscalationTriggeredEvent : IDomainEvent
{
    public EscalationTriggeredEvent(
        int incidentId,
        string escalationRuleId,
        string reason,
        List<string> escalationTargets)
    {
        IncidentId = incidentId;
        EscalationRuleId = escalationRuleId;
        Reason = reason;
        EscalationTargets = escalationTargets;
        OccurredOn = DateTime.UtcNow;
    }

    public int IncidentId { get; }
    public string EscalationRuleId { get; }
    public string Reason { get; }
    public List<string> EscalationTargets { get; }
    public DateTime OccurredOn { get; }
}

public class RegulatoryReportRequiredEvent : IDomainEvent
{
    public RegulatoryReportRequiredEvent(
        int incidentId,
        string regulationType,
        DateTime deadline,
        List<string> authorities)
    {
        IncidentId = incidentId;
        RegulationType = regulationType;
        Deadline = deadline;
        Authorities = authorities;
        OccurredOn = DateTime.UtcNow;
    }

    public int IncidentId { get; }
    public string RegulationType { get; }
    public DateTime Deadline { get; }
    public List<string> Authorities { get; }
    public DateTime OccurredOn { get; }
}

public class EmergencyAlertTriggeredEvent : IDomainEvent
{
    public EmergencyAlertTriggeredEvent(
        int incidentId,
        string alertLevel,
        string location,
        List<string> emergencyContacts)
    {
        IncidentId = incidentId;
        AlertLevel = alertLevel;
        Location = location;
        EmergencyContacts = emergencyContacts;
        OccurredOn = DateTime.UtcNow;
    }

    public int IncidentId { get; }
    public string AlertLevel { get; }
    public string Location { get; }
    public List<string> EmergencyContacts { get; }
    public DateTime OccurredOn { get; }
}

public class DeadlineApproachingEvent : IDomainEvent
{
    public DeadlineApproachingEvent(
        int incidentId,
        string taskType,
        DateTime deadline,
        TimeSpan timeRemaining,
        string assignedToId)
    {
        IncidentId = incidentId;
        TaskType = taskType;
        Deadline = deadline;
        TimeRemaining = timeRemaining;
        AssignedToId = assignedToId;
        OccurredOn = DateTime.UtcNow;
    }

    public int IncidentId { get; }
    public string TaskType { get; }
    public DateTime Deadline { get; }
    public TimeSpan TimeRemaining { get; }
    public string AssignedToId { get; }
    public DateTime OccurredOn { get; }
}

