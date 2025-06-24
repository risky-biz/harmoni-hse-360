namespace Harmoni360.Application.Common.Interfaces;

public interface IHSSENotificationService
{
    Task NotifyHazardCreatedAsync(int hazardId, string department, string location, CancellationToken cancellationToken = default);
    Task NotifyHazardUpdatedAsync(int hazardId, string department, string location, CancellationToken cancellationToken = default);
    Task NotifyIncidentCreatedAsync(int incidentId, string department, string location, CancellationToken cancellationToken = default);
    Task NotifyDashboardDataUpdatedAsync(string? department = null, CancellationToken cancellationToken = default);
    Task NotifyMaterializedViewRefreshedAsync(string viewName, CancellationToken cancellationToken = default);
}