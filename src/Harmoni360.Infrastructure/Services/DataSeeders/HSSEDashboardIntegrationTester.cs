using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class HSSEDashboardIntegrationTester : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HSSEDashboardIntegrationTester> _logger;
    private readonly IConfiguration _configuration;

    public HSSEDashboardIntegrationTester(ApplicationDbContext context, ILogger<HSSEDashboardIntegrationTester> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting HSSE dashboard integration testing...");

        await TestBasicDataAvailabilityAsync();
        await TestDataQualityAsync();

        _logger.LogInformation("HSSE dashboard integration testing completed");
    }

    private async Task TestBasicDataAvailabilityAsync()
    {
        _logger.LogInformation("Testing basic data availability for dashboard...");

        var incidentCount = await _context.Incidents.CountAsync();
        var hazardCount = await _context.Hazards.CountAsync();
        var riskAssessmentCount = await _context.RiskAssessments.CountAsync();
        var correctiveActionCount = await _context.CorrectiveActions.CountAsync();

        _logger.LogInformation("Dashboard data availability - Incidents: {Incidents}, Hazards: {Hazards}, Risk Assessments: {RiskAssessments}, Corrective Actions: {Actions}", 
            incidentCount, hazardCount, riskAssessmentCount, correctiveActionCount);

        if (incidentCount > 0 && hazardCount > 0)
        {
            _logger.LogInformation("Dashboard data availability test: PASSED - Sufficient data for visualization");
        }
        else
        {
            _logger.LogWarning("Dashboard data availability test: WARNING - Limited data may affect dashboard functionality");
        }
    }

    private async Task TestDataQualityAsync()
    {
        _logger.LogInformation("Testing data quality for dashboard integration...");

        // Check for data distribution across time periods
        var recentIncidents = await _context.Incidents.Where(i => i.IncidentDate >= DateTime.UtcNow.AddDays(-30)).CountAsync();
        var historicalIncidents = await _context.Incidents.Where(i => i.IncidentDate < DateTime.UtcNow.AddDays(-30)).CountAsync();

        var recentHazards = await _context.Hazards.Where(h => h.IdentifiedDate >= DateTime.UtcNow.AddDays(-30)).CountAsync();
        var historicalHazards = await _context.Hazards.Where(h => h.IdentifiedDate < DateTime.UtcNow.AddDays(-30)).CountAsync();

        _logger.LogInformation("Data distribution - Recent Incidents: {RecentIncidents}, Historical: {HistoricalIncidents}, Recent Hazards: {RecentHazards}, Historical: {HistoricalHazards}", 
            recentIncidents, historicalIncidents, recentHazards, historicalHazards);

        if (historicalIncidents > 0 || historicalHazards > 0)
        {
            _logger.LogInformation("Data quality test: PASSED - Historical data available for trend analysis");
        }
        else
        {
            _logger.LogInformation("Data quality test: INFO - Only recent data available, trends may be limited");
        }
    }
}