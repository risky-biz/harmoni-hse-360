using Harmoni360.Domain.Common;
using Harmoni360.Domain.Entities.Security;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Events;

/// <summary>
/// Event raised when a security incident is created
/// </summary>
public class SecurityIncidentCreatedEvent : IDomainEvent
{
    public SecurityIncident SecurityIncident { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public SecurityIncidentCreatedEvent(SecurityIncident securityIncident)
    {
        SecurityIncident = securityIncident;
    }
}

/// <summary>
/// Event raised when a critical security incident is reported
/// </summary>
public class CriticalSecurityIncidentEvent : IDomainEvent
{
    public SecurityIncident SecurityIncident { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public CriticalSecurityIncidentEvent(SecurityIncident securityIncident)
    {
        SecurityIncident = securityIncident;
    }
}

/// <summary>
/// Event raised when a security incident is assigned to someone
/// </summary>
public class SecurityIncidentAssignedEvent : IDomainEvent
{
    public SecurityIncident SecurityIncident { get; }
    public int AssignedToUserId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public SecurityIncidentAssignedEvent(SecurityIncident securityIncident, int assignedToUserId)
    {
        SecurityIncident = securityIncident;
        AssignedToUserId = assignedToUserId;
    }
}

/// <summary>
/// Event raised when an investigator is assigned to a security incident
/// </summary>
public class InvestigatorAssignedToSecurityIncidentEvent : IDomainEvent
{
    public SecurityIncident SecurityIncident { get; }
    public int InvestigatorId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public InvestigatorAssignedToSecurityIncidentEvent(SecurityIncident securityIncident, int investigatorId)
    {
        SecurityIncident = securityIncident;
        InvestigatorId = investigatorId;
    }
}

/// <summary>
/// Event raised when threat level is escalated
/// </summary>
public class SecurityThreatEscalatedEvent : IDomainEvent
{
    public SecurityIncident SecurityIncident { get; }
    public ThreatLevel PreviousLevel { get; }
    public ThreatLevel NewLevel { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public SecurityThreatEscalatedEvent(SecurityIncident securityIncident, ThreatLevel previousLevel, ThreatLevel newLevel)
    {
        SecurityIncident = securityIncident;
        PreviousLevel = previousLevel;
        NewLevel = newLevel;
    }
}

/// <summary>
/// Event raised when a data breach is detected
/// </summary>
public class DataBreachDetectedEvent : IDomainEvent
{
    public SecurityIncident SecurityIncident { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public DataBreachDetectedEvent(SecurityIncident securityIncident)
    {
        SecurityIncident = securityIncident;
    }
}

/// <summary>
/// Event raised when a security incident is contained
/// </summary>
public class SecurityIncidentContainedEvent : IDomainEvent
{
    public SecurityIncident SecurityIncident { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public SecurityIncidentContainedEvent(SecurityIncident securityIncident)
    {
        SecurityIncident = securityIncident;
    }
}

/// <summary>
/// Event raised when a security incident is resolved
/// </summary>
public class SecurityIncidentResolvedEvent : IDomainEvent
{
    public SecurityIncident SecurityIncident { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public SecurityIncidentResolvedEvent(SecurityIncident securityIncident)
    {
        SecurityIncident = securityIncident;
    }
}

/// <summary>
/// Event raised when a security incident is closed
/// </summary>
public class SecurityIncidentClosedEvent : IDomainEvent
{
    public SecurityIncident SecurityIncident { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public SecurityIncidentClosedEvent(SecurityIncident securityIncident)
    {
        SecurityIncident = securityIncident;
    }
}

/// <summary>
/// Event raised when a security control is implemented
/// </summary>
public class SecurityControlImplementedEvent : IDomainEvent
{
    public SecurityControl SecurityControl { get; }
    public int? RelatedIncidentId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public SecurityControlImplementedEvent(SecurityControl securityControl, int? relatedIncidentId = null)
    {
        SecurityControl = securityControl;
        RelatedIncidentId = relatedIncidentId;
    }
}

/// <summary>
/// Event raised when a security control becomes overdue for review
/// </summary>
public class SecurityControlOverdueEvent : IDomainEvent
{
    public SecurityControl SecurityControl { get; }
    public int DaysOverdue { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public SecurityControlOverdueEvent(SecurityControl securityControl, int daysOverdue)
    {
        SecurityControl = securityControl;
        DaysOverdue = daysOverdue;
    }
}

/// <summary>
/// Event raised when a threat indicator is detected
/// </summary>
public class ThreatIndicatorDetectedEvent : IDomainEvent
{
    public ThreatIndicator ThreatIndicator { get; }
    public SecurityIncident? RelatedIncident { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public ThreatIndicatorDetectedEvent(ThreatIndicator threatIndicator, SecurityIncident? relatedIncident = null)
    {
        ThreatIndicator = threatIndicator;
        RelatedIncident = relatedIncident;
    }
}

/// <summary>
/// Event raised when multiple threat indicators suggest a coordinated attack
/// </summary>
public class CoordinatedAttackDetectedEvent : IDomainEvent
{
    public List<ThreatIndicator> ThreatIndicators { get; }
    public string AttackPattern { get; }
    public ThreatLevel EstimatedThreatLevel { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public CoordinatedAttackDetectedEvent(List<ThreatIndicator> threatIndicators, string attackPattern, ThreatLevel estimatedThreatLevel)
    {
        ThreatIndicators = threatIndicators;
        AttackPattern = attackPattern;
        EstimatedThreatLevel = estimatedThreatLevel;
    }
}

/// <summary>
/// Event raised when insider threat activity is detected
/// </summary>
public class InsiderThreatDetectedEvent : IDomainEvent
{
    public SecurityIncident SecurityIncident { get; }
    public int SuspectedUserId { get; }
    public List<string> IndicatorsSummary { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public InsiderThreatDetectedEvent(SecurityIncident securityIncident, int suspectedUserId, List<string> indicatorsSummary)
    {
        SecurityIncident = securityIncident;
        SuspectedUserId = suspectedUserId;
        IndicatorsSummary = indicatorsSummary;
    }
}

/// <summary>
/// Event raised when security incident requires external reporting
/// </summary>
public class SecurityIncidentRequiresReportingEvent : IDomainEvent
{
    public SecurityIncident SecurityIncident { get; }
    public List<string> ReportingRequirements { get; }
    public DateTime ReportingDeadline { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public SecurityIncidentRequiresReportingEvent(SecurityIncident securityIncident, List<string> reportingRequirements, DateTime reportingDeadline)
    {
        SecurityIncident = securityIncident;
        ReportingRequirements = reportingRequirements;
        ReportingDeadline = reportingDeadline;
    }
}