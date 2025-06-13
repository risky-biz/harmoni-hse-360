using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities.Security;
using Harmoni360.Domain.Enums;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class SecurityDataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SecurityDataSeeder> _logger;
    private readonly IConfiguration _configuration;

    public SecurityDataSeeder(ApplicationDbContext context, ILogger<SecurityDataSeeder> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        // Check if we should re-seed security data even if it exists
        var forceReseedValue = _configuration["DataSeeding:ForceReseed"];
        var forceReseed = string.Equals(forceReseedValue, "true", StringComparison.OrdinalIgnoreCase) || 
                         string.Equals(forceReseedValue, "True", StringComparison.OrdinalIgnoreCase) ||
                         (bool.TryParse(forceReseedValue, out var boolResult) && boolResult);
        var reSeedSecurityData = _configuration["DataSeeding:ReSeedSecurityData"] == "true";

        _logger.LogInformation("SecurityDataSeeder - ForceReseed: {ForceReseed}, ReSeedSecurityData: {ReSeed}", forceReseed, reSeedSecurityData);

        if (!forceReseed && !reSeedSecurityData && await _context.SecurityIncidents.AnyAsync())
        {
            _logger.LogInformation("Security data already exists and neither ForceReseed nor ReSeedSecurityData is enabled, skipping security data seeding");
            return;
        }

        _logger.LogInformation("Starting security data seeding...");

        // If re-seeding is enabled, clear existing data first
        if ((reSeedSecurityData || forceReseed) && await _context.SecurityIncidents.AnyAsync())
        {
            _logger.LogInformation("Clearing existing security data for re-seeding...");
            _context.SecurityIncidents.RemoveRange(_context.SecurityIncidents);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Existing security data cleared");
        }

        await SeedSecurityIncidentsAsync();
        await SeedThreatAssessmentsAsync();
        await SeedSecurityControlsAsync();
    }

    private async Task SeedSecurityIncidentsAsync()
    {
        _logger.LogInformation("Starting security incident seeding...");

        // Get users for incident reporting
        var securityManager = await _context.Users.FirstOrDefaultAsync(u => u.Email == "security.manager@harmoni360.com");
        var securityOfficer = await _context.Users.FirstOrDefaultAsync(u => u.Email == "security.officer@harmoni360.com");
        
        if (securityManager == null || securityOfficer == null)
        {
            _logger.LogWarning("Required security users not found for security incident seeding. Skipping security incident seeding.");
            return;
        }

        var securityIncidents = new List<SecurityIncident>();

        // Create incidents with different creation times to ensure unique incident numbers
        var incidentTemplates = new[]
        {
            new { 
                Type = SecurityIncidentType.PhysicalSecurity, 
                Category = SecurityIncidentCategory.UnauthorizedAccess,
                Title = "Unauthorized access attempt to server room",
                Description = "Individual without proper authorization attempted to access server room using tailgating method. Security guard intervened and escorted person out.",
                Severity = SecuritySeverity.High,
                IncidentTime = DateTime.UtcNow.AddDays(-3).AddHours(14).AddMinutes(30),
                Location = "IT Building - Server Room Level B1",
                Reporter = securityOfficer.Id
            },
            new { 
                Type = SecurityIncidentType.Cybersecurity, 
                Category = SecurityIncidentCategory.PhishingAttempt,
                Title = "Attempted phishing attack on staff email",
                Description = "Multiple staff members received suspicious emails requesting login credentials. IT team blocked sender and issued security alert.",
                Severity = SecuritySeverity.High,
                IncidentTime = DateTime.UtcNow.AddDays(-2).AddHours(10).AddMinutes(45),
                Location = "Email System - Multiple Departments",
                Reporter = securityManager.Id
            },
            new { 
                Type = SecurityIncidentType.PersonnelSecurity, 
                Category = SecurityIncidentCategory.CredentialMisuse,
                Title = "Lost access card reported",
                Description = "Staff member reported lost access card. Card immediately deactivated and replacement issued. No unauthorized usage detected.",
                Severity = SecuritySeverity.Low,
                IncidentTime = DateTime.UtcNow.AddDays(-4).AddHours(16).AddMinutes(30),
                Location = "Administration Building",
                Reporter = securityOfficer.Id
            }
        };

        // Create incidents with sufficient time gaps to ensure unique incident numbers
        for (int i = 0; i < incidentTemplates.Length; i++)
        {
            var template = incidentTemplates[i];
            
            // Wait to ensure different timestamp for incident number generation
            if (i > 0)
            {
                await Task.Delay(1200); // Wait over 1 second between creations
            }
            
            var incident = SecurityIncident.Create(
                template.Type,
                template.Category,
                template.Title,
                template.Description,
                template.Severity,
                template.IncidentTime,
                template.Location,
                template.Reporter,
                "system"
            );
            
            securityIncidents.Add(incident);
        }

        // Update incident statuses and details using available methods
        securityIncidents[0].AssignInvestigator(securityManager.Id, "system");
        securityIncidents[0].RecordContainment("Access logs reviewed, additional security measures implemented", DateTime.UtcNow.AddDays(-2), "system");

        securityIncidents[1].RecordContainment("Email blocked, staff training conducted, monitoring increased", DateTime.UtcNow.AddDays(-1), "system");

        securityIncidents[2].ResolveIncident("Standard access card replacement procedure", DateTime.UtcNow.AddDays(-3), "system");
        securityIncidents[2].AddResponse(SecurityResponseType.LessonsLearned, "Implemented stronger access card tracking procedures and improved staff training on access card security protocols", DateTime.UtcNow.AddDays(-2), securityManager.Id);
        securityIncidents[2].CloseIncident("system");

        await _context.SecurityIncidents.AddRangeAsync(securityIncidents);
        _logger.LogInformation("Seeded {Count} security incidents", securityIncidents.Count);
    }

    private async Task SeedThreatAssessmentsAsync()
    {
        _logger.LogInformation("Starting threat assessment seeding...");

        // Get security incidents to create threat assessments for
        var securityIncidents = await _context.SecurityIncidents.ToListAsync();
        var threatAssessments = new List<ThreatAssessment>();

        // Get security manager for assessments
        var securityManager = await _context.Users.FirstOrDefaultAsync(u => u.Email == "security.manager@harmoni360.com");
        if (securityManager == null) return;

        foreach (var incident in securityIncidents) // Create assessments for all incidents
        {
            var (currentLevel, rationale) = incident.Title switch
            {
                "Unauthorized access attempt to server room" => (ThreatLevel.High, "Individual demonstrated knowledge of building layout and targeted high-value area. Potential corporate espionage or data theft attempt."),
                "Attempted phishing attack on staff email" => (ThreatLevel.Severe, "Sophisticated phishing campaign specifically targeting educational institutions. Organized cybercriminal group attempting credential harvesting."),
                "Lost access card reported" => (ThreatLevel.Low, "Standard security protocol for lost access cards. No evidence of malicious intent or unauthorized access."),
                _ => (ThreatLevel.Low, "Standard threat assessment")
            };

            var assessment = ThreatAssessment.Create(
                incident.Id,
                currentLevel,
                ThreatLevel.Low, // Previous level
                rationale,
                securityManager.Id,
                "system"
            );

            threatAssessments.Add(assessment);
        }

        await _context.ThreatAssessments.AddRangeAsync(threatAssessments);
        _logger.LogInformation("Seeded {Count} threat assessments", threatAssessments.Count);
    }

    private async Task SeedSecurityControlsAsync()
    {
        _logger.LogInformation("Starting security controls seeding...");

        // Get a user to assign as implementer
        var systemUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@harmoni360.com");
        if (systemUser == null) return;

        var securityControls = new List<SecurityControl>
        {
            // Physical Security Controls
            SecurityControl.Create(
                "Access Card System",
                "Electronic access control system for building entry",
                SecurityControlType.Preventive,
                SecurityControlCategory.Physical,
                systemUser.Id,
                "system"
            ),

            SecurityControl.Create(
                "CCTV Surveillance System",
                "Comprehensive video surveillance covering all public areas",
                SecurityControlType.Detective,
                SecurityControlCategory.Physical,
                systemUser.Id,
                "system"
            ),

            SecurityControl.Create(
                "Visitor Management System",
                "Digital check-in system for all campus visitors",
                SecurityControlType.Preventive,
                SecurityControlCategory.Administrative,
                systemUser.Id,
                "system"
            ),

            // Technical Security Controls
            SecurityControl.Create(
                "Network Firewall",
                "Enterprise firewall protecting network perimeter",
                SecurityControlType.Preventive,
                SecurityControlCategory.Technical,
                systemUser.Id,
                "system"
            ),

            SecurityControl.Create(
                "Email Security Gateway",
                "Anti-phishing and malware protection for email communications",
                SecurityControlType.Detective,
                SecurityControlCategory.Technical,
                systemUser.Id,
                "system"
            ),

            SecurityControl.Create(
                "Data Backup System",
                "Automated backup system for critical data and systems",
                SecurityControlType.Corrective,
                SecurityControlCategory.Technical,
                systemUser.Id,
                "system"
            ),

            // Administrative Security Controls
            SecurityControl.Create(
                "Security Awareness Training",
                "Regular cybersecurity training for all staff and students",
                SecurityControlType.Preventive,
                SecurityControlCategory.Administrative,
                systemUser.Id,
                "system"
            ),

            SecurityControl.Create(
                "Incident Response Procedure",
                "Documented procedures for security incident response",
                SecurityControlType.Corrective,
                SecurityControlCategory.Administrative,
                systemUser.Id,
                "system"
            )
        };

        // Transition controls through proper states
        foreach (var control in securityControls)
        {
            control.StartImplementation("system");
            control.CompleteImplementation("system");
        }

        await _context.SecurityControls.AddRangeAsync(securityControls);
        _logger.LogInformation("Seeded {Count} security controls", securityControls.Count);
    }
}