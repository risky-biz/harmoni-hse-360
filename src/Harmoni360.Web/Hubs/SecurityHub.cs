using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Harmoni360.Web.Hubs;

[Authorize]
public class SecurityHub : Hub
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SecurityHub> _logger;

    public SecurityHub(
        ICurrentUserService currentUserService,
        ILogger<SecurityHub> logger)
    {
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = _currentUserService.UserId;
        var userName = _currentUserService.Name;

        // Add to user-specific group
        await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");

        // Add to role-based groups based on permissions
        await JoinSecurityGroups(userId);

        _logger.LogInformation("User {UserName} connected to SecurityHub with connection {ConnectionId}", 
            userName, Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _currentUserService.UserId;
        var userName = _currentUserService.Name;

        // Remove from user-specific group
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");

        // Remove from role-based groups
        await LeaveSecurityGroups(userId);

        _logger.LogInformation("User {UserName} disconnected from SecurityHub with connection {ConnectionId}", 
            userName, Context.ConnectionId);

        if (exception != null)
        {
            _logger.LogError(exception, "SecurityHub connection error for user {UserName}", userName);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join security-specific notification groups
    /// </summary>
    public async Task JoinSecurityIncidentGroup(int incidentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"SecurityIncident_{incidentId}");
        _logger.LogDebug("User joined security incident group {IncidentId}", incidentId);
    }

    /// <summary>
    /// Leave security incident group
    /// </summary>
    public async Task LeaveSecurityIncidentGroup(int incidentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"SecurityIncident_{incidentId}");
        _logger.LogDebug("User left security incident group {IncidentId}", incidentId);
    }

    /// <summary>
    /// Join threat level monitoring group
    /// </summary>
    public async Task JoinThreatLevelGroup(ThreatLevel minLevel)
    {
        var groupName = $"ThreatLevel_{minLevel}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogDebug("User joined threat level group {ThreatLevel}", minLevel);
    }

    /// <summary>
    /// Leave threat level monitoring group
    /// </summary>
    public async Task LeaveThreatLevelGroup(ThreatLevel minLevel)
    {
        var groupName = $"ThreatLevel_{minLevel}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogDebug("User left threat level group {ThreatLevel}", minLevel);
    }

    /// <summary>
    /// Join location-based security monitoring
    /// </summary>
    public async Task JoinLocationGroup(string location)
    {
        var groupName = $"Location_{location.Replace(" ", "_")}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogDebug("User joined location group {Location}", location);
    }

    /// <summary>
    /// Leave location-based security monitoring
    /// </summary>
    public async Task LeaveLocationGroup(string location)
    {
        var groupName = $"Location_{location.Replace(" ", "_")}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogDebug("User left location group {Location}", location);
    }

    /// <summary>
    /// Subscribe to security dashboard updates
    /// </summary>
    public async Task SubscribeToDashboard()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "SecurityDashboard");
        _logger.LogDebug("User subscribed to security dashboard updates");
    }

    /// <summary>
    /// Unsubscribe from security dashboard updates
    /// </summary>
    public async Task UnsubscribeFromDashboard()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SecurityDashboard");
        _logger.LogDebug("User unsubscribed from security dashboard updates");
    }

    /// <summary>
    /// Request current security status
    /// </summary>
    public async Task RequestSecurityStatus()
    {
        try
        {
            // This would typically fetch current security metrics
            var status = new
            {
                Timestamp = DateTime.UtcNow,
                ActiveIncidents = 5, // Would come from service
                CriticalAlerts = 2,
                ThreatLevel = "Medium",
                SystemStatus = "Operational"
            };

            await Clients.Caller.SendAsync("SecurityStatusUpdate", status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending security status to user");
            await Clients.Caller.SendAsync("Error", "Failed to retrieve security status");
        }
    }

    private async Task JoinSecurityGroups(int userId)
    {
        // Add to general security notification groups based on role/permissions
        // This would typically check user permissions/roles

        // Add to security managers group
        await Groups.AddToGroupAsync(Context.ConnectionId, "SecurityManagers");

        // Add to security officers group
        await Groups.AddToGroupAsync(Context.ConnectionId, "SecurityOfficers");

        // Add to general security notifications
        await Groups.AddToGroupAsync(Context.ConnectionId, "SecurityNotifications");

        // Add to critical incident notifications
        await Groups.AddToGroupAsync(Context.ConnectionId, "CriticalIncidents");
    }

    private async Task LeaveSecurityGroups(int userId)
    {
        // Remove from security groups
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SecurityManagers");
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SecurityOfficers");
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SecurityNotifications");
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "CriticalIncidents");
    }
}

/// <summary>
/// Security notification service for SignalR integration
/// </summary>
public interface ISecurityNotificationHub
{
    Task SendSecurityIncidentNotification(int incidentId, string message, List<int>? userIds = null);
    Task SendThreatLevelUpdate(int incidentId, ThreatLevel newLevel, ThreatLevel previousLevel);
    Task SendSecurityAlert(SecurityIncidentType type, SecuritySeverity severity, string location, string message);
    Task SendEscalationNotification(int incidentId, string reason, List<int> notifyUserIds);
    Task SendAssignmentNotification(int incidentId, int assigneeId, string assigneeName);
    Task SendClosureNotification(int incidentId, string resolution);
    Task SendDashboardUpdate(object dashboardData);
    Task SendComplianceAlert(string complianceIssue, SecuritySeverity severity);
    Task SendThreatIntelligenceUpdate(string indicatorType, string indicatorValue, int confidence);
}

public class SecurityNotificationHub : ISecurityNotificationHub
{
    private readonly IHubContext<SecurityHub> _hubContext;
    private readonly ILogger<SecurityNotificationHub> _logger;

    public SecurityNotificationHub(
        IHubContext<SecurityHub> hubContext,
        ILogger<SecurityNotificationHub> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendSecurityIncidentNotification(int incidentId, string message, List<int>? userIds = null)
    {
        try
        {
            var notification = new
            {
                Type = "SecurityIncident",
                IncidentId = incidentId,
                Message = message,
                Timestamp = DateTime.UtcNow,
                Severity = "High"
            };

            if (userIds?.Any() == true)
            {
                // Send to specific users
                foreach (var userId in userIds)
                {
                    await _hubContext.Clients.Group($"User_{userId}")
                        .SendAsync("SecurityIncidentNotification", notification);
                }
            }
            else
            {
                // Send to security incident group and general security notifications
                await _hubContext.Clients.Groups($"SecurityIncident_{incidentId}", "SecurityNotifications")
                    .SendAsync("SecurityIncidentNotification", notification);
            }

            _logger.LogInformation("Security incident notification sent for incident {IncidentId}", incidentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending security incident notification for incident {IncidentId}", incidentId);
        }
    }

    public async Task SendThreatLevelUpdate(int incidentId, ThreatLevel newLevel, ThreatLevel previousLevel)
    {
        try
        {
            var notification = new
            {
                Type = "ThreatLevelUpdate",
                IncidentId = incidentId,
                NewLevel = newLevel.ToString(),
                PreviousLevel = previousLevel.ToString(),
                Timestamp = DateTime.UtcNow,
                IsEscalation = newLevel > previousLevel
            };

            // Send to incident-specific group and threat level groups
            await _hubContext.Clients.Groups($"SecurityIncident_{incidentId}", $"ThreatLevel_{newLevel}")
                .SendAsync("ThreatLevelUpdate", notification);

            // If escalated to High or Severe, notify critical incidents group
            if (newLevel >= ThreatLevel.High)
            {
                await _hubContext.Clients.Group("CriticalIncidents")
                    .SendAsync("CriticalThreatAlert", notification);
            }

            _logger.LogInformation("Threat level update sent for incident {IncidentId}: {PreviousLevel} -> {NewLevel}", 
                incidentId, previousLevel, newLevel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending threat level update for incident {IncidentId}", incidentId);
        }
    }

    public async Task SendSecurityAlert(SecurityIncidentType type, SecuritySeverity severity, string location, string message)
    {
        try
        {
            var alert = new
            {
                Type = "SecurityAlert",
                IncidentType = type.ToString(),
                Severity = severity.ToString(),
                Location = location,
                Message = message,
                Timestamp = DateTime.UtcNow,
                IsCritical = severity >= SecuritySeverity.High
            };

            // Send to location group and general notifications
            var locationGroup = $"Location_{location.Replace(" ", "_")}";
            await _hubContext.Clients.Groups(locationGroup, "SecurityNotifications")
                .SendAsync("SecurityAlert", alert);

            // Send to critical incidents if high severity
            if (severity >= SecuritySeverity.High)
            {
                await _hubContext.Clients.Group("CriticalIncidents")
                    .SendAsync("CriticalSecurityAlert", alert);
            }

            _logger.LogInformation("Security alert sent: {Type} at {Location} with severity {Severity}", 
                type, location, severity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending security alert for {Type} at {Location}", type, location);
        }
    }

    public async Task SendEscalationNotification(int incidentId, string reason, List<int> notifyUserIds)
    {
        try
        {
            var notification = new
            {
                Type = "IncidentEscalation",
                IncidentId = incidentId,
                Reason = reason,
                Timestamp = DateTime.UtcNow,
                RequiresAttention = true
            };

            // Send to specific users and security managers
            foreach (var userId in notifyUserIds)
            {
                await _hubContext.Clients.Group($"User_{userId}")
                    .SendAsync("EscalationNotification", notification);
            }

            await _hubContext.Clients.Group("SecurityManagers")
                .SendAsync("EscalationNotification", notification);

            _logger.LogInformation("Escalation notification sent for incident {IncidentId}", incidentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending escalation notification for incident {IncidentId}", incidentId);
        }
    }

    public async Task SendAssignmentNotification(int incidentId, int assigneeId, string assigneeName)
    {
        try
        {
            var notification = new
            {
                Type = "IncidentAssignment",
                IncidentId = incidentId,
                AssigneeId = assigneeId,
                AssigneeName = assigneeName,
                Timestamp = DateTime.UtcNow
            };

            // Send to assignee and incident group
            await _hubContext.Clients.Groups($"User_{assigneeId}", $"SecurityIncident_{incidentId}")
                .SendAsync("AssignmentNotification", notification);

            _logger.LogInformation("Assignment notification sent for incident {IncidentId} to user {AssigneeId}", 
                incidentId, assigneeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending assignment notification for incident {IncidentId}", incidentId);
        }
    }

    public async Task SendClosureNotification(int incidentId, string resolution)
    {
        try
        {
            var notification = new
            {
                Type = "IncidentClosure",
                IncidentId = incidentId,
                Resolution = resolution,
                Timestamp = DateTime.UtcNow
            };

            // Send to incident group and security notifications
            await _hubContext.Clients.Groups($"SecurityIncident_{incidentId}", "SecurityNotifications")
                .SendAsync("ClosureNotification", notification);

            _logger.LogInformation("Closure notification sent for incident {IncidentId}", incidentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending closure notification for incident {IncidentId}", incidentId);
        }
    }

    public async Task SendDashboardUpdate(object dashboardData)
    {
        try
        {
            var update = new
            {
                Type = "DashboardUpdate",
                Data = dashboardData,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group("SecurityDashboard")
                .SendAsync("DashboardUpdate", update);

            _logger.LogDebug("Security dashboard update sent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending dashboard update");
        }
    }

    public async Task SendComplianceAlert(string complianceIssue, SecuritySeverity severity)
    {
        try
        {
            var alert = new
            {
                Type = "ComplianceAlert",
                Issue = complianceIssue,
                Severity = severity.ToString(),
                Timestamp = DateTime.UtcNow,
                RequiresAction = severity >= SecuritySeverity.High
            };

            await _hubContext.Clients.Group("SecurityManagers")
                .SendAsync("ComplianceAlert", alert);

            _logger.LogInformation("Compliance alert sent: {Issue} with severity {Severity}", 
                complianceIssue, severity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending compliance alert: {Issue}", complianceIssue);
        }
    }

    public async Task SendThreatIntelligenceUpdate(string indicatorType, string indicatorValue, int confidence)
    {
        try
        {
            var update = new
            {
                Type = "ThreatIntelligenceUpdate",
                IndicatorType = indicatorType,
                IndicatorValue = indicatorValue,
                Confidence = confidence,
                Timestamp = DateTime.UtcNow,
                IsHighConfidence = confidence >= 80
            };

            await _hubContext.Clients.Group("SecurityNotifications")
                .SendAsync("ThreatIntelligenceUpdate", update);

            if (confidence >= 80)
            {
                await _hubContext.Clients.Group("CriticalIncidents")
                    .SendAsync("HighConfidenceThreatAlert", update);
            }

            _logger.LogInformation("Threat intelligence update sent: {Type}:{Value} with confidence {Confidence}", 
                indicatorType, indicatorValue, confidence);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending threat intelligence update for {Type}:{Value}", 
                indicatorType, indicatorValue);
        }
    }
}