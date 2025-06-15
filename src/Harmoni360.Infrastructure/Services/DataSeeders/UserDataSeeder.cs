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
        // Get seeding configuration
        var seedUserAccounts = _configuration["DataSeeding:Categories:UserAccounts"] == "true";

        // Seed essential admin users (always required)
        await SeedEssentialAdminUsersAsync();

        // Seed demo/sample user accounts if enabled
        if (seedUserAccounts)
        {
            await SeedSampleUserAccountsAsync();
        }
    }

    public async Task SeedEssentialAdminUsersAsync(bool forceReseed = false)
    {
        // Skip existence check if forceReseed is true
        if (!forceReseed)
        {
            // Check if essential admin users already exist
            var hasEssentialAdmins = await _context.Users.AnyAsync(u => 
                u.Email == "superadmin@harmoni360.com" || 
                u.Email == "developer@harmoni360.com" || 
                u.Email == "admin@harmoni360.com");

            if (hasEssentialAdmins)
            {
                _logger.LogInformation("Essential admin users already exist, skipping");
                return;
            }
        }

        _logger.LogInformation("Seeding essential admin users...");

        // Get required roles
        var superAdminRole = await _context.Roles.FirstAsync(r => r.Name == "SuperAdmin");
        var developerRole = await _context.Roles.FirstAsync(r => r.Name == "Developer");
        var adminRole = await _context.Roles.FirstAsync(r => r.Name == "Admin");

        // Hash passwords for production use
        var passwordHashService = new PasswordHashService();

        // Essential admin users (ALWAYS seeded for production initial setup)
        var essentialAdminUsers = new List<User>
        {
            User.Create("superadmin@harmoni360.com", passwordHashService.HashPassword("SuperAdmin123!"), "Super Administrator", "SA001", "IT", "Super Administrator"),
            User.Create("developer@harmoni360.com", passwordHashService.HashPassword("Developer123!"), "Software Developer", "DEV001", "IT", "Developer User"),
            User.Create("admin@harmoni360.com", passwordHashService.HashPassword("Admin123!"), "System Administrator", "ADM001", "IT", "System Administrator")
        };

        // Assign roles to essential admin users
        essentialAdminUsers[0].AssignRole(superAdminRole);   // superadmin@harmoni360.com
        essentialAdminUsers[1].AssignRole(developerRole);    // developer@harmoni360.com  
        essentialAdminUsers[2].AssignRole(adminRole);        // admin@harmoni360.com

        await _context.Users.AddRangeAsync(essentialAdminUsers);
        _logger.LogInformation("Seeded {Count} essential admin users", essentialAdminUsers.Count);
    }

    public async Task SeedSampleUserAccountsAsync(bool forceReseed = false)
    {
        // Skip existence check if forceReseed is true
        if (!forceReseed)
        {
            // Check if demo users already exist
            var hasDemoUsers = await _context.Users.AnyAsync(u => 
                u.Email.Contains("incident.manager") || 
                u.Email.Contains("risk.manager") ||
                u.Email.Contains("ppe.manager"));

            if (hasDemoUsers)
            {
                _logger.LogInformation("Sample user accounts already exist, skipping");
                return;
            }
        }

        _logger.LogInformation("Seeding sample user accounts...");

        // Get all roles from database - HSSE expansion
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

        // Sample/Demo users for testing and demonstration
        var demoUsers = new List<User>
        {
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

        // Assign roles to demo users
        demoUsers[0].AssignRole(incidentManagerRole);     // incident.manager@harmoni360.com
        demoUsers[1].AssignRole(riskManagerRole);         // risk.manager@harmoni360.com
        demoUsers[2].AssignRole(ppeManagerRole);          // ppe.manager@harmoni360.com
        demoUsers[3].AssignRole(healthMonitorRole);       // health.monitor@harmoni360.com
        
        // Security domain specialists
        demoUsers[4].AssignRole(securityManagerRole);     // security.manager@harmoni360.com
        demoUsers[5].AssignRole(securityOfficerRole);     // security.officer@harmoni360.com
        demoUsers[6].AssignRole(complianceOfficerRole);   // compliance.officer@harmoni360.com
        
        // General access roles
        demoUsers[7].AssignRole(reporterRole);            // reporter@harmoni360.com
        demoUsers[8].AssignRole(viewerRole);              // viewer@harmoni360.com
        demoUsers[9].AssignRole(reporterRole);            // john.doe@bsj.sch.id
        demoUsers[10].AssignRole(viewerRole);             // jane.smith@bsj.sch.id

        await _context.Users.AddRangeAsync(demoUsers);
        _logger.LogInformation("Seeded {Count} sample user accounts", demoUsers.Count);
    }
}