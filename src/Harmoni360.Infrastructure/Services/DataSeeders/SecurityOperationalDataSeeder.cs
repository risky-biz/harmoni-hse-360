using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Entities.Security;
using Harmoni360.Domain.Enums;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class SecurityOperationalDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SecurityOperationalDataSeeder> _logger;
    private readonly IConfiguration _configuration;
    private readonly Random _random = new();

    public SecurityOperationalDataSeeder(ApplicationDbContext context, ILogger<SecurityOperationalDataSeeder> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting Security Management operational data seeding...");

            // Get required data
            var users = await _context.Users.ToListAsync();
            var departments = new[] { "Security", "Operations", "IT", "Administration", "Maintenance", "Finance" };

            if (!users.Any())
            {
                _logger.LogWarning("Cannot seed Security operational data - missing users");
                return;
            }

            // Clear existing operational data if force reseed
            var forceReseedValue = _configuration["DataSeeding:ForceReseed"];
            var forceReseed = string.Equals(forceReseedValue, "true", StringComparison.OrdinalIgnoreCase) || 
                             string.Equals(forceReseedValue, "True", StringComparison.OrdinalIgnoreCase) ||
                             (bool.TryParse(forceReseedValue, out var boolResult) && boolResult);
            if (forceReseed)
            {
                await ClearExistingDataAsync();
            }

            // Check if data already exists
            if (!forceReseed && await _context.SecurityIncidents.AnyAsync())
            {
                _logger.LogInformation("Security operational data already exists, skipping seeding");
                return;
            }

            // Create operational data
            await SeedSecurityIncidentsAsync(users, departments);
            await SeedThreatIndicatorsAsync();
            await SeedSecurityControlsAsync(users, departments);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Security Management operational data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding Security operational data");
            throw;
        }
    }

    private async Task ClearExistingDataAsync()
    {
        _logger.LogInformation("Clearing existing Security operational data...");
        
        _context.SecurityAuditLogs.RemoveRange(_context.SecurityAuditLogs);
        _context.SecurityIncidentResponses.RemoveRange(_context.SecurityIncidentResponses);
        _context.SecurityIncidentAttachments.RemoveRange(_context.SecurityIncidentAttachments);
        _context.SecurityIncidentInvolvedPersons.RemoveRange(_context.SecurityIncidentInvolvedPersons);
        _context.ThreatAssessments.RemoveRange(_context.ThreatAssessments);
        _context.SecurityIncidents.RemoveRange(_context.SecurityIncidents);
        _context.ThreatIndicators.RemoveRange(_context.ThreatIndicators);
        _context.SecurityControls.RemoveRange(_context.SecurityControls);
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Existing Security operational data cleared");
    }

    private async Task SeedSecurityIncidentsAsync(List<User> users, string[] departments)
    {
        _logger.LogInformation("Seeding security incidents...");

        var incidents = new List<SecurityIncident>();
        var startDate = DateTime.UtcNow.AddYears(-2);
        var incidentCount = _random.Next(80, 121);

        // Create incidents in batches to ensure uniqueness using reflection to set IncidentNumber
        var createdIncidents = new List<SecurityIncident>();
        
        for (int i = 0; i < incidentCount; i++)
        {
            var reporter = users[_random.Next(users.Count)];
            var incidentDate = startDate.AddDays(_random.Next(0, 730));
            var incidentType = (SecurityIncidentType)_random.Next(1, 5); // PhysicalSecurity, Cybersecurity, PersonnelSecurity, InformationSecurity
            var category = GetCategoryForType(incidentType);
            var severity = (SecuritySeverity)_random.Next(1, 5); // Low, Medium, High, Critical

            var incident = SecurityIncident.Create(
                incidentType: incidentType,
                category: category,
                title: GetSecurityIncidentTitle(incidentType),
                description: GetSecurityIncidentDescription(incidentType, severity),
                severity: severity,
                incidentDateTime: incidentDate,
                location: GetRandomLocation(),
                reporterId: reporter.Id,
                createdBy: reporter.Name
            );

            // Override the IncidentNumber to ensure uniqueness during seeding
            var year = DateTime.UtcNow.Year;
            var uniqueNumber = $"SEC-{year}-SEED-{(i + 1):D4}";
            
            // Use reflection to set the private IncidentNumber property
            var incidentNumberProperty = typeof(SecurityIncident).GetProperty("IncidentNumber");
            incidentNumberProperty?.SetValue(incident, uniqueNumber);

            createdIncidents.Add(incident);
            
            _logger.LogInformation($"Created security incident {i + 1}/{incidentCount} with number {uniqueNumber}");
        }

        // Add all incidents at once
        _context.SecurityIncidents.AddRange(createdIncidents);
        await _context.SaveChangesAsync();

        // Now add threat assessments for some incidents
        foreach (var incident in createdIncidents)
        {
            if (_random.NextDouble() > 0.4) // 60% get threat assessments
            {
                var assessor = users.FirstOrDefault(u => u.Department == "Security") ?? users[_random.Next(users.Count)];
                var currentLevel = (ThreatLevel)_random.Next(1, 6); // Minimal, Low, Medium, High, Severe
                var previousLevel = (ThreatLevel)_random.Next(1, 6);

                var assessment = ThreatAssessment.Create(
                    securityIncidentId: incident.Id,
                    currentLevel: currentLevel,
                    previousLevel: previousLevel,
                    rationale: GetThreatAssessmentRationale(currentLevel, incident.IncidentType),
                    assessedById: assessor.Id,
                    createdBy: assessor.Name
                );

                _context.ThreatAssessments.Add(assessment);
            }
        }

        _logger.LogInformation($"Seeded {incidentCount} security incidents");
    }

    private Task SeedThreatIndicatorsAsync()
    {
        _logger.LogInformation("Seeding threat indicators...");

        var indicators = new List<ThreatIndicator>();
        var indicatorTypes = new[] { "IP", "Domain", "Hash", "Email", "URL", "FileHash", "Registry", "Process" };
        var threatTypes = new[] { "Malware", "Phishing", "Intrusion", "DDoS", "DataTheft", "Reconnaissance" };
        var sources = new[] { "SIEM", "Firewall", "IDS", "Antivirus", "Manual Analysis", "Threat Intel Feed" };
        
        // Track generated type-value combinations to avoid duplicates
        var generatedCombinations = new HashSet<string>();
        var targetCount = _random.Next(50, 101);
        var attempts = 0;
        var maxAttempts = targetCount * 3; // Allow more attempts to find unique combinations

        while (indicators.Count < targetCount && attempts < maxAttempts)
        {
            attempts++;
            var indicatorType = indicatorTypes[_random.Next(indicatorTypes.Length)];
            var indicatorValue = GenerateIndicatorValue(indicatorType);
            var combination = $"{indicatorType}:{indicatorValue}";
            
            // Skip if this combination already exists
            if (generatedCombinations.Contains(combination))
                continue;
                
            generatedCombinations.Add(combination);
            
            var threatType = threatTypes[_random.Next(threatTypes.Length)];
            var source = sources[_random.Next(sources.Length)];
            var confidence = _random.Next(60, 101);

            var indicator = ThreatIndicator.Create(
                indicatorType: indicatorType,
                indicatorValue: indicatorValue,
                threatType: threatType,
                confidence: confidence,
                source: source,
                createdBy: "system",
                description: GetIndicatorDescription(indicatorType, threatType)
            );

            indicators.Add(indicator);
        }

        _context.ThreatIndicators.AddRange(indicators);
        _logger.LogInformation($"Seeded {indicators.Count} threat indicators (attempted {attempts} times)");
        return Task.CompletedTask;
    }

    private Task SeedSecurityControlsAsync(List<User> users, string[] departments)
    {
        _logger.LogInformation("Seeding security controls...");

        var controls = new List<SecurityControl>();
        var controlTypes = Enum.GetValues<SecurityControlType>();
        var controlCategories = Enum.GetValues<SecurityControlCategory>();

        var controlNames = new[]
        {
            "Access Control System", "Surveillance Cameras", "Intrusion Detection", "Fire Suppression",
            "Security Badges", "Network Firewall", "Antivirus Software", "Backup Systems",
            "Emergency Lighting", "Security Guards", "Visitor Management", "Asset Tracking"
        };

        foreach (var controlName in controlNames)
        {
            var implementer = users[_random.Next(users.Count)];
            var controlType = controlTypes[_random.Next(controlTypes.Length)];
            var category = controlCategories[_random.Next(controlCategories.Length)];

            var control = SecurityControl.Create(
                name: controlName,
                description: GetControlDescription(controlName),
                controlType: controlType,
                category: category,
                implementedById: implementer.Id,
                createdBy: implementer.Name
            );

            controls.Add(control);
        }

        _context.SecurityControls.AddRange(controls);
        _logger.LogInformation($"Seeded {controls.Count} security controls");
        return Task.CompletedTask;
    }

    private SecurityIncidentCategory GetCategoryForType(SecurityIncidentType incidentType)
    {
        return incidentType switch
        {
            SecurityIncidentType.PhysicalSecurity => (SecurityIncidentCategory)_random.Next(101, 201), // Physical categories 101-200
            SecurityIncidentType.Cybersecurity => (SecurityIncidentCategory)_random.Next(201, 301), // Cyber categories 201-300
            SecurityIncidentType.PersonnelSecurity => (SecurityIncidentCategory)_random.Next(301, 401), // Personnel categories 301-400
            SecurityIncidentType.InformationSecurity => (SecurityIncidentCategory)_random.Next(401, 405), // Information categories 401-404
            _ => SecurityIncidentCategory.UnauthorizedAccess
        };
    }

    private string GetRandomLocation()
    {
        var locations = new[]
        {
            "Main Entrance", "Parking Lot A", "Parking Lot B", "Building 1", "Building 2",
            "Server Room", "Reception Area", "Loading Dock", "Perimeter Fence",
            "Security Office", "Executive Floor", "Data Center", "Warehouse"
        };
        return locations[_random.Next(locations.Length)];
    }

    private string GetSecurityIncidentTitle(SecurityIncidentType type)
    {
        return type switch
        {
            SecurityIncidentType.PhysicalSecurity => "Unauthorized Access Attempt",
            SecurityIncidentType.Cybersecurity => "Suspicious Network Activity",
            SecurityIncidentType.PersonnelSecurity => "Security Badge Misuse",
            SecurityIncidentType.InformationSecurity => "Data Access Violation",
            _ => "General Security Incident"
        };
    }

    private string GetSecurityIncidentDescription(SecurityIncidentType type, SecuritySeverity severity)
    {
        var descriptions = type switch
        {
            SecurityIncidentType.PhysicalSecurity => new[]
            {
                "Unauthorized person attempted to enter restricted area without proper credentials",
                "Tailgating incident observed at main security checkpoint",
                "Person found in secure area without escort or authorization"
            },
            SecurityIncidentType.Cybersecurity => new[]
            {
                "Unusual network traffic patterns detected from external source",
                "Suspected malware activity on employee workstation",
                "Failed login attempts exceeding security thresholds"
            },
            SecurityIncidentType.PersonnelSecurity => new[]
            {
                "Employee badge used outside of normal working hours",
                "Badge sharing incident between employees observed",
                "Lost or stolen security credentials reported"
            },
            _ => new[]
            {
                "Security incident requiring investigation and response",
                "Potential security breach detected through monitoring systems",
                "Security policy violation reported by staff member"
            }
        };

        var severityNote = severity >= SecuritySeverity.High ? " - High priority investigation required." : "";
        return descriptions[_random.Next(descriptions.Length)] + severityNote;
    }

    private string GetThreatAssessmentRationale(ThreatLevel currentLevel, SecurityIncidentType incidentType)
    {
        var baseRationale = incidentType switch
        {
            SecurityIncidentType.PhysicalSecurity => "Physical security threat assessment based on access control breach analysis",
            SecurityIncidentType.Cybersecurity => "Cybersecurity threat assessment based on network activity and potential impact",
            SecurityIncidentType.PersonnelSecurity => "Personnel security threat assessment based on insider risk evaluation",
            SecurityIncidentType.InformationSecurity => "Information security threat assessment based on data exposure risk",
            _ => "General security threat assessment based on incident characteristics"
        };

        var levelNote = currentLevel switch
        {
            ThreatLevel.Severe => " - Immediate action required due to severe threat level",
            ThreatLevel.High => " - High priority response needed",
            ThreatLevel.Medium => " - Moderate threat requiring monitoring",
            _ => " - Low to minimal threat level identified"
        };

        return baseRationale + levelNote;
    }

    private string GenerateIndicatorValue(string indicatorType)
    {
        var timestamp = DateTime.UtcNow.Ticks.ToString()[^6..]; // Last 6 digits for uniqueness
        
        return indicatorType switch
        {
            "IP" => $"{_random.Next(1, 255)}.{_random.Next(1, 255)}.{_random.Next(1, 255)}.{_random.Next(1, 255)}",
            "Domain" => $"suspicious-domain-{_random.Next(1000, 9999)}-{timestamp}.com",
            "Hash" => Guid.NewGuid().ToString("N")[..32],
            "Email" => $"threat{_random.Next(100, 999)}-{timestamp}@malicious-domain.com",
            "URL" => $"https://malicious-site-{_random.Next(100, 999)}-{timestamp}.com/payload",
            "FileHash" => Guid.NewGuid().ToString("N"),
            "Registry" => $"HKEY_LOCAL_MACHINE\\SOFTWARE\\Threat{_random.Next(100, 999)}-{timestamp}",
            "Process" => $"malicious_process_{_random.Next(100, 999)}-{timestamp}.exe",
            _ => $"indicator-{_random.Next(1000, 9999)}-{timestamp}"
        };
    }

    private string GetIndicatorDescription(string indicatorType, string threatType)
    {
        return $"{threatType} indicator of type {indicatorType} detected through security monitoring systems";
    }

    private string GetControlDescription(string controlName)
    {
        return controlName switch
        {
            "Access Control System" => "Electronic access control system managing entry to restricted areas",
            "Surveillance Cameras" => "CCTV surveillance system providing continuous monitoring",
            "Intrusion Detection" => "Security system detecting unauthorized access attempts",
            "Fire Suppression" => "Automated fire suppression system for critical areas",
            "Security Badges" => "Identity verification system using security badges",
            "Network Firewall" => "Network security system filtering and monitoring traffic",
            "Antivirus Software" => "Endpoint protection software detecting and preventing malware",
            "Backup Systems" => "Data backup and recovery systems ensuring business continuity",
            "Emergency Lighting" => "Emergency lighting system for safe evacuation",
            "Security Guards" => "Physical security personnel providing on-site protection",
            "Visitor Management" => "System managing and tracking visitor access",
            "Asset Tracking" => "System monitoring and tracking valuable assets",
            _ => "Security control system providing protection and monitoring capabilities"
        };
    }
}