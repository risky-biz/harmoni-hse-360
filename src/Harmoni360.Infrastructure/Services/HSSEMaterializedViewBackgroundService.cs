using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;

namespace Harmoni360.Infrastructure.Services;

public class HSSEMaterializedViewBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HSSEMaterializedViewBackgroundService> _logger;
    private readonly TimeSpan _refreshInterval = TimeSpan.FromHours(1); // Refresh every hour

    public HSSEMaterializedViewBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<HSSEMaterializedViewBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("HSSE Materialized View Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var refreshService = scope.ServiceProvider.GetRequiredService<IMaterializedViewRefreshService>();
                
                _logger.LogInformation("Starting scheduled refresh of HSSE materialized views");
                await refreshService.RefreshAllHSSEViewsAsync(stoppingToken);
                _logger.LogInformation("Completed scheduled refresh of HSSE materialized views");

                // Wait for the next refresh cycle
                await Task.Delay(_refreshInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // This is expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scheduled materialized view refresh");
                
                // Wait a shorter time before retrying after an error
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("HSSE Materialized View Background Service stopped");
    }
}