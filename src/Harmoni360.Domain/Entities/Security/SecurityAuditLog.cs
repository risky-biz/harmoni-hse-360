using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities.Security;

public class SecurityAuditLog : BaseEntity, IAuditableEntity
{
    public SecurityAuditAction Action { get; private set; }
    public SecurityAuditCategory Category { get; private set; }
    public int UserId { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public string UserRole { get; private set; } = string.Empty;
    public string Resource { get; private set; } = string.Empty;
    public string Details { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;
    public DateTime ActionTimestamp { get; private set; }
    public SecuritySeverity Severity { get; private set; }
    public int? RelatedIncidentId { get; private set; }
    public string Metadata { get; private set; } = string.Empty; // JSON string
    public bool IsSecurityCritical { get; private set; }
    public string SessionId { get; private set; } = string.Empty;
    
    // Navigation Properties
    public User User { get; private set; } = null!;
    public SecurityIncident? RelatedIncident { get; private set; }
    
    // IAuditableEntity implementation
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    private SecurityAuditLog() { } // For EF Core

    public static SecurityAuditLog Create(
        SecurityAuditAction action,
        SecurityAuditCategory category,
        int userId,
        string userName,
        string userRole,
        string resource,
        string details,
        string ipAddress,
        string userAgent,
        SecuritySeverity severity = SecuritySeverity.Low,
        int? relatedIncidentId = null,
        Dictionary<string, string>? metadata = null,
        bool isSecurityCritical = false,
        string sessionId = "")
    {
        var auditLog = new SecurityAuditLog
        {
            Action = action,
            Category = category,
            UserId = userId,
            UserName = userName,
            UserRole = userRole,
            Resource = resource,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            ActionTimestamp = DateTime.UtcNow,
            Severity = severity,
            RelatedIncidentId = relatedIncidentId,
            Metadata = metadata != null ? System.Text.Json.JsonSerializer.Serialize(metadata) : "{}",
            IsSecurityCritical = isSecurityCritical,
            SessionId = sessionId
        };

        return auditLog;
    }

    public void UpdateSeverity(SecuritySeverity severity)
    {
        Severity = severity;
    }

    public void MarkAsSecurityCritical()
    {
        IsSecurityCritical = true;
    }

    public void LinkToIncident(int incidentId)
    {
        RelatedIncidentId = incidentId;
    }

    public Dictionary<string, string> GetMetadata()
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(Metadata) 
                   ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    public void AddMetadata(string key, string value)
    {
        var metadata = GetMetadata();
        metadata[key] = value;
        Metadata = System.Text.Json.JsonSerializer.Serialize(metadata);
    }

    // Computed properties for reporting
    public bool IsRecentActivity => ActionTimestamp >= DateTime.UtcNow.AddHours(-24);
    public bool IsHighRiskAction => Severity >= SecuritySeverity.High || IsSecurityCritical;
    public bool IsComplianceRelevant => Category is SecurityAuditCategory.DataAccess or 
                                                     SecurityAuditCategory.ComplianceReporting or 
                                                     SecurityAuditCategory.SystemConfiguration;

    public string ActionDescription => Action switch
    {
        SecurityAuditAction.Create => "Created",
        SecurityAuditAction.Read => "Viewed",
        SecurityAuditAction.Update => "Updated",
        SecurityAuditAction.Delete => "Deleted",
        SecurityAuditAction.Escalate => "Escalated",
        SecurityAuditAction.Assign => "Assigned",
        SecurityAuditAction.Close => "Closed",
        SecurityAuditAction.Export => "Exported",
        SecurityAuditAction.Login => "Logged In",
        SecurityAuditAction.Logout => "Logged Out",
        SecurityAuditAction.AccessDenied => "Access Denied",
        SecurityAuditAction.ConfigurationChange => "Configuration Changed",
        _ => Action.ToString()
    };

    public string CategoryDescription => Category switch
    {
        SecurityAuditCategory.IncidentManagement => "Incident Management",
        SecurityAuditCategory.ThreatAssessment => "Threat Assessment",
        SecurityAuditCategory.UserAuthentication => "User Authentication",
        SecurityAuditCategory.DataAccess => "Data Access",
        SecurityAuditCategory.SystemConfiguration => "System Configuration",
        SecurityAuditCategory.ComplianceReporting => "Compliance Reporting",
        SecurityAuditCategory.ThreatIntelligence => "Threat Intelligence",
        _ => Category.ToString()
    };
}

