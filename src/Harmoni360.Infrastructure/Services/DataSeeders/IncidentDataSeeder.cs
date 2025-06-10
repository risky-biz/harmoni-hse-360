using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.ValueObjects;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class IncidentDataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<IncidentDataSeeder> _logger;
    private readonly IConfiguration _configuration;

    public IncidentDataSeeder(ApplicationDbContext context, ILogger<IncidentDataSeeder> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        // Check if we should re-seed incidents even if they exist
        var reSeedIncidents = _configuration["DataSeeding:ReSeedIncidents"] == "true";

        if (!reSeedIncidents && await _context.Incidents.AnyAsync())
        {
            _logger.LogInformation("Incidents already exist and ReSeedIncidents is false, skipping incident seeding");
            return;
        }

        _logger.LogInformation("Starting incident seeding...");

        // If re-seeding is enabled, clear existing incidents first
        if (reSeedIncidents && await _context.Incidents.AnyAsync())
        {
            _logger.LogInformation("Clearing existing incidents for re-seeding...");
            _context.Incidents.RemoveRange(_context.Incidents);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Existing incidents cleared");
        }

        // Get users for incident reporting
        var incidentManager = await _context.Users.FirstOrDefaultAsync(u => u.Email == "incident.manager@harmoni360.com");
        var employee1 = await _context.Users.FirstOrDefaultAsync(u => u.Email == "john.doe@bsj.sch.id");
        var employee2 = await _context.Users.FirstOrDefaultAsync(u => u.Email == "jane.smith@bsj.sch.id");
        
        // Check if users exist
        if (incidentManager == null || employee1 == null || employee2 == null)
        {
            _logger.LogWarning("Required users not found for incident seeding. Skipping incident seeding.");
            return;
        }

        var incidents = new List<Incident>
        {
            // Critical incident - Fire alarm malfunction
            Incident.Create(
                "Fire alarm system malfunction in East Wing",
                "False alarm triggered at 11:45 AM causing evacuation of entire East Wing building. System showed smoke detection in Room 304 but no smoke was present. Maintenance team investigated and found faulty sensor.",
                IncidentSeverity.Serious,
                DateTime.UtcNow.AddDays(-5).AddHours(11).AddMinutes(45),
                "East Wing - 3rd Floor, Room 304",
                "Emily Chen",
                "emily.chen@bsj.sch.id",
                "Facilities Management",
                GeoLocation.Create(-6.2088, 106.8456) // BSJ coordinates
            ),

            // Moderate incident - Chemistry lab accident
            Incident.Create(
                "Student minor burn injury in Chemistry Lab",
                "Grade 11 student suffered minor burn on left hand during chemistry practical. Student was heating solution and accidentally touched hot beaker. First aid administered immediately.",
                IncidentSeverity.Moderate,
                DateTime.UtcNow.AddDays(-3).AddHours(14).AddMinutes(30),
                "Science Building - Chemistry Lab Room 205",
                "Dr. Sarah Johnson",
                "sarah.johnson@bsj.sch.id",
                "Science Department",
                GeoLocation.Create(-6.2090, 106.8458)
            ),

            // Minor incident - Slip and fall
            Incident.Create(
                "Slip and fall incident near main entrance",
                "Staff member slipped on wet floor near main entrance during rainy weather. Warning sign was not properly placed. No serious injury, but staff member experienced minor bruising.",
                IncidentSeverity.Minor,
                DateTime.UtcNow.AddDays(-7).AddHours(9).AddMinutes(15),
                "Main Building - Ground Floor Entrance",
                "David Wilson",
                "david.wilson@bsj.sch.id",
                "Administration",
                GeoLocation.Create(-6.2085, 106.8455)
            ),

            // Critical incident - Playground equipment failure
            Incident.Create(
                "Playground equipment structural failure",
                "Swing set chain broke while student was using it. Student fell but landed safely on rubber matting. Equipment immediately cordoned off. Inspection revealed metal fatigue in chain links.",
                IncidentSeverity.Critical,
                DateTime.UtcNow.AddDays(-2).AddHours(10).AddMinutes(30),
                "Primary School Playground - Area B",
                "Lisa Martinez",
                "lisa.martinez@bsj.sch.id",
                "Primary School",
                GeoLocation.Create(-6.2092, 106.8460)
            ),

            // Moderate incident - Food poisoning
            Incident.Create(
                "Multiple students report food poisoning symptoms",
                "12 students from Grade 9 reported nausea and stomach pain after lunch. All had consumed chicken sandwich from cafeteria. Health office provided treatment, parents notified.",
                IncidentSeverity.Serious,
                DateTime.UtcNow.AddDays(-10).AddHours(13).AddMinutes(45),
                "School Cafeteria",
                "Nurse Patricia",
                "patricia.nurse@bsj.sch.id",
                "Health Services",
                GeoLocation.Create(-6.2087, 106.8457)
            ),

            // Minor incident - Sports injury
            Incident.Create(
                "Student ankle sprain during PE class",
                "Grade 8 student twisted ankle during basketball game in PE class. Ice pack applied, student sent to health office. Parents contacted for pickup.",
                IncidentSeverity.Minor,
                DateTime.UtcNow.AddDays(-4).AddHours(15).AddMinutes(20),
                "Sports Hall - Basketball Court 2",
                "Coach Michael Brown",
                "michael.brown@bsj.sch.id",
                "Physical Education",
                GeoLocation.Create(-6.2089, 106.8459)
            )
        };

        // Update incident statuses and add additional details
        incidents[0].UpdateStatus(IncidentStatus.AwaitingAction);
        incidents[0].UpdateInjuryDetails(InjuryType.None, false, false);
        incidents[0].AddWitnessInformation("Multiple staff members from East Wing");
        incidents[0].RecordImmediateActions("Building evacuated, Fire department notified, Maintenance team dispatched");

        incidents[1].UpdateStatus(IncidentStatus.UnderInvestigation);
        incidents[1].UpdateInjuryDetails(InjuryType.Burn, true, false);
        incidents[1].AddWitnessInformation("Lab assistant James Wong, 3 other students");
        incidents[1].RecordImmediateActions("First aid administered, Cold water applied to burn, Parents notified");

        incidents[2].UpdateStatus(IncidentStatus.Resolved);
        incidents[2].UpdateInjuryDetails(InjuryType.Bruise, false, false);
        incidents[2].AddWitnessInformation("Security guard on duty");
        incidents[2].RecordImmediateActions("Area dried and warning signs placed, Incident report filed");

        incidents[3].UpdateStatus(IncidentStatus.AwaitingAction);
        incidents[3].UpdateInjuryDetails(InjuryType.None, true, false);
        incidents[3].AddWitnessInformation("PE Teacher, 5 students");
        incidents[3].RecordImmediateActions("Equipment cordoned off, All playground equipment scheduled for inspection");

        incidents[4].UpdateStatus(IncidentStatus.Closed);
        incidents[4].UpdateInjuryDetails(InjuryType.Other, true, false);
        incidents[4].AddWitnessInformation("Cafeteria staff");
        incidents[4].RecordImmediateActions("Food samples collected, Kitchen inspection conducted, Menu item removed");

        incidents[5].UpdateStatus(IncidentStatus.Resolved);
        incidents[5].UpdateInjuryDetails(InjuryType.Sprain, true, false);
        incidents[5].AddWitnessInformation("Other students in PE class");
        incidents[5].RecordImmediateActions("RICE protocol applied, Student sent to health office");

        // Add some corrective actions to resolved/closed incidents
        var correctiveAction1 = CorrectiveAction.Create(
            incidents[0].Id,
            "Replace all faulty smoke sensors in East Wing",
            "Maintenance",
            DateTime.UtcNow.AddDays(5),
            ActionPriority.High,
            "System"
        );
        incidents[0].AddCorrectiveAction(correctiveAction1);

        var correctiveAction2 = CorrectiveAction.Create(
            incidents[1].Id,
            "Review and update chemistry lab safety protocols",
            "Science Department",
            DateTime.UtcNow.AddDays(7),
            ActionPriority.Medium,
            "System"
        );
        incidents[1].AddCorrectiveAction(correctiveAction2);

        var correctiveAction3 = CorrectiveAction.Create(
            incidents[2].Id,
            "Install additional wet floor warning signs at all entrances",
            "Facilities",
            DateTime.UtcNow.AddDays(3),
            ActionPriority.Medium,
            "System"
        );
        correctiveAction3.MarkAsCompleted(DateTime.UtcNow.AddDays(-1), "20 new warning signs installed");
        incidents[2].AddCorrectiveAction(correctiveAction3);

        await _context.Incidents.AddRangeAsync(incidents);
        _logger.LogInformation("Seeded {Count} incidents", incidents.Count);
    }
}