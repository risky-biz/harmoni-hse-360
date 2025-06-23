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
            User.Create("superadmin@harmoni360.com", passwordHashService.HashPassword("SuperAdmin123!"), "Super Administrator", "SA001", "IT", "Super Administrator", 
                "+1234567890", "Jakarta HQ", "IT-ADMIN", DateTime.UtcNow.AddYears(-5)),
            User.Create("developer@harmoni360.com", passwordHashService.HashPassword("Developer123!"), "Software Developer", "DEV001", "IT", "Developer User",
                "+1234567891", "Jakarta HQ", "IT-DEV", DateTime.UtcNow.AddYears(-3)),
            User.Create("admin@harmoni360.com", passwordHashService.HashPassword("Admin123!"), "System Administrator", "ADM001", "IT", "System Administrator",
                "+1234567892", "Jakarta HQ", "IT-ADMIN", DateTime.UtcNow.AddYears(-4))
        };
        
        // Set emergency contacts for essential users
        essentialAdminUsers[0].UpdateEmergencyContact("Emergency Contact 1", "+6281234567890");
        essentialAdminUsers[1].UpdateEmergencyContact("Emergency Contact 2", "+6281234567891");
        essentialAdminUsers[2].UpdateEmergencyContact("Emergency Contact 3", "+6281234567892");

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
        
        // Work Permit Approval Roles - NEW
        var safetyOfficerRole = await _context.Roles.FirstAsync(r => r.Name == "SafetyOfficer");
        var departmentHeadRole = await _context.Roles.FirstAsync(r => r.Name == "DepartmentHead");
        var hotWorkSpecialistRole = await _context.Roles.FirstAsync(r => r.Name == "HotWorkSpecialist");
        var confinedSpaceSpecialistRole = await _context.Roles.FirstAsync(r => r.Name == "ConfinedSpaceSpecialist");
        var electricalSupervisorRole = await _context.Roles.FirstAsync(r => r.Name == "ElectricalSupervisor");
        var specialWorkSpecialistRole = await _context.Roles.FirstAsync(r => r.Name == "SpecialWorkSpecialist");
        var hseManagerRole = await _context.Roles.FirstAsync(r => r.Name == "HSEManager");
        
        // General Access Roles
        var reporterRole = await _context.Roles.FirstAsync(r => r.Name == "Reporter");
        var viewerRole = await _context.Roles.FirstAsync(r => r.Name == "Viewer");

        // Hash demo passwords for production use
        var passwordHashService = new PasswordHashService();

        // Sample/Demo users for testing and demonstration
        var demoUsers = new List<User>
        {
            // HSE (Health, Safety, Environment) specialized managers
            User.Create("incident.manager@harmoni360.com", passwordHashService.HashPassword("IncidentMgr123!"), "Incident Manager", "IM001", "Health & Safety", "Incident Management Specialist",
                "+62812345001", "Site A - Production", "HSE-001", DateTime.UtcNow.AddYears(-2)),
            User.Create("risk.manager@harmoni360.com", passwordHashService.HashPassword("RiskMgr123!"), "Risk Manager", "RM001", "Health & Safety", "Risk Assessment Specialist",
                "+62812345002", "Site A - Production", "HSE-002", DateTime.UtcNow.AddYears(-3)),
            User.Create("ppe.manager@harmoni360.com", passwordHashService.HashPassword("PPEMgr123!"), "PPE Manager", "PM001", "Health & Safety", "PPE Management Specialist",
                "+62812345003", "Site B - Warehouse", "HSE-003", DateTime.UtcNow.AddYears(-1)),
            User.Create("health.monitor@harmoni360.com", passwordHashService.HashPassword("HealthMon123!"), "Health Monitor", "HM001", "Health & Safety", "Health Monitoring Specialist",
                "+62812345004", "Site C - Office", "HSE-004", DateTime.UtcNow.AddYears(-2)),
            
            // Security domain specialists - NEW for HSSE expansion
            User.Create("security.manager@harmoni360.com", passwordHashService.HashPassword("SecurityMgr123!"), "Security Manager", "SM001", "Security", "Physical & Information Security Manager",
                "+62812345005", "Jakarta HQ", "SEC-001", DateTime.UtcNow.AddYears(-5)),
            User.Create("security.officer@harmoni360.com", passwordHashService.HashPassword("SecurityOfc123!"), "Security Officer", "SO001", "Security", "Security Operations Officer",
                "+62812345006", "Site A - Production", "SEC-002", DateTime.UtcNow.AddYears(-2)),
            User.Create("compliance.officer@harmoni360.com", passwordHashService.HashPassword("ComplianceOfc123!"), "Compliance Officer", "CO001", "Compliance", "HSSE Compliance Specialist",
                "+62812345007", "Jakarta HQ", "COMP-001", DateTime.UtcNow.AddYears(-4)),
            
            // Work Permit Approval Specialists - NEW for work permit workflow
            User.Create("safety.officer@harmoni360.com", passwordHashService.HashPassword("SafetyOfc123!"), "Safety Officer", "SFO001", "Health & Safety", "Work Permit Safety Specialist",
                "+62812345008", "Site A - Production", "HSE-005", DateTime.UtcNow.AddYears(-3)),
            User.Create("department.head@harmoni360.com", passwordHashService.HashPassword("DeptHead123!"), "Department Head", "DH001", "Operations", "Department Manager & Approver",
                "+62812345009", "Site A - Production", "OPS-001", DateTime.UtcNow.AddYears(-6)),
            User.Create("hotwork.specialist@harmoni360.com", passwordHashService.HashPassword("HotWork123!"), "Hot Work Specialist", "HWS001", "Health & Safety", "Hot Work & Fire Safety Expert",
                "+62812345010", "Site B - Warehouse", "HSE-006", DateTime.UtcNow.AddYears(-4)),
            User.Create("confinedspace.specialist@harmoni360.com", passwordHashService.HashPassword("ConfinedSpace123!"), "Confined Space Specialist", "CSS001", "Health & Safety", "Confined Space Entry Expert",
                "+62812345011", "Site A - Production", "HSE-007", DateTime.UtcNow.AddYears(-3)),
            User.Create("electrical.supervisor@harmoni360.com", passwordHashService.HashPassword("ElecSup123!"), "Electrical Supervisor", "ES001", "Maintenance", "Electrical Safety Supervisor",
                "+62812345012", "Site A - Production", "MAINT-001", DateTime.UtcNow.AddYears(-5)),
            User.Create("specialwork.specialist@harmoni360.com", passwordHashService.HashPassword("SpecialWork123!"), "Special Work Specialist", "SWS001", "Health & Safety", "High-Risk Work Specialist",
                "+62812345013", "Site C - Office", "HSE-008", DateTime.UtcNow.AddYears(-2)),
            User.Create("hse.manager@harmoni360.com", passwordHashService.HashPassword("HSEMgr123!"), "HSE Manager", "HSEM001", "Health & Safety", "Health Safety Environment Manager",
                "+62812345014", "Jakarta HQ", "HSE-009", DateTime.UtcNow.AddYears(-7)),
            
            // Reporter and Viewer roles
            User.Create("reporter@harmoni360.com", passwordHashService.HashPassword("Reporter123!"), "Safety Reporter", "REP001", "Health & Safety", "Safety Data Analyst",
                "+62812345015", "Jakarta HQ", "HSE-010", DateTime.UtcNow.AddYears(-1)),
            User.Create("viewer@harmoni360.com", passwordHashService.HashPassword("Viewer123!"), "Safety Viewer", "VW001", "General", "Safety Information Viewer",
                "+62812345016", "Jakarta HQ", "GEN-001", DateTime.UtcNow.AddMonths(-6)),
            
            // Legacy compatibility users (for existing demo purposes)
            User.Create("john.doe@bsj.sch.id", passwordHashService.HashPassword("Employee123!"), "John Doe", "EMP001", "Facilities", "Maintenance Supervisor",
                "+62812345017", "Site A - Production", "FAC-001", DateTime.UtcNow.AddYears(-3)),
            User.Create("jane.smith@bsj.sch.id", passwordHashService.HashPassword("Employee123!"), "Jane Smith", "EMP002", "Academic", "Teacher",
                "+62812345018", "Site C - Office", "ACAD-001", DateTime.UtcNow.AddYears(-2))
        };
        
        // Set emergency contacts and supervisors for demo users
        for (int i = 0; i < demoUsers.Count; i++)
        {
            demoUsers[i].UpdateEmergencyContact($"Emergency Contact {i + 1}", $"+628987654{i:D3}");
            
            // Set supervisors (department heads report to HSE Manager)
            if (i < 14) // Most users report to department head
            {
                demoUsers[i].UpdateSupervisor("DH001");
            }
        }
        
        // Update supervisor hierarchy
        demoUsers[8].UpdateSupervisor("HSEM001"); // Department Head reports to HSE Manager
        demoUsers[13].UpdateSupervisor(null); // HSE Manager has no supervisor in demo data

        // Assign roles to demo users
        demoUsers[0].AssignRole(incidentManagerRole);     // incident.manager@harmoni360.com
        demoUsers[1].AssignRole(riskManagerRole);         // risk.manager@harmoni360.com
        demoUsers[2].AssignRole(ppeManagerRole);          // ppe.manager@harmoni360.com
        demoUsers[3].AssignRole(healthMonitorRole);       // health.monitor@harmoni360.com
        
        // Security domain specialists
        demoUsers[4].AssignRole(securityManagerRole);     // security.manager@harmoni360.com
        demoUsers[5].AssignRole(securityOfficerRole);     // security.officer@harmoni360.com
        demoUsers[6].AssignRole(complianceOfficerRole);   // compliance.officer@harmoni360.com
        
        // Work permit approval specialists
        demoUsers[7].AssignRole(safetyOfficerRole);       // safety.officer@harmoni360.com
        demoUsers[8].AssignRole(departmentHeadRole);      // department.head@harmoni360.com
        demoUsers[9].AssignRole(hotWorkSpecialistRole);   // hotwork.specialist@harmoni360.com
        demoUsers[10].AssignRole(confinedSpaceSpecialistRole); // confinedspace.specialist@harmoni360.com
        demoUsers[11].AssignRole(electricalSupervisorRole);    // electrical.supervisor@harmoni360.com
        demoUsers[12].AssignRole(specialWorkSpecialistRole);   // specialwork.specialist@harmoni360.com
        demoUsers[13].AssignRole(hseManagerRole);         // hse.manager@harmoni360.com
        
        // General access roles
        demoUsers[14].AssignRole(reporterRole);           // reporter@harmoni360.com
        demoUsers[15].AssignRole(viewerRole);             // viewer@harmoni360.com
        demoUsers[16].AssignRole(reporterRole);           // john.doe@bsj.sch.id
        demoUsers[17].AssignRole(viewerRole);             // jane.smith@bsj.sch.id

        await _context.Users.AddRangeAsync(demoUsers);
        _logger.LogInformation("Seeded {Count} sample user accounts", demoUsers.Count);
    }
}