using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Common.Interfaces;

public interface ISecurityAuditService
{
    Task LogSecurityActionAsync(SecurityAuditLogRequest auditLog, CancellationToken cancellationToken = default);
    Task LogIncidentCreatedAsync(int incidentId, int userId, string details, CancellationToken cancellationToken = default);
    Task LogIncidentUpdatedAsync(int incidentId, int userId, string changes, CancellationToken cancellationToken = default);
    Task LogIncidentDeletedAsync(int incidentId, int userId, string reason, CancellationToken cancellationToken = default);
    Task LogIncidentEscalatedAsync(int incidentId, int userId, string reason, CancellationToken cancellationToken = default);
    Task LogIncidentAssignedAsync(int incidentId, int assignedByUserId, int assignedToUserId, CancellationToken cancellationToken = default);
    Task LogIncidentClosedAsync(int incidentId, int userId, string resolution, CancellationToken cancellationToken = default);
    Task LogThreatAssessmentAsync(int incidentId, int userId, ThreatLevel previousLevel, ThreatLevel newLevel, CancellationToken cancellationToken = default);
    Task LogUnauthorizedAccessAttemptAsync(int userId, string resource, string ipAddress, CancellationToken cancellationToken = default);
    Task LogDataAccessAsync(int userId, string dataType, string action, string details, CancellationToken cancellationToken = default);
    Task LogConfigurationChangeAsync(int userId, string setting, string oldValue, string newValue, CancellationToken cancellationToken = default);
    Task<List<SecurityAuditLogDto>> GetAuditTrailAsync(int? incidentId = null, int? userId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<List<SecurityAuditLogDto>> GetUserActivityAsync(int userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<SecurityComplianceAuditDto> GenerateComplianceAuditAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}

public class SecurityAuditLogRequest
{
    public SecurityAuditAction Action { get; set; }
    public SecurityAuditCategory Category { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public SecuritySeverity Severity { get; set; } = SecuritySeverity.Low;
    public int? RelatedIncidentId { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    public bool IsSecurityCritical { get; set; }
    public string SessionId { get; set; } = string.Empty;
}

public class SecurityAuditLogDto
{
    public int Id { get; set; }
    public SecurityAuditAction Action { get; set; }
    public SecurityAuditCategory Category { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public SecuritySeverity Severity { get; set; }
    public int? RelatedIncidentId { get; set; }
    public bool IsSecurityCritical { get; set; }
    public string ActionDescription => GetActionDescription();
    public string CategoryDescription => GetCategoryDescription();
    
    private string GetActionDescription()
    {
        return Action switch
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
    }
    
    private string GetCategoryDescription()
    {
        return Category switch
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
}

public class SecurityComplianceAuditDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalActions { get; set; }
    public int SecurityCriticalActions { get; set; }
    public int UnauthorizedAccessAttempts { get; set; }
    public int DataAccessViolations { get; set; }
    public int ConfigurationChanges { get; set; }
    public List<SecurityAuditSummaryDto> ActionSummary { get; set; } = new();
    public List<SecurityAuditSummaryDto> CategorySummary { get; set; } = new();
    public List<SecurityAuditLogDto> CriticalActions { get; set; } = new();
    public List<string> ComplianceNotes { get; set; } = new();
    public bool IsCompliant { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class SecurityAuditSummaryDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

