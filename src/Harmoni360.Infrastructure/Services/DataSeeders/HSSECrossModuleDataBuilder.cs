using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Entities.Waste;
using Harmoni360.Domain.Enums;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class HSSECrossModuleDataBuilder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HSSECrossModuleDataBuilder> _logger;
    private readonly IConfiguration _configuration;
    private readonly Random _random = new(42);

    public HSSECrossModuleDataBuilder(ApplicationDbContext context, ILogger<HSSECrossModuleDataBuilder> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        var forceReseedValue = _configuration["DataSeeding:ForceReseed"];
        var forceReseed = string.Equals(forceReseedValue, "true", StringComparison.OrdinalIgnoreCase);
        
        _logger.LogInformation("HSSECrossModuleDataBuilder - ForceReseed: {ForceReseed}", forceReseed);

        // Check if cross-module relationships already exist
        var existingCorrectiveActions = await _context.CorrectiveActions.CountAsync();
        
        if (!forceReseed && existingCorrectiveActions > 10)
        {
            _logger.LogInformation("Cross-module relationships already exist ({Count} corrective actions), skipping cross-module data building", existingCorrectiveActions);
            return;
        }

        _logger.LogInformation("Starting HSSE cross-module relationship building...");

        // Build basic cross-module relationships
        await BuildIncidentCorrectiveActionsAsync();
        await BuildHazardMitigationActionsAsync();

        _logger.LogInformation("HSSE cross-module relationship building completed");
    }

    private async Task BuildIncidentCorrectiveActionsAsync()
    {
        _logger.LogInformation("Building incident corrective actions...");

        // Get resolved incidents that don't have corrective actions yet
        var incidents = await _context.Incidents
            .Where(i => i.Status == IncidentStatus.Resolved && !i.CorrectiveActions.Any())
            .Take(20)
            .ToListAsync();

        var correctiveActions = new List<CorrectiveAction>();

        foreach (var incident in incidents)
        {
            // Create 1-2 corrective actions per incident
            var actionCount = _random.Next(1, 3);
            
            for (int i = 0; i < actionCount; i++)
            {
                var action = CorrectiveAction.Create(
                    incident.Id,
                    GenerateCorrectiveActionDescription(incident.Title),
                    incident.ReporterDepartment,
                    DateTime.UtcNow.AddDays(_random.Next(7, 30)),
                    (ActionPriority)_random.Next(1, 4),
                    "system"
                );

                // Mark some actions as completed for historical data
                if (_random.NextDouble() < 0.7)
                {
                    action.Complete("Action completed successfully", "system");
                }

                correctiveActions.Add(action);
            }
        }

        if (correctiveActions.Any())
        {
            await _context.CorrectiveActions.AddRangeAsync(correctiveActions);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created {Count} corrective actions for incidents", correctiveActions.Count);
        }
    }

    private async Task BuildHazardMitigationActionsAsync()
    {
        _logger.LogInformation("Building hazard mitigation actions...");

        // Get hazards that don't have mitigation actions yet
        var hazards = await _context.Hazards
            .Where(h => h.Severity >= HazardSeverity.Moderate && !h.MitigationActions.Any())
            .Take(20)
            .ToListAsync();

        var mitigationActions = new List<HazardMitigationAction>();

        foreach (var hazard in hazards)
        {
            // Create 1-3 mitigation actions per hazard
            var actionCount = _random.Next(1, 4);
            
            for (int i = 0; i < actionCount; i++)
            {
                var action = HazardMitigationAction.Create(
                    hazard.Id,
                    GenerateMitigationActionDescription(hazard.Title),
                    (MitigationActionType)_random.Next(1, 6), // 1-5 for all mitigation types
                    DateTime.UtcNow.AddDays(_random.Next(7, 45)),
                    hazard.ReporterId,
                    (MitigationPriority)_random.Next(1, 5) // 1-4 for all priority levels
                );

                // Mark some actions as completed for historical data
                if (_random.NextDouble() < 0.6)
                {
                    action.StartImplementation("system");
                    action.CompleteAction("Mitigation action implemented successfully", null, "system");
                }

                mitigationActions.Add(action);
            }
        }

        if (mitigationActions.Any())
        {
            await _context.HazardMitigationActions.AddRangeAsync(mitigationActions);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created {Count} mitigation actions for hazards", mitigationActions.Count);
        }
    }

    private string GenerateCorrectiveActionDescription(string incidentTitle)
    {
        var actions = new[]
        {
            "Review and update safety procedures",
            "Provide additional training to affected personnel",
            "Install additional safety equipment",
            "Conduct thorough equipment inspection",
            "Update emergency response procedures",
            "Implement additional monitoring measures",
            "Review staff training requirements",
            "Upgrade safety signage and warnings"
        };

        return $"{actions[_random.Next(actions.Length)]} related to: {incidentTitle}";
    }

    private string GenerateMitigationActionDescription(string hazardTitle)
    {
        var actions = new[]
        {
            "Implement engineering controls",
            "Establish administrative procedures",
            "Provide appropriate PPE",
            "Install safety barriers",
            "Improve ventilation systems",
            "Conduct regular safety inspections",
            "Implement lockout/tagout procedures",
            "Establish safety monitoring protocols"
        };

        return $"{actions[_random.Next(actions.Length)]} to address: {hazardTitle}";
    }
}