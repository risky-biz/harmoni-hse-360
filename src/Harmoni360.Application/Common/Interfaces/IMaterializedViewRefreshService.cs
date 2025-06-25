namespace Harmoni360.Application.Common.Interfaces;

public interface IMaterializedViewRefreshService
{
    Task RefreshAllHSSEViewsAsync(CancellationToken cancellationToken = default);
    Task RefreshHazardStatisticsViewAsync(CancellationToken cancellationToken = default);
    Task RefreshIncidentFrequencyRatesViewAsync(CancellationToken cancellationToken = default);
    Task RefreshSafetyPerformanceViewAsync(CancellationToken cancellationToken = default);
    Task RefreshMonthlyHazardTrendsViewAsync(CancellationToken cancellationToken = default);
    
    // Extended module views
    Task RefreshPPEComplianceViewAsync(CancellationToken cancellationToken = default);
    Task RefreshTrainingSafetyViewAsync(CancellationToken cancellationToken = default);
    Task RefreshInspectionSafetyViewAsync(CancellationToken cancellationToken = default);
    Task RefreshWorkPermitSafetyViewAsync(CancellationToken cancellationToken = default);
    Task RefreshWasteEnvironmentalViewAsync(CancellationToken cancellationToken = default);
    Task RefreshSecurityIncidentsViewAsync(CancellationToken cancellationToken = default);
    Task RefreshHealthMonitoringViewAsync(CancellationToken cancellationToken = default);
    Task RefreshAuditFindingsViewAsync(CancellationToken cancellationToken = default);
}