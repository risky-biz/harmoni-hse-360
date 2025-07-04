using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class RoleDataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RoleDataSeeder> _logger;
    private readonly IConfiguration _configuration;

    public RoleDataSeeder(ApplicationDbContext context, ILogger<RoleDataSeeder> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        var roleCount = await _context.Roles.CountAsync();
        
        // Get seeding configuration
        var forceReseedValue = _configuration["DataSeeding:ForceReseed"];
        var forceReseed = string.Equals(forceReseedValue, "true", StringComparison.OrdinalIgnoreCase) || 
                         string.Equals(forceReseedValue, "True", StringComparison.OrdinalIgnoreCase) ||
                         (bool.TryParse(forceReseedValue, out var boolResult) && boolResult);
        
        _logger.LogInformation("Current role count: {RoleCount}, ForceReseed: {ForceReseed}", roleCount, forceReseed);

        if (!forceReseed && roleCount > 0)
        {
            _logger.LogInformation("Roles already exist, skipping seeding");
            return;
        }

        _logger.LogInformation("Starting role and permission seeding...");

        // Create roles using the new RoleType enum - HSSE (Health, Safety, Security, Environment) expansion
        var superAdminRole = Role.Create(RoleType.SuperAdmin, "SuperAdmin", "Super Administrator with complete system access including application settings", 1);
        var developerRole = Role.Create(RoleType.Developer, "Developer", "Developer with complete system access for development purposes", 2);
        var adminRole = Role.Create(RoleType.Admin, "Admin", "Administrator with access to all functional modules", 3);
        
        // HSE (Health, Safety, Environment) Management Roles
        var incidentManagerRole = Role.Create(RoleType.IncidentManager, "IncidentManager", "Manager with access only to Incident Management module", 4);
        var riskManagerRole = Role.Create(RoleType.RiskManager, "RiskManager", "Manager with access only to Risk Management module", 5);
        var ppeManagerRole = Role.Create(RoleType.PPEManager, "PPEManager", "Manager with access only to PPE Management module", 6);
        var healthMonitorRole = Role.Create(RoleType.HealthMonitor, "HealthMonitor", "Monitor with access only to Health Monitoring module", 7);
        
        // Security Domain Roles - NEW for HSSE expansion
        var securityManagerRole = Role.Create(RoleType.SecurityManager, "SecurityManager", "Manager with comprehensive access to ALL Security modules (Physical, Information, Personnel Security)", 8);
        var securityOfficerRole = Role.Create(RoleType.SecurityOfficer, "SecurityOfficer", "Officer with operational access to day-to-day Security operations and incident management", 9);
        var complianceOfficerRole = Role.Create(RoleType.ComplianceOfficer, "ComplianceOfficer", "Officer with enhanced access to HSSE compliance management across all domains", 10);
        
        // Work Permit Approval Roles - NEW for work permit workflow
        var safetyOfficerRole = Role.Create(RoleType.SafetyOfficer, "SafetyOfficer", "Officer with specialized access to safety operations and work permit approvals", 11);
        var departmentHeadRole = Role.Create(RoleType.DepartmentHead, "DepartmentHead", "Department manager with approval authority for departmental work permits", 12);
        var hotWorkSpecialistRole = Role.Create(RoleType.HotWorkSpecialist, "HotWorkSpecialist", "Specialist with authority to approve hot work permits and fire safety oversight", 13);
        var confinedSpaceSpecialistRole = Role.Create(RoleType.ConfinedSpaceSpecialist, "ConfinedSpaceSpecialist", "Specialist with authority to approve confined space entry permits", 14);
        var electricalSupervisorRole = Role.Create(RoleType.ElectricalSupervisor, "ElectricalSupervisor", "Supervisor with authority to approve electrical work permits and electrical safety", 15);
        var specialWorkSpecialistRole = Role.Create(RoleType.SpecialWorkSpecialist, "SpecialWorkSpecialist", "Specialist with authority to approve special/high-risk work permits", 16);
        var hseManagerRole = Role.Create(RoleType.HSEManager, "HSEManager", "Manager with comprehensive HSE authority and high-level approval permissions", 17);
        
        // General Access Roles
        var reporterRole = Role.Create(RoleType.Reporter, "Reporter", "User with read-only access to reporting across multiple modules", 18);
        var viewerRole = Role.Create(RoleType.Viewer, "Viewer", "User with read-only access to basic dashboard information", 19);

        await _context.Roles.AddRangeAsync(
            superAdminRole, developerRole, adminRole, 
            incidentManagerRole, riskManagerRole, ppeManagerRole, healthMonitorRole, 
            securityManagerRole, securityOfficerRole, complianceOfficerRole,
            safetyOfficerRole, departmentHeadRole, hotWorkSpecialistRole, confinedSpaceSpecialistRole, 
            electricalSupervisorRole, specialWorkSpecialistRole, hseManagerRole,
            reporterRole, viewerRole
        );
        
        _logger.LogInformation("Roles seeded successfully. Module permissions will be handled separately.");
    }
}