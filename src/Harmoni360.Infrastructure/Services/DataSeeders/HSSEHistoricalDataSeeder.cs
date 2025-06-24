using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Entities.Waste;
using Harmoni360.Domain.Enums;
using Harmoni360.Domain.ValueObjects;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class HSSEHistoricalDataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HSSEHistoricalDataSeeder> _logger;
    private readonly IConfiguration _configuration;
    private readonly Random _random = new(42); // Fixed seed for consistent data
    private readonly DateTime _startDate = DateTime.UtcNow.AddYears(-3);
    private readonly DateTime _endDate = DateTime.UtcNow;

    // Reference data for realistic seeding
    private readonly List<string> _departments = new()
    {
        "Operations", "Maintenance", "Production", "Quality Control", "Engineering",
        "Safety Department", "Environmental", "Human Resources", "Logistics", "Administration"
    };

    private readonly List<string> _locations = new()
    {
        "Main Production Facility", "Warehouse A", "Warehouse B", "Administrative Building",
        "Maintenance Workshop", "Chemical Storage Area", "Loading Dock", "Parking Area",
        "Cafeteria", "Emergency Assembly Point", "Pump House", "Electrical Substation"
    };

    private readonly List<string> _hazardTitles = new()
    {
        "Chemical Spill Risk in Storage Area", "Electrical Equipment Overheating", "Blocked Emergency Exit",
        "Unsafe Lifting Practices", "Poor Ventilation in Confined Space", "Damaged Safety Equipment",
        "Slippery Floor Conditions", "Inadequate Lighting", "Noise Exposure Risk", "Fire Safety Concern"
    };

    private readonly List<string> _incidentTitles = new()
    {
        "Equipment Failure Incident", "Personnel Injury Report", "Chemical Spill Event", 
        "Fire Emergency Response", "Electrical Fault", "Transportation Accident",
        "Environmental Release", "Security Breach", "Health Emergency", "Structural Damage"
    };

    public HSSEHistoricalDataSeeder(ApplicationDbContext context, ILogger<HSSEHistoricalDataSeeder> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        var forceReseedValue = _configuration["DataSeeding:ForceReseed"];
        var forceReseed = string.Equals(forceReseedValue, "true", StringComparison.OrdinalIgnoreCase);
        
        _logger.LogInformation("HSSEHistoricalDataSeeder - ForceReseed: {ForceReseed}", forceReseed);

        // Check if historical data already exists
        var existingHistoricalDataCount = await _context.Incidents.Where(i => i.IncidentDate < DateTime.UtcNow.AddMonths(-1)).CountAsync();
        
        if (!forceReseed && existingHistoricalDataCount > 10)
        {
            _logger.LogInformation("Historical HSSE data already exists ({Count} old incidents), skipping historical data generation", existingHistoricalDataCount);
            return;
        }

        _logger.LogInformation("Starting HSSE historical data seeding for period {StartDate} to {EndDate}...", 
            _startDate.ToString("yyyy-MM-dd"), _endDate.ToString("yyyy-MM-dd"));

        // Get reference users for assignments - following existing pattern
        var users = await _context.Users.Take(20).ToListAsync();
        if (!users.Any())
        {
            _logger.LogWarning("No users found - cannot seed historical HSSE data");
            return;
        }

        // Get configuration data - following existing pattern
        var hazardCategories = await _context.HazardCategories.ToListAsync();
        var hazardTypes = await _context.HazardTypes.ToListAsync();
        var incidentCategories = await _context.IncidentCategories.ToListAsync();
        var incidentLocations = await _context.IncidentLocations.ToListAsync();

        if (!hazardCategories.Any() || !hazardTypes.Any())
        {
            _logger.LogWarning("Required configuration data not found (hazard categories/types) - skipping historical data generation");
            return;
        }

        // Seed historical data in chronological order
        await SeedHistoricalHazardsAsync(users, hazardCategories, hazardTypes);
        await SeedHistoricalIncidentsAsync(users, incidentCategories, incidentLocations);
        await SeedHistoricalRiskAssessmentsAsync(users);

        _logger.LogInformation("HSSE historical data seeding completed successfully");
    }

    private async Task SeedHistoricalHazardsAsync(List<User> users, List<Domain.Entities.HazardCategory> categories, List<HazardType> types)
    {
        _logger.LogInformation("Seeding historical hazards data...");

        var hazards = new List<Hazard>();
        var currentDate = _startDate;

        while (currentDate < _endDate)
        {
            // Generate 3-8 hazards per month with seasonal variation
            var monthlyCount = _random.Next(3, 9);
            
            for (int i = 0; i < monthlyCount; i++)
            {
                var identifiedDate = currentDate.AddDays(_random.Next(0, DateTime.DaysInMonth(currentDate.Year, currentDate.Month)));
                var user = users[_random.Next(users.Count)];
                var location = _locations[_random.Next(_locations.Count)];
                var category = categories[_random.Next(categories.Count)];
                var type = types[_random.Next(types.Count)];
                var title = _hazardTitles[_random.Next(_hazardTitles.Count)];

                // Create hazard following the exact pattern from HazardDataSeeder
                var hazard = Hazard.Create(
                    title,
                    $"Historical hazard identified in {location} requiring assessment and mitigation actions.",
                    category.Id,
                    type.Id,
                    location,
                    (HazardSeverity)_random.Next(1, 5), // 1-4 severity levels
                    user.Id,
                    user.Department ?? _departments[_random.Next(_departments.Count)],
                    GeoLocation.Create(-6.2088 + (_random.NextDouble() - 0.5) * 0.01, 106.8456 + (_random.NextDouble() - 0.5) * 0.01)
                );

                // Set historical creation date - following existing pattern
                var hazardType = typeof(Hazard);
                var createdAtProperty = hazardType.GetProperty("CreatedAt");
                createdAtProperty?.SetValue(hazard, identifiedDate);

                // Update status for historical hazards - following existing pattern
                var daysOld = (DateTime.UtcNow - identifiedDate).TotalDays;
                if (daysOld > 60 && _random.NextDouble() < 0.8) // 80% of old hazards are resolved
                {
                    hazard.UpdateStatus(HazardStatus.Resolved, "system");
                }
                else if (daysOld > 30 && _random.NextDouble() < 0.6)
                {
                    hazard.UpdateStatus(HazardStatus.Monitoring, "system");
                }
                else if (_random.NextDouble() < 0.3)
                {
                    hazard.UpdateStatus(HazardStatus.ActionRequired, "system");
                }

                hazards.Add(hazard);
            }

            currentDate = currentDate.AddMonths(1);
        }

        await _context.Hazards.AddRangeAsync(hazards);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} historical hazards", hazards.Count);
    }

    private async Task SeedHistoricalIncidentsAsync(List<User> users, List<IncidentCategory> categories, List<IncidentLocation> locations)
    {
        _logger.LogInformation("Seeding historical incidents data...");

        var incidents = new List<Incident>();
        var currentDate = _startDate;

        while (currentDate < _endDate)
        {
            // Generate 1-4 incidents per month - incidents should be less frequent than hazards
            var monthlyCount = _random.Next(1, 5);
            
            for (int i = 0; i < monthlyCount; i++)
            {
                var incidentDate = currentDate.AddDays(_random.Next(0, DateTime.DaysInMonth(currentDate.Year, currentDate.Month)));
                var user = users[_random.Next(users.Count)];
                var location = _locations[_random.Next(_locations.Count)];
                var title = _incidentTitles[_random.Next(_incidentTitles.Count)];
                var severity = (IncidentSeverity)_random.Next(1, 5); // 1-4 severity levels

                // Create incident following the exact pattern from IncidentDataSeeder
                var incident = Incident.Create(
                    title,
                    $"Historical incident that occurred in {location} requiring investigation and follow-up actions.",
                    severity,
                    incidentDate,
                    location,
                    user.Name,
                    user.Email,
                    user.Department ?? _departments[_random.Next(_departments.Count)],
                    GeoLocation.Create(-6.2088 + (_random.NextDouble() - 0.5) * 0.01, 106.8456 + (_random.NextDouble() - 0.5) * 0.01),
                    user.Id,
                    null, // departmentId
                    categories.Any() ? categories[_random.Next(categories.Count)].Id : null,
                    locations.Any() ? locations[_random.Next(locations.Count)].Id : null
                );

                // Set historical creation date - following existing pattern
                var incidentType = typeof(Incident);
                var createdAtProperty = incidentType.GetProperty("CreatedAt");
                createdAtProperty?.SetValue(incident, incidentDate);

                // Update status for historical incidents - following existing pattern
                var daysOld = (DateTime.UtcNow - incidentDate).TotalDays;
                if (daysOld > 30 && _random.NextDouble() < 0.9) // 90% of old incidents are resolved
                {
                    incident.UpdateStatus(IncidentStatus.Resolved);
                }
                else if (daysOld > 14 && _random.NextDouble() < 0.7)
                {
                    incident.UpdateStatus(IncidentStatus.UnderInvestigation);
                }
                else if (_random.NextDouble() < 0.4)
                {
                    incident.UpdateStatus(IncidentStatus.AwaitingAction);
                }

                // Add injury details for more serious incidents - following existing pattern
                if (severity >= IncidentSeverity.Moderate && _random.NextDouble() < 0.6)
                {
                    var injuryTypes = Enum.GetValues<InjuryType>();
                    var injuryType = injuryTypes[_random.Next(injuryTypes.Length)];
                    incident.UpdateInjuryDetails(injuryType, severity >= IncidentSeverity.Serious, false);
                }

                incidents.Add(incident);
            }

            currentDate = currentDate.AddMonths(1);
        }

        await _context.Incidents.AddRangeAsync(incidents);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} historical incidents", incidents.Count);
    }

    private async Task SeedHistoricalRiskAssessmentsAsync(List<User> users)
    {
        _logger.LogInformation("Seeding historical risk assessments...");

        // Get hazards that need risk assessments
        var hazards = await _context.Hazards
            .Where(h => h.IdentifiedDate >= _startDate && h.IdentifiedDate <= _endDate)
            .ToListAsync();

        var riskAssessments = new List<RiskAssessment>();

        foreach (var hazard in hazards.Where(h => _random.NextDouble() < 0.75)) // 75% of hazards have risk assessments
        {
            var assessor = users[_random.Next(users.Count)];
            var assessmentDate = hazard.IdentifiedDate.AddDays(_random.Next(1, 14)); // Within 2 weeks

            var riskAssessment = RiskAssessment.Create(
                hazard.Id,
                RiskAssessmentType.General,
                assessor.Id,
                _random.Next(1, 6), // probabilityScore
                _random.Next(1, 6), // severityScore
                "Potential for injury, equipment damage, or operational disruption",
                "Engineering controls, administrative procedures, and PPE requirements in place",
                "Regular monitoring, staff training, and periodic review of control effectiveness",
                "Assessment completed as part of hazard management process"
            );

            // Set historical creation date
            var riskAssessmentType = typeof(RiskAssessment);
            var createdAtProperty = riskAssessmentType.GetProperty("CreatedAt");
            createdAtProperty?.SetValue(riskAssessment, assessmentDate);

            riskAssessments.Add(riskAssessment);
        }

        await _context.RiskAssessments.AddRangeAsync(riskAssessments);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} historical risk assessments", riskAssessments.Count);
    }
}