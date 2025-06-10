using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.ValueObjects;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class HazardDataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HazardDataSeeder> _logger;
    private readonly IConfiguration _configuration;

    public HazardDataSeeder(ApplicationDbContext context, ILogger<HazardDataSeeder> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        // Check if we should re-seed hazards even if they exist
        var reSeedHazards = _configuration["DataSeeding:ReSeedHazards"] == "true";

        if (!reSeedHazards && await _context.Hazards.AnyAsync())
        {
            _logger.LogInformation("Hazards already exist and ReSeedHazards is false, skipping hazard seeding");
            return;
        }

        _logger.LogInformation("Starting hazard seeding...");

        // If re-seeding is enabled, clear existing hazards first
        if (reSeedHazards && await _context.Hazards.AnyAsync())
        {
            _logger.LogInformation("Clearing existing hazards for re-seeding...");
            _context.Hazards.RemoveRange(_context.Hazards);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Existing hazards cleared");
        }

        var random = new Random();
        var departments = new[] { "Chemistry", "Physics", "Biology", "PE", "Maintenance", "Administration", "IT", "Security", "Cafeteria", "Library" };
        
        // Get a system user for all operations
        var systemUser = await _context.Users.FirstOrDefaultAsync();
        if (systemUser == null)
        {
            _logger.LogWarning("No users found for hazard seeding. Skipping hazard seeding.");
            return;
        }

        var hazardTemplates = new[]
        {
            new { 
                Title = "Slippery floors in laboratory", 
                Description = "Chemical spills and water on laboratory floors create slip hazards", 
                Category = HazardCategory.Physical, 
                Type = HazardType.Slip,
                Severity = HazardSeverity.Moderate,
            },
            new { 
                Title = "Unsecured chemical storage", 
                Description = "Chemical containers not properly secured in storage areas", 
                Category = HazardCategory.Chemical, 
                Type = HazardType.Exposure,
                Severity = HazardSeverity.Major,
            },
            new { 
                Title = "Faulty electrical wiring in older buildings", 
                Description = "Outdated electrical systems pose fire and electrocution risks", 
                Category = HazardCategory.Electrical, 
                Type = HazardType.Other,
                Severity = HazardSeverity.Major,
            },
            new { 
                Title = "Heavy equipment without proper lifting aids", 
                Description = "Manual handling of heavy laboratory equipment without mechanical aids", 
                Category = HazardCategory.Ergonomic, 
                Type = HazardType.Other,
                Severity = HazardSeverity.Moderate,
            },
            new { 
                Title = "Inadequate ventilation in chemistry labs", 
                Description = "Poor air circulation in areas where chemical fumes are present", 
                Category = HazardCategory.Environmental, 
                Type = HazardType.Exposure,
                Severity = HazardSeverity.Moderate,
            },
            new { 
                Title = "Playground equipment wear and tear", 
                Description = "Aging playground equipment showing signs of metal fatigue and wear", 
                Category = HazardCategory.Mechanical, 
                Type = HazardType.Fall,
                Severity = HazardSeverity.Major,
            },
            new { 
                Title = "Fire evacuation route blockages", 
                Description = "Emergency exits and evacuation routes partially blocked by storage", 
                Category = HazardCategory.Fire, 
                Type = HazardType.Fire,
                Severity = HazardSeverity.Catastrophic,
            },
            new { 
                Title = "Poor lighting in stairwells", 
                Description = "Inadequate lighting in main stairwells during evening hours", 
                Category = HazardCategory.Physical, 
                Type = HazardType.Fall,
                Severity = HazardSeverity.Moderate,
            },
            new { 
                Title = "Noise levels exceeding limits in workshop", 
                Description = "Power tools and machinery generating noise above acceptable levels", 
                Category = HazardCategory.Physical, 
                Type = HazardType.Exposure,
                Severity = HazardSeverity.Minor,
            },
            new { 
                Title = "Food storage temperature control issues", 
                Description = "Cafeteria refrigeration units not maintaining consistent temperatures", 
                Category = HazardCategory.Biological, 
                Type = HazardType.Exposure,
                Severity = HazardSeverity.Major,
            }
        };

        var hazards = new List<Hazard>();

        foreach (var template in hazardTemplates)
        {
            var department = departments[random.Next(departments.Length)];
            var createdDate = DateTime.UtcNow.AddDays(-random.Next(1, 30));
            
            // Create location with 70% probability
            GeoLocation? location = null;
            if (random.NextDouble() > 0.3)
            {
                // BSJ coordinates with small variations
                var baseLat = -6.2088;
                var baseLng = 106.8456;
                var latVariation = (random.NextDouble() - 0.5) * 0.001; // Small variation
                var lngVariation = (random.NextDouble() - 0.5) * 0.001;
                location = GeoLocation.Create(baseLat + latVariation, baseLng + lngVariation);
            }

            // Use the system user for the reporter

            var hazard = Hazard.Create(
                template.Title,
                template.Description,
                template.Category,
                template.Type,
                department,
                template.Severity,
                systemUser.Id,
                department,
                location
            );

            hazards.Add(hazard);
        }

        await _context.Hazards.AddRangeAsync(hazards);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} hazards", hazards.Count);

        // Now create risk assessments for each hazard
        var riskAssessments = new List<RiskAssessment>();
            foreach (var hazard in hazards)
            {
                var template = hazardTemplates.First(t => t.Title == hazard.Title);
                
                // Map severity to probability and severity scores
                var (probabilityScore, severityScore) = template.Severity switch
                {
                    HazardSeverity.Negligible => (2, 1),
                    HazardSeverity.Minor => (3, 2),
                    HazardSeverity.Moderate => (3, 3),
                    HazardSeverity.Major => (4, 4),
                    HazardSeverity.Catastrophic => (4, 5),
                    _ => (3, 3)
                };
                
                var riskAssessment = RiskAssessment.Create(
                    hazard.Id,
                    RiskAssessmentType.General,
                    systemUser.Id,
                    probabilityScore,
                    severityScore,
                    "Potential consequences based on hazard category and type",
                    "Basic safety protocols and procedures in place",
                    "Implementation of recommended mitigation actions",
                    "Initial risk assessment based on hazard identification"
                );

                riskAssessments.Add(riskAssessment);
            }

        await _context.RiskAssessments.AddRangeAsync(riskAssessments);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} risk assessments", riskAssessments.Count);

        // Create mitigation actions for high and critical risk hazards
        var mitigationActions = new List<HazardMitigationAction>();
        var mitigationTemplates = new Dictionary<string, string[]>
        {
            ["Slippery floors in laboratory"] = new[] { "Install non-slip mats", "Improve spill cleanup procedures", "Regular floor maintenance schedule" },
            ["Unsecured chemical storage"] = new[] { "Install secure chemical cabinets", "Implement chemical inventory system", "Staff training on storage protocols" },
            ["Faulty electrical wiring in older buildings"] = new[] { "Electrical system inspection", "Upgrade to modern wiring standards", "Install additional circuit breakers" },
            ["Heavy equipment without proper lifting aids"] = new[] { "Purchase mechanical lifting equipment", "Train staff on proper lifting techniques", "Implement buddy system for heavy lifting" },
            ["Inadequate ventilation in chemistry labs"] = new[] { "Install additional fume hoods", "Upgrade ventilation system", "Regular air quality monitoring" },
            ["Playground equipment wear and tear"] = new[] { "Schedule comprehensive equipment inspection", "Replace worn components", "Implement preventive maintenance program" },
            ["Fire evacuation route blockages"] = new[] { "Clear all evacuation routes", "Install evacuation route signage", "Regular evacuation drills" },
            ["Poor lighting in stairwells"] = new[] { "Install LED lighting upgrades", "Add emergency lighting systems", "Regular lighting maintenance" },
            ["Noise levels exceeding limits in workshop"] = new[] { "Provide hearing protection equipment", "Install noise dampening materials", "Limit exposure time for students" },
            ["Unsecured data access in computer labs"] = new[] { "Implement network access controls", "Install monitoring software", "Staff training on data security" },
            ["Food storage temperature control issues"] = new[] { "Service refrigeration equipment", "Install temperature monitoring systems", "Staff training on food safety" },
            ["Unauthorized access to building after hours"] = new[] { "Upgrade security system", "Install additional cameras", "Implement access card system" }
        };

        foreach (var hazard in hazards)
        {
            if (mitigationTemplates.ContainsKey(hazard.Title))
            {
                var actions = mitigationTemplates[hazard.Title];
                foreach (var actionDescription in actions)
                {
                    // Use the system user to assign the action to

                    var action = HazardMitigationAction.Create(
                        hazard.Id,
                        actionDescription,
                        MitigationActionType.Administrative,
                        DateTime.UtcNow.AddDays(random.Next(7, 30)),
                        systemUser.Id,
                        MitigationPriority.Medium
                    );

                    // Some actions are completed (30% chance)
                    if (random.NextDouble() < 0.3)
                    {
                        action.StartImplementation("system");
                        action.CompleteAction("Completed as part of safety improvement program", null, "system");
                    }

                    mitigationActions.Add(action);
                }
            }
        }

        await _context.HazardMitigationActions.AddRangeAsync(mitigationActions);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} mitigation actions", mitigationActions.Count);
    }
}