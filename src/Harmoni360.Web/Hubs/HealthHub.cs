using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Web.Hubs;

[Authorize]
public class HealthHub : Hub
{
    private readonly ILogger<HealthHub> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _context;

    public HealthHub(
        ILogger<HealthHub> logger, 
        ICurrentUserService currentUserService,
        IApplicationDbContext context)
    {
        _logger = logger;
        _currentUserService = currentUserService;
        _context = context;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var roles = Context.User?.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? new List<string>();

        // Add user to role-based groups for health notifications
        foreach (var role in roles)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"health-role-{role}");
        }

        // Add to user-specific health group
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"health-user-{userId}");
        }

        // Add to department-specific groups if user has department info
        var department = await GetUserDepartment(userId);
        if (!string.IsNullOrEmpty(department))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"health-dept-{department.ToLower()}");
        }

        // Add to health monitoring groups based on role
        if (roles.Contains("HealthManager") || roles.Contains("Nurse") || roles.Contains("Admin"))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "health-monitors");
        }

        if (roles.Contains("Admin") || roles.Contains("HealthManager"))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "health-analytics");
        }

        await base.OnConnectedAsync();
        _logger.LogInformation("User {UserId} connected to HealthHub with roles: {Roles}", userId, string.Join(", ", roles));
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("User {UserId} disconnected from HealthHub", userId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to health alerts for specific person
    /// </summary>
    public async Task SubscribeToPersonHealth(string personId)
    {
        try
        {
            // Verify user has permission to monitor this person's health
            var canMonitor = await CanMonitorPersonHealth(personId);
            if (!canMonitor)
            {
                await Clients.Caller.SendAsync("Error", "Unauthorized to monitor this person's health");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"health-person-{personId}");
            await Clients.Caller.SendAsync("SubscriptionConfirmed", $"Subscribed to health alerts for person {personId}");
            
            _logger.LogInformation("User {UserId} subscribed to health alerts for person {PersonId}", 
                Context.UserIdentifier, personId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to person health alerts");
            await Clients.Caller.SendAsync("Error", "Failed to subscribe to health alerts");
        }
    }

    /// <summary>
    /// Unsubscribe from health alerts for specific person
    /// </summary>
    public async Task UnsubscribeFromPersonHealth(string personId)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"health-person-{personId}");
            await Clients.Caller.SendAsync("UnsubscriptionConfirmed", $"Unsubscribed from health alerts for person {personId}");
            
            _logger.LogInformation("User {UserId} unsubscribed from health alerts for person {PersonId}", 
                Context.UserIdentifier, personId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from person health alerts");
            await Clients.Caller.SendAsync("Error", "Failed to unsubscribe from health alerts");
        }
    }

    /// <summary>
    /// Subscribe to department-wide health alerts
    /// </summary>
    public async Task SubscribeToDepartmentHealth(string department)
    {
        try
        {
            // Verify user has permission to monitor department health
            var canMonitor = await CanMonitorDepartmentHealth(department);
            if (!canMonitor)
            {
                await Clients.Caller.SendAsync("Error", "Unauthorized to monitor department health");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"health-dept-{department.ToLower()}");
            await Clients.Caller.SendAsync("SubscriptionConfirmed", $"Subscribed to health alerts for {department} department");
            
            _logger.LogInformation("User {UserId} subscribed to health alerts for department {Department}", 
                Context.UserIdentifier, department);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to department health alerts");
            await Clients.Caller.SendAsync("Error", "Failed to subscribe to department health alerts");
        }
    }

    /// <summary>
    /// Request real-time health dashboard updates
    /// </summary>
    public async Task RequestDashboardUpdates(string dashboardType = "general")
    {
        try
        {
            var roles = Context.User?.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList() ?? new List<string>();

            // Verify user has permission for dashboard access
            if (!roles.Contains("HealthManager") && !roles.Contains("Nurse") && !roles.Contains("Admin"))
            {
                await Clients.Caller.SendAsync("Error", "Unauthorized for health dashboard access");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"health-dashboard-{dashboardType}");
            await Clients.Caller.SendAsync("DashboardSubscriptionConfirmed", dashboardType);
            
            _logger.LogInformation("User {UserId} subscribed to health dashboard updates: {DashboardType}", 
                Context.UserIdentifier, dashboardType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to dashboard updates");
            await Clients.Caller.SendAsync("Error", "Failed to subscribe to dashboard updates");
        }
    }

    /// <summary>
    /// Send emergency health alert to appropriate groups
    /// </summary>
    public async Task SendEmergencyAlert(EmergencyHealthAlert alert)
    {
        try
        {
            // Verify user has permission to send emergency alerts
            var roles = Context.User?.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList() ?? new List<string>();

            if (!roles.Contains("HealthManager") && !roles.Contains("Nurse") && !roles.Contains("Admin"))
            {
                await Clients.Caller.SendAsync("Error", "Unauthorized to send emergency alerts");
                return;
            }

            // Send to health monitors
            await Clients.Group("health-monitors").SendAsync("EmergencyHealthAlert", alert);

            // Send to specific department if specified
            if (!string.IsNullOrEmpty(alert.Department))
            {
                await Clients.Group($"health-dept-{alert.Department.ToLower()}").SendAsync("EmergencyHealthAlert", alert);
            }

            // Send to person-specific subscribers
            if (!string.IsNullOrEmpty(alert.PersonId))
            {
                await Clients.Group($"health-person-{alert.PersonId}").SendAsync("EmergencyHealthAlert", alert);
            }

            _logger.LogWarning("Emergency health alert sent by user {UserId}: {AlertType} for person {PersonId}", 
                Context.UserIdentifier, alert.AlertType, alert.PersonId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending emergency health alert");
            await Clients.Caller.SendAsync("Error", "Failed to send emergency alert");
        }
    }

    /// <summary>
    /// Update vaccination status in real-time
    /// </summary>
    public async Task UpdateVaccinationStatus(string personId, VaccinationStatusUpdate update)
    {
        try
        {
            // Verify permission
            var canUpdate = await CanUpdateHealthRecords();
            if (!canUpdate)
            {
                await Clients.Caller.SendAsync("Error", "Unauthorized to update vaccination status");
                return;
            }

            // Notify subscribers
            await Clients.Group($"health-person-{personId}").SendAsync("VaccinationStatusUpdated", update);
            await Clients.Group("health-monitors").SendAsync("VaccinationStatusUpdated", update);

            _logger.LogInformation("Vaccination status updated for person {PersonId} by user {UserId}", 
                personId, Context.UserIdentifier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vaccination status");
            await Clients.Caller.SendAsync("Error", "Failed to update vaccination status");
        }
    }

    /// <summary>
    /// Report health incident in real-time
    /// </summary>
    public async Task ReportHealthIncident(HealthIncidentReport incident)
    {
        try
        {
            // Verify permission
            var canReport = await CanReportHealthIncidents();
            if (!canReport)
            {
                await Clients.Caller.SendAsync("Error", "Unauthorized to report health incidents");
                return;
            }

            // Send to appropriate groups based on severity
            if (incident.Severity == "Critical" || incident.Severity == "Emergency")
            {
                await Clients.Group("health-monitors").SendAsync("CriticalHealthIncident", incident);
                
                // Send to department
                if (!string.IsNullOrEmpty(incident.Department))
                {
                    await Clients.Group($"health-dept-{incident.Department.ToLower()}").SendAsync("CriticalHealthIncident", incident);
                }
            }
            else
            {
                await Clients.Group("health-monitors").SendAsync("HealthIncidentReported", incident);
            }

            _logger.LogInformation("Health incident reported by user {UserId}: {Severity} incident for person {PersonId}", 
                Context.UserIdentifier, incident.Severity, incident.PersonId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reporting health incident");
            await Clients.Caller.SendAsync("Error", "Failed to report health incident");
        }
    }

    // Helper methods for permission checking
    private Task<bool> CanMonitorPersonHealth(string personId)
    {
        var roles = Context.User?.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? new List<string>();

        // Admins and health managers can monitor anyone
        if (roles.Contains("Admin") || roles.Contains("HealthManager") || roles.Contains("Nurse"))
        {
            return Task.FromResult(true);
        }

        // Users can monitor their own health
        return Task.FromResult(Context.UserIdentifier == personId);
    }

    private Task<bool> CanMonitorDepartmentHealth(string department)
    {
        var roles = Context.User?.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? new List<string>();

        // Admins and health managers can monitor any department
        if (roles.Contains("Admin") || roles.Contains("HealthManager"))
        {
            return Task.FromResult(true);
        }

        // Department managers can monitor their own department  
        // TODO: Implement department checking
        return Task.FromResult(roles.Contains("DepartmentManager"));
    }

    private Task<bool> CanUpdateHealthRecords()
    {
        var roles = Context.User?.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? new List<string>();

        return Task.FromResult(roles.Contains("Admin") || roles.Contains("HealthManager") || roles.Contains("Nurse"));
    }

    private Task<bool> CanReportHealthIncidents()
    {
        var roles = Context.User?.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? new List<string>();

        // Most roles can report incidents, but not guests
        return Task.FromResult(!roles.Contains("Guest"));
    }

    private Task<string?> GetUserDepartment(string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return Task.FromResult<string?>(null);

        try
        {
            // This would typically query the user's department from the database
            // For now, return null as placeholder
            return Task.FromResult<string?>(null);
        }
        catch
        {
            return Task.FromResult<string?>(null);
        }
    }
}

// Supporting classes for HealthHub
public class EmergencyHealthAlert
{
    public string PersonId { get; set; } = string.Empty;
    public string PersonName { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Department { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public List<string> EmergencyContacts { get; set; } = new();
    public bool RequiresImmediateResponse { get; set; }
}

public class VaccinationStatusUpdate
{
    public string PersonId { get; set; } = string.Empty;
    public string PersonName { get; set; } = string.Empty;
    public string VaccineName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? DateAdministered { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class HealthIncidentReport
{
    public string PersonId { get; set; } = string.Empty;
    public string PersonName { get; set; } = string.Empty;
    public string IncidentType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Department { get; set; }
    public DateTime IncidentTime { get; set; }
    public string ReportedBy { get; set; } = string.Empty;
    public bool RequiresHospitalization { get; set; }
    public bool ParentsNotified { get; set; }
    public List<string> TreatmentProvided { get; set; } = new();
}