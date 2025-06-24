using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Web.Hubs;

namespace Harmoni360.Web.Services;

public class HSSENotificationService : IHSSENotificationService
{
    private readonly IHubContext<HSSEHub> _hubContext;
    private readonly ILogger<HSSENotificationService> _logger;

    public HSSENotificationService(
        IHubContext<HSSEHub> hubContext,
        ILogger<HSSENotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyHazardCreatedAsync(int hazardId, string department, string location, CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = new
            {
                Type = "HazardCreated",
                HazardId = hazardId,
                Department = department,
                Location = location,
                Timestamp = DateTime.UtcNow
            };

            // Notify all HSSE dashboard viewers
            await _hubContext.Clients.Group("HSSEDashboard")
                .SendAsync("HazardCreated", notification, cancellationToken);

            // Notify department-specific viewers
            if (!string.IsNullOrEmpty(department))
            {
                await _hubContext.Clients.Group($"Department_{department}")
                    .SendAsync("HazardCreated", notification, cancellationToken);
            }

            _logger.LogDebug("Sent hazard created notification for hazard {HazardId}", hazardId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending hazard created notification for hazard {HazardId}", hazardId);
        }
    }

    public async Task NotifyHazardUpdatedAsync(int hazardId, string department, string location, CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = new
            {
                Type = "HazardUpdated",
                HazardId = hazardId,
                Department = department,
                Location = location,
                Timestamp = DateTime.UtcNow
            };

            // Notify all HSSE dashboard viewers
            await _hubContext.Clients.Group("HSSEDashboard")
                .SendAsync("HazardUpdated", notification, cancellationToken);

            // Notify department-specific viewers
            if (!string.IsNullOrEmpty(department))
            {
                await _hubContext.Clients.Group($"Department_{department}")
                    .SendAsync("HazardUpdated", notification, cancellationToken);
            }

            _logger.LogDebug("Sent hazard updated notification for hazard {HazardId}", hazardId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending hazard updated notification for hazard {HazardId}", hazardId);
        }
    }

    public async Task NotifyIncidentCreatedAsync(int incidentId, string department, string location, CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = new
            {
                Type = "IncidentCreated",
                IncidentId = incidentId,
                Department = department,
                Location = location,
                Timestamp = DateTime.UtcNow
            };

            // Notify all HSSE dashboard viewers
            await _hubContext.Clients.Group("HSSEDashboard")
                .SendAsync("IncidentCreated", notification, cancellationToken);

            // Notify department-specific viewers
            if (!string.IsNullOrEmpty(department))
            {
                await _hubContext.Clients.Group($"Department_{department}")
                    .SendAsync("IncidentCreated", notification, cancellationToken);
            }

            _logger.LogDebug("Sent incident created notification for incident {IncidentId}", incidentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending incident created notification for incident {IncidentId}", incidentId);
        }
    }

    public async Task NotifyDashboardDataUpdatedAsync(string? department = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = new
            {
                Type = "DashboardDataUpdated",
                Department = department,
                Timestamp = DateTime.UtcNow
            };

            if (!string.IsNullOrEmpty(department))
            {
                // Notify specific department viewers
                await _hubContext.Clients.Group($"Department_{department}")
                    .SendAsync("DashboardDataUpdated", notification, cancellationToken);
            }
            else
            {
                // Notify all HSSE dashboard viewers
                await _hubContext.Clients.Group("HSSEDashboard")
                    .SendAsync("DashboardDataUpdated", notification, cancellationToken);
            }

            _logger.LogDebug("Sent dashboard data updated notification for department {Department}", department ?? "All");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending dashboard data updated notification for department {Department}", department ?? "All");
        }
    }

    public async Task NotifyMaterializedViewRefreshedAsync(string viewName, CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = new
            {
                Type = "MaterializedViewRefreshed",
                ViewName = viewName,
                Timestamp = DateTime.UtcNow
            };

            // Notify all HSSE dashboard viewers that materialized view data is refreshed
            await _hubContext.Clients.Group("HSSEDashboard")
                .SendAsync("MaterializedViewRefreshed", notification, cancellationToken);

            _logger.LogDebug("Sent materialized view refreshed notification for view {ViewName}", viewName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending materialized view refreshed notification for view {ViewName}", viewName);
        }
    }
}