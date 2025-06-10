using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Infrastructure.Persistence;
using Harmoni360.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class UserDataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserDataSeeder> _logger;
    private readonly IConfiguration _configuration;

    public UserDataSeeder(ApplicationDbContext context, ILogger<UserDataSeeder> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        // Check if we should re-seed users even if they exist
        var reSeedUsers = _configuration["DataSeeding:ReSeedUsers"] == "true";

        if (!reSeedUsers && await _context.Users.AnyAsync())
        {
            _logger.LogInformation("Users already exist and ReSeedUsers is false, skipping user seeding");
            return;
        }

        _logger.LogInformation("Starting user seeding...");

        // If re-seeding is enabled, clear existing users first
        if (reSeedUsers && await _context.Users.AnyAsync())
        {
            _logger.LogInformation("Clearing existing users for re-seeding...");
            _context.Users.RemoveRange(_context.Users);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Existing users cleared");
        }

        // Get all roles from database - HSSE expansion
        var superAdminRole = await _context.Roles.FirstAsync(r => r.Name == "SuperAdmin");
        var developerRole = await _context.Roles.FirstAsync(r => r.Name == "Developer");
        var adminRole = await _context.Roles.FirstAsync(r => r.Name == "Admin");
        
        // HSE Management Roles
        var incidentManagerRole = await _context.Roles.FirstAsync(r => r.Name == "IncidentManager");
        var riskManagerRole = await _context.Roles.FirstAsync(r => r.Name == "RiskManager");
        var ppeManagerRole = await _context.Roles.FirstAsync(r => r.Name == "PPEManager");
        var healthMonitorRole = await _context.Roles.FirstAsync(r => r.Name == "HealthMonitor");
        
        // Security Domain Roles - NEW
        var securityManagerRole = await _context.Roles.FirstAsync(r => r.Name == "SecurityManager");
        var securityOfficerRole = await _context.Roles.FirstAsync(r => r.Name == "SecurityOfficer");
        var complianceOfficerRole = await _context.Roles.FirstAsync(r => r.Name == "ComplianceOfficer");
        
        // General Access Roles
        var reporterRole = await _context.Roles.FirstAsync(r => r.Name == "Reporter");
        var viewerRole = await _context.Roles.FirstAsync(r => r.Name == "Viewer");

        // Hash demo passwords for production use
        var passwordHashService = new PasswordHashService();

        var users = new List<User>
        {
            // System administration users
            User.Create("superadmin@harmoni360.com", passwordHashService.HashPassword("SuperAdmin123!"), "Super Administrator", "SA001", "IT", "Super Administrator"),
            User.Create("developer@harmoni360.com", passwordHashService.HashPassword("Developer123!"), "System Developer", "DEV001", "IT", "Software Developer"),
            User.Create("admin@harmoni360.com", passwordHashService.HashPassword("Admin123!"), "System Administrator", "ADM001", "IT", "System Administrator"),
            
            // HSE (Health, Safety, Environment) specialized managers
            User.Create("incident.manager@harmoni360.com", passwordHashService.HashPassword("IncidentMgr123!"), "Incident Manager", "IM001", "Health & Safety", "Incident Management Specialist"),
            User.Create("risk.manager@harmoni360.com", passwordHashService.HashPassword("RiskMgr123!"), "Risk Manager", "RM001", "Health & Safety", "Risk Assessment Specialist"),
            User.Create("ppe.manager@harmoni360.com", passwordHashService.HashPassword("PPEMgr123!"), "PPE Manager", "PM001", "Health & Safety", "PPE Management Specialist"),
            User.Create("health.monitor@harmoni360.com", passwordHashService.HashPassword("HealthMon123!"), "Health Monitor", "HM001", "Health & Safety", "Health Monitoring Specialist"),
            
            // Security domain specialists - NEW for HSSE expansion
            User.Create("security.manager@harmoni360.com", passwordHashService.HashPassword("SecurityMgr123!"), "Security Manager", "SM001", "Security", "Physical & Information Security Manager"),
            User.Create("security.officer@harmoni360.com", passwordHashService.HashPassword("SecurityOfc123!"), "Security Officer", "SO001", "Security", "Security Operations Officer"),
            User.Create("compliance.officer@harmoni360.com", passwordHashService.HashPassword("ComplianceOfc123!"), "Compliance Officer", "CO001", "Compliance", "HSSE Compliance Specialist"),
            
            // Reporter and Viewer roles
            User.Create("reporter@harmoni360.com", passwordHashService.HashPassword("Reporter123!"), "Safety Reporter", "REP001", "Health & Safety", "Safety Data Analyst"),
            User.Create("viewer@harmoni360.com", passwordHashService.HashPassword("Viewer123!"), "Safety Viewer", "VW001", "General", "Safety Information Viewer"),
            
            // Legacy compatibility users (for existing demo purposes)
            User.Create("john.doe@bsj.sch.id", passwordHashService.HashPassword("Employee123!"), "John Doe", "EMP001", "Facilities", "Maintenance Supervisor"),
            User.Create("jane.smith@bsj.sch.id", passwordHashService.HashPassword("Employee123!"), "Jane Smith", "EMP002", "Academic", "Teacher")
        };

        // Assign roles to users - HSSE expansion
        users[0].AssignRole(superAdminRole);          // Super Administrator
        users[1].AssignRole(developerRole);           // System Developer
        users[2].AssignRole(adminRole);               // System Administrator
        
        // HSE specialized managers
        users[3].AssignRole(incidentManagerRole);     // Incident Manager
        users[4].AssignRole(riskManagerRole);         // Risk Manager
        users[5].AssignRole(ppeManagerRole);          // PPE Manager
        users[6].AssignRole(healthMonitorRole);       // Health Monitor
        
        // Security domain specialists - NEW
        users[7].AssignRole(securityManagerRole);     // Security Manager
        users[8].AssignRole(securityOfficerRole);     // Security Officer
        users[9].AssignRole(complianceOfficerRole);   // Compliance Officer
        
        // General access roles
        users[10].AssignRole(reporterRole);           // Safety Reporter
        users[11].AssignRole(viewerRole);             // Safety Viewer
        users[12].AssignRole(reporterRole);           // John Doe (legacy compatibility)
        users[13].AssignRole(viewerRole);             // Jane Smith (legacy compatibility)

        await _context.Users.AddRangeAsync(users);
        _logger.LogInformation("Seeded {Count} users", users.Count);
    }
}