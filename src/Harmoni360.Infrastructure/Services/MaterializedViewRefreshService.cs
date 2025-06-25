using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;

namespace Harmoni360.Infrastructure.Services;

public class MaterializedViewRefreshService : IMaterializedViewRefreshService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<MaterializedViewRefreshService> _logger;
    private readonly IHSSENotificationService _notificationService;

    public MaterializedViewRefreshService(
        IApplicationDbContext context,
        ILogger<MaterializedViewRefreshService> logger,
        IHSSENotificationService notificationService)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task RefreshAllHSSEViewsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting refresh of all HSSE materialized views");
        
        try
        {
            // Core HSSE views
            await RefreshHazardStatisticsViewAsync(cancellationToken);
            await RefreshIncidentFrequencyRatesViewAsync(cancellationToken);
            await RefreshSafetyPerformanceViewAsync(cancellationToken);
            await RefreshMonthlyHazardTrendsViewAsync(cancellationToken);
            
            // Extended module views
            await RefreshPPEComplianceViewAsync(cancellationToken);
            await RefreshTrainingSafetyViewAsync(cancellationToken);
            await RefreshInspectionSafetyViewAsync(cancellationToken);
            await RefreshWorkPermitSafetyViewAsync(cancellationToken);
            await RefreshWasteEnvironmentalViewAsync(cancellationToken);
            await RefreshSecurityIncidentsViewAsync(cancellationToken);
            await RefreshHealthMonitoringViewAsync(cancellationToken);
            await RefreshAuditFindingsViewAsync(cancellationToken);
            
            // Notify dashboard that all views have been refreshed
            await _notificationService.NotifyDashboardDataUpdatedAsync(cancellationToken: cancellationToken);
            
            _logger.LogInformation("Successfully refreshed all HSSE materialized views");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing HSSE materialized views");
            throw;
        }
    }

    public async Task RefreshHazardStatisticsViewAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Refreshing hazard statistics materialized view");
        
        try
        {
            await ExecuteRefreshCommand("REFRESH MATERIALIZED VIEW CONCURRENTLY mv_hazard_statistics_monthly;", cancellationToken);
            await _notificationService.NotifyMaterializedViewRefreshedAsync("mv_hazard_statistics_monthly", cancellationToken);
        }
        catch (Exception ex) when (ex.Message.Contains("does not exist"))
        {
            _logger.LogWarning("Materialized view mv_hazard_statistics_monthly does not exist, skipping refresh");
        }
    }

    public async Task RefreshIncidentFrequencyRatesViewAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Refreshing incident frequency rates materialized view");
        
        try
        {
            await ExecuteRefreshCommand("REFRESH MATERIALIZED VIEW CONCURRENTLY mv_incident_frequency_rates;", cancellationToken);
            await _notificationService.NotifyMaterializedViewRefreshedAsync("mv_incident_frequency_rates", cancellationToken);
        }
        catch (Exception ex) when (ex.Message.Contains("does not exist"))
        {
            _logger.LogWarning("Materialized view mv_incident_frequency_rates does not exist, skipping refresh");
        }
    }

    public async Task RefreshSafetyPerformanceViewAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Refreshing safety performance materialized view");
        
        try
        {
            await ExecuteRefreshCommand("REFRESH MATERIALIZED VIEW CONCURRENTLY mv_safety_performance_monthly;", cancellationToken);
            await _notificationService.NotifyMaterializedViewRefreshedAsync("mv_safety_performance_monthly", cancellationToken);
        }
        catch (Exception ex) when (ex.Message.Contains("does not exist"))
        {
            _logger.LogWarning("Materialized view mv_safety_performance_monthly does not exist, skipping refresh");
        }
    }

    public async Task RefreshMonthlyHazardTrendsViewAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Refreshing monthly hazard trends materialized view");
        
        try
        {
            await ExecuteRefreshCommand("REFRESH MATERIALIZED VIEW CONCURRENTLY mv_monthly_hazard_trends;", cancellationToken);
            await _notificationService.NotifyMaterializedViewRefreshedAsync("mv_monthly_hazard_trends", cancellationToken);
        }
        catch (Exception ex) when (ex.Message.Contains("does not exist"))
        {
            _logger.LogWarning("Materialized view mv_monthly_hazard_trends does not exist, skipping refresh");
        }
    }

    private async Task ExecuteRefreshCommand(string sql, CancellationToken cancellationToken)
    {
        try
        {
            var dbContext = (DbContext)_context;
            await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            _logger.LogDebug("Successfully executed: {Sql}", sql);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing SQL command: {Sql}", sql);
            
            // Try non-concurrent refresh as fallback
            var fallbackSql = sql.Replace(" CONCURRENTLY", "");
            _logger.LogWarning("Attempting fallback refresh without CONCURRENTLY: {FallbackSql}", fallbackSql);
            
            var dbContext = (DbContext)_context;
            await dbContext.Database.ExecuteSqlRawAsync(fallbackSql, cancellationToken);
        }
    }

    // Extended module view refresh methods
    public async Task RefreshPPEComplianceViewAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Refreshing PPE compliance materialized view");
        
        try
        {
            await ExecuteRefreshCommand("REFRESH MATERIALIZED VIEW CONCURRENTLY mv_ppe_compliance;", cancellationToken);
            await _notificationService.NotifyMaterializedViewRefreshedAsync("mv_ppe_compliance", cancellationToken);
        }
        catch (Exception ex) when (ex.Message.Contains("does not exist"))
        {
            _logger.LogWarning("Materialized view mv_ppe_compliance does not exist, skipping refresh");
        }
    }

    public async Task RefreshTrainingSafetyViewAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Refreshing training safety materialized view");
        
        try
        {
            await ExecuteRefreshCommand("REFRESH MATERIALIZED VIEW CONCURRENTLY mv_training_safety;", cancellationToken);
            await _notificationService.NotifyMaterializedViewRefreshedAsync("mv_training_safety", cancellationToken);
        }
        catch (Exception ex) when (ex.Message.Contains("does not exist"))
        {
            _logger.LogWarning("Materialized view mv_training_safety does not exist, skipping refresh");
        }
    }

    public async Task RefreshInspectionSafetyViewAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Refreshing inspection safety materialized view");
        
        try
        {
            await ExecuteRefreshCommand("REFRESH MATERIALIZED VIEW CONCURRENTLY mv_inspection_safety;", cancellationToken);
            await _notificationService.NotifyMaterializedViewRefreshedAsync("mv_inspection_safety", cancellationToken);
        }
        catch (Exception ex) when (ex.Message.Contains("does not exist"))
        {
            _logger.LogWarning("Materialized view mv_inspection_safety does not exist, skipping refresh");
        }
    }

    public async Task RefreshWorkPermitSafetyViewAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Refreshing work permit safety materialized view");
        
        try
        {
            await ExecuteRefreshCommand("REFRESH MATERIALIZED VIEW CONCURRENTLY mv_work_permit_safety;", cancellationToken);
            await _notificationService.NotifyMaterializedViewRefreshedAsync("mv_work_permit_safety", cancellationToken);
        }
        catch (Exception ex) when (ex.Message.Contains("does not exist"))
        {
            _logger.LogWarning("Materialized view mv_work_permit_safety does not exist, skipping refresh");
        }
    }

    public async Task RefreshWasteEnvironmentalViewAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Refreshing waste environmental materialized view");
        
        try
        {
            await ExecuteRefreshCommand("REFRESH MATERIALIZED VIEW CONCURRENTLY mv_waste_environmental;", cancellationToken);
            await _notificationService.NotifyMaterializedViewRefreshedAsync("mv_waste_environmental", cancellationToken);
        }
        catch (Exception ex) when (ex.Message.Contains("does not exist"))
        {
            _logger.LogWarning("Materialized view mv_waste_environmental does not exist, skipping refresh");
        }
    }

    public async Task RefreshSecurityIncidentsViewAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Refreshing security incidents materialized view");
        
        try
        {
            await ExecuteRefreshCommand("REFRESH MATERIALIZED VIEW CONCURRENTLY mv_security_incidents;", cancellationToken);
            await _notificationService.NotifyMaterializedViewRefreshedAsync("mv_security_incidents", cancellationToken);
        }
        catch (Exception ex) when (ex.Message.Contains("does not exist"))
        {
            _logger.LogWarning("Materialized view mv_security_incidents does not exist, skipping refresh");
        }
    }

    public async Task RefreshHealthMonitoringViewAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Refreshing health monitoring materialized view");
        
        try
        {
            await ExecuteRefreshCommand("REFRESH MATERIALIZED VIEW CONCURRENTLY mv_health_monitoring;", cancellationToken);
            await _notificationService.NotifyMaterializedViewRefreshedAsync("mv_health_monitoring", cancellationToken);
        }
        catch (Exception ex) when (ex.Message.Contains("does not exist"))
        {
            _logger.LogWarning("Materialized view mv_health_monitoring does not exist, skipping refresh");
        }
    }

    public async Task RefreshAuditFindingsViewAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Refreshing audit findings materialized view");
        
        try
        {
            await ExecuteRefreshCommand("REFRESH MATERIALIZED VIEW CONCURRENTLY mv_audit_findings;", cancellationToken);
            await _notificationService.NotifyMaterializedViewRefreshedAsync("mv_audit_findings", cancellationToken);
        }
        catch (Exception ex) when (ex.Message.Contains("does not exist"))
        {
            _logger.LogWarning("Materialized view mv_audit_findings does not exist, skipping refresh");
        }
    }
}