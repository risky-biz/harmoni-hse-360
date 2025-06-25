using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class HSSEKPIBaselineCalculator : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HSSEKPIBaselineCalculator> _logger;
    private readonly IConfiguration _configuration;
    private readonly Random _random = new(42);

    public HSSEKPIBaselineCalculator(ApplicationDbContext context, ILogger<HSSEKPIBaselineCalculator> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        var forceReseedValue = _configuration["DataSeeding:ForceReseed"];
        var forceReseed = string.Equals(forceReseedValue, "true", StringComparison.OrdinalIgnoreCase);
        
        _logger.LogInformation("HSSEKPIBaselineCalculator - ForceReseed: {ForceReseed}", forceReseed);

        _logger.LogInformation("Starting HSSE KPI baseline calculation...");

        // Calculate basic KPIs from existing data
        await CalculateIncidentKPIsAsync();
        await CalculateHazardKPIsAsync();
        await CalculateGeneralSafetyKPIsAsync();

        _logger.LogInformation("HSSE KPI baseline calculation completed");
    }

    private async Task CalculateIncidentKPIsAsync()
    {
        _logger.LogInformation("Calculating incident-based KPIs...");

        var incidents = await _context.Incidents.ToListAsync();
        
        if (!incidents.Any())
        {
            _logger.LogInformation("No incidents found for KPI calculation");
            return;
        }

        // Calculate basic incident metrics
        var totalIncidents = incidents.Count;
        var seriousIncidents = incidents.Count(i => i.Severity >= IncidentSeverity.Serious);
        var resolvedIncidents = incidents.Count(i => i.Status == IncidentStatus.Resolved);
        var recentIncidents = incidents.Count(i => i.IncidentDate >= DateTime.UtcNow.AddDays(-30));

        // Calculate percentages
        var seriousIncidentRate = totalIncidents > 0 ? (double)seriousIncidents / totalIncidents * 100 : 0;
        var resolutionRate = totalIncidents > 0 ? (double)resolvedIncidents / totalIncidents * 100 : 0;
        
        _logger.LogInformation("Incident KPIs - Total: {Total}, Serious: {Serious}, Resolution Rate: {ResolutionRate:F1}%", 
            totalIncidents, seriousIncidents, resolutionRate);
    }

    private async Task CalculateHazardKPIsAsync()
    {
        _logger.LogInformation("Calculating hazard-based KPIs...");

        var hazards = await _context.Hazards.ToListAsync();
        
        if (!hazards.Any())
        {
            _logger.LogInformation("No hazards found for KPI calculation");
            return;
        }

        // Calculate basic hazard metrics
        var totalHazards = hazards.Count;
        var criticalHazards = hazards.Count(h => h.Severity >= HazardSeverity.Major);
        var resolvedHazards = hazards.Count(h => h.Status == HazardStatus.Resolved);
        var recentHazards = hazards.Count(h => h.IdentifiedDate >= DateTime.UtcNow.AddDays(-30));

        // Calculate percentages
        var criticalHazardRate = totalHazards > 0 ? (double)criticalHazards / totalHazards * 100 : 0;
        var hazardResolutionRate = totalHazards > 0 ? (double)resolvedHazards / totalHazards * 100 : 0;

        _logger.LogInformation("Hazard KPIs - Total: {Total}, Critical: {Critical}, Resolution Rate: {ResolutionRate:F1}%", 
            totalHazards, criticalHazards, hazardResolutionRate);
    }

    private async Task CalculateGeneralSafetyKPIsAsync()
    {
        _logger.LogInformation("Calculating general safety KPIs...");

        // Get risk assessments
        var riskAssessments = await _context.RiskAssessments.ToListAsync();
        var totalRiskAssessments = riskAssessments.Count;

        // Get corrective actions
        var correctiveActions = await _context.CorrectiveActions.ToListAsync();
        var completedActions = correctiveActions.Count(ca => ca.Status == ActionStatus.Completed);
        var actionCompletionRate = correctiveActions.Count > 0 ? (double)completedActions / correctiveActions.Count * 100 : 0;

        // Calculate overall safety performance indicator
        var incidents = await _context.Incidents.ToListAsync();
        var hazards = await _context.Hazards.ToListAsync();
        
        var safetyScore = CalculateOverallSafetyScore(incidents, hazards, correctiveActions);

        _logger.LogInformation("General Safety KPIs - Risk Assessments: {RiskAssessments}, Action Completion: {ActionCompletion:F1}%, Safety Score: {SafetyScore:F1}", 
            totalRiskAssessments, actionCompletionRate, safetyScore);
    }

    private double CalculateOverallSafetyScore(List<Incident> incidents, List<Hazard> hazards, List<CorrectiveAction> actions)
    {
        // Simple safety score calculation based on resolution rates and severity distribution
        var baseScore = 100.0;
        
        // Deduct points for unresolved serious issues
        var unresolvedSeriousIncidents = incidents.Count(i => i.Severity >= IncidentSeverity.Serious && i.Status != IncidentStatus.Resolved);
        var unresolvedCriticalHazards = hazards.Count(h => h.Severity >= HazardSeverity.Major && h.Status != HazardStatus.Resolved);
        var overdueActions = actions.Count(a => a.DueDate < DateTime.UtcNow && a.Status != ActionStatus.Completed);
        
        baseScore -= (unresolvedSeriousIncidents * 5); // -5 points per unresolved serious incident
        baseScore -= (unresolvedCriticalHazards * 3); // -3 points per unresolved critical hazard
        baseScore -= (overdueActions * 2); // -2 points per overdue action
        
        return Math.Max(0, Math.Min(100, baseScore)); // Keep score between 0-100
    }
}