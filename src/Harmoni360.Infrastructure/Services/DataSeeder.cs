using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.ValueObjects;
using Harmoni360.Domain.Enums;
using Harmoni360.Domain.Authorization;
using Harmoni360.Domain.Constants;
using Harmoni360.Domain.Common;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services;

public interface IDataSeeder
{
    Task SeedAsync();
}

public class DataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataSeeder> _logger;
    private readonly IConfiguration _configuration;

    public DataSeeder(ApplicationDbContext context, ILogger<DataSeeder> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        try
        {
            await SeedRolesAndPermissionsAsync();
            await _context.SaveChangesAsync(); // Save roles first

            await SeedModulePermissionsAsync();
            await _context.SaveChangesAsync(); // Save module permissions

            await SeedRoleModulePermissionsAsync();
            await _context.SaveChangesAsync(); // Save role-module permission associations

            await SeedUsersAsync();
            await _context.SaveChangesAsync(); // Save users

            // Seed PPE management data if enabled in configuration
            var seedPPEData = _configuration["DataSeeding:SeedPPEData"] != "false";
            if (seedPPEData)
            {
                await SeedPPEDataAsync();
                await _context.SaveChangesAsync(); // Save PPE data
            }

            // Seed incidents if enabled in configuration
            var seedIncidents = _configuration["DataSeeding:SeedIncidents"] != "false";
            if (seedIncidents)
            {
                await SeedIncidentsAsync();
                await _context.SaveChangesAsync(); // Save incidents
            }

            // Seed PPE items if enabled in configuration
            var seedPPEItems = _configuration["DataSeeding:SeedPPEItems"] != "false";
            if (seedPPEItems)
            {
                await SeedPPEItemsAsync();
                await _context.SaveChangesAsync(); // Save PPE items
            }

            // Seed hazards if enabled in configuration
            var seedHazards = _configuration["DataSeeding:SeedHazards"] != "false";
            if (seedHazards)
            {
                await SeedHazardsAsync();
                await _context.SaveChangesAsync(); // Save hazards
            }

            // Seed health data if enabled in configuration
            var seedHealthData = _configuration["DataSeeding:SeedHealthData"] != "false";
            if (seedHealthData)
            {
                await SeedHealthDataAsync();
                await _context.SaveChangesAsync(); // Save health data
            }

            // Seed extended data if enabled in configuration
            var seedExtendedData = _configuration["DataSeeding:SeedExtendedData"] != "false";
            if (seedExtendedData)
            {
                await SeedExtendedDataAsync();
                await _context.SaveChangesAsync(); // Save extended data
            }

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding database");
            throw;
        }
    }

    private async Task SeedRolesAndPermissionsAsync()
    {
        var roleCount = await _context.Roles.CountAsync();
        _logger.LogInformation("Current role count: {RoleCount}", roleCount);

        if (roleCount > 0)
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
        
        // General Access Roles
        var reporterRole = Role.Create(RoleType.Reporter, "Reporter", "User with read-only access to reporting across multiple modules", 11);
        var viewerRole = Role.Create(RoleType.Viewer, "Viewer", "User with read-only access to basic dashboard information", 12);

        await _context.Roles.AddRangeAsync(superAdminRole, developerRole, adminRole, incidentManagerRole, riskManagerRole, ppeManagerRole, healthMonitorRole, securityManagerRole, securityOfficerRole, complianceOfficerRole, reporterRole, viewerRole);
        _logger.LogInformation("Roles seeded successfully. Module permissions will be handled separately.");
    }

    private async Task SeedModulePermissionsAsync()
    {
        var modulePermissionCount = await _context.ModulePermissions.CountAsync();
        _logger.LogInformation("Current module permission count: {ModulePermissionCount}", modulePermissionCount);

        var reSeedModulePermissions = bool.Parse(_configuration["DataSeeding:ReSeedModulePermissions"] ?? "false");
        
        if (modulePermissionCount > 0 && !reSeedModulePermissions)
        {
            _logger.LogInformation("Module permissions already exist, skipping seeding");
            return;
        }
        
        if (reSeedModulePermissions && modulePermissionCount > 0)
        {
            _logger.LogInformation("ReSeedModulePermissions is enabled, clearing existing module permissions...");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"RoleModulePermissions\"");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"ModulePermissions\"");
            _logger.LogInformation("Existing module permissions cleared");
        }

        _logger.LogInformation("Starting module permission seeding...");

        var modulePermissions = new List<ModulePermission>();
        var modules = Enum.GetValues<ModuleType>();
        var permissions = Enum.GetValues<PermissionType>();

        foreach (var module in modules)
        {
            foreach (var permission in permissions)
            {
                var name = $"{module}.{permission}";
                var description = $"Allows {permission.ToString().ToLower()} operations in the {module} module";

                var modulePermission = ModulePermission.Create(module, permission, name, description);
                modulePermissions.Add(modulePermission);
            }
        }

        await _context.ModulePermissions.AddRangeAsync(modulePermissions);
        _logger.LogInformation("Seeded {Count} module permissions", modulePermissions.Count);
    }

    private async Task SeedRoleModulePermissionsAsync()
    {
        var roleModulePermissionCount = await _context.RoleModulePermissions.CountAsync();
        _logger.LogInformation("Current role-module permission count: {RoleModulePermissionCount}", roleModulePermissionCount);

        var reSeedRoleModulePermissions = bool.Parse(_configuration["DataSeeding:ReSeedRoleModulePermissions"] ?? "false");
        
        if (roleModulePermissionCount > 0 && !reSeedRoleModulePermissions)
        {
            _logger.LogInformation("Role-module permissions already exist, skipping seeding");
            return;
        }
        
        if (reSeedRoleModulePermissions && roleModulePermissionCount > 0)
        {
            _logger.LogInformation("ReSeedRoleModulePermissions is enabled, clearing existing role-module permissions...");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"RoleModulePermissions\"");
            _logger.LogInformation("Existing role-module permissions cleared");
        }

        _logger.LogInformation("Starting role-module permission seeding...");

        // Get all roles and module permissions from database
        var roles = await _context.Roles.ToListAsync();
        var modulePermissions = await _context.ModulePermissions.ToListAsync();

        var roleModulePermissions = new List<RoleModulePermission>();

        // Get the role-module permission mapping from the authorization system
        var permissionMap = ModulePermissionMap.GetRoleModulePermissions();

        foreach (var role in roles)
        {
            // Parse role type from name
            if (!Enum.TryParse<RoleType>(role.Name, out var roleType))
            {
                _logger.LogWarning("Could not parse role type for role: {RoleName}", role.Name);
                continue;
            }

            // Check if this role type exists in our permission map
            if (!permissionMap.ContainsKey(roleType))
            {
                _logger.LogWarning("No permission mapping found for role type: {RoleType}", roleType);
                continue;
            }

            var rolePermissions = permissionMap[roleType];

            foreach (var moduleMapping in rolePermissions)
            {
                var module = moduleMapping.Key;
                var permissions = moduleMapping.Value;

                foreach (var permission in permissions)
                {
                    // Find the corresponding module permission in the database
                    var modulePermission = modulePermissions
                        .FirstOrDefault(mp => mp.Module == module && mp.Permission == permission);

                    if (modulePermission == null)
                    {
                        _logger.LogWarning("Module permission not found: {Module}.{Permission}", module, permission);
                        continue;
                    }

                    // Create the role-module-permission association
                    var roleModulePermission = RoleModulePermission.Create(
                        role.Id, 
                        modulePermission.Id, 
                        grantedByUserId: null, 
                        grantReason: "System seeded during initial setup"
                    );

                    roleModulePermissions.Add(roleModulePermission);
                }
            }
        }

        await _context.RoleModulePermissions.AddRangeAsync(roleModulePermissions);
        _logger.LogInformation("Seeded {Count} role-module permission associations", roleModulePermissions.Count);
    }

    private async Task SeedUsersAsync()
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
    }

    private async Task SeedIncidentsAsync()
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

    private async Task SeedPPEDataAsync()
    {
        // Check if we should re-seed PPE data even if it exists
        var reSeedPPEData = _configuration["DataSeeding:ReSeedPPEData"] == "true";

        if (!reSeedPPEData && (await _context.PPECategories.AnyAsync() || await _context.PPESizes.AnyAsync() || await _context.PPEStorageLocations.AnyAsync()))
        {
            _logger.LogInformation("PPE data already exists and ReSeedPPEData is false, skipping PPE data seeding");
            return;
        }

        _logger.LogInformation("Starting PPE data seeding...");

        // If re-seeding is enabled, clear existing data first
        if (reSeedPPEData)
        {
            if (await _context.PPECategories.AnyAsync())
            {
                _logger.LogInformation("Clearing existing PPE categories for re-seeding...");
                _context.PPECategories.RemoveRange(_context.PPECategories);
            }
            if (await _context.PPESizes.AnyAsync())
            {
                _logger.LogInformation("Clearing existing PPE sizes for re-seeding...");
                _context.PPESizes.RemoveRange(_context.PPESizes);
            }
            if (await _context.PPEStorageLocations.AnyAsync())
            {
                _logger.LogInformation("Clearing existing PPE storage locations for re-seeding...");
                _context.PPEStorageLocations.RemoveRange(_context.PPEStorageLocations);
            }
            await _context.SaveChangesAsync();
            _logger.LogInformation("Existing PPE data cleared");
        }

        // Seed PPE Categories
        var categories = new List<PPECategory>
        {
            PPECategory.Create("Hard Hats", "HARDHAT", "Head protection equipment", PPEType.HeadProtection, "system", true, true, 90, false, null, "EN 397"),
            PPECategory.Create("Safety Glasses", "GLASSES", "Eye protection equipment", PPEType.EyeProtection, "system", false, true, 30, false, null, "EN 166"),
            PPECategory.Create("Hearing Protection", "HEARING", "Ear protection equipment", PPEType.HearingProtection, "system", false, true, 30, false, null, "EN 352"),
            PPECategory.Create("Respirators", "RESPIRATOR", "Respiratory protection equipment", PPEType.RespiratoryProtection, "system", true, true, 30, true, 365, "EN 149"),
            PPECategory.Create("Work Gloves", "GLOVES", "Hand protection equipment", PPEType.HandProtection, "system", false, true, 30, false, null, "EN 388"),
            PPECategory.Create("Safety Boots", "BOOTS", "Foot protection equipment", PPEType.FootProtection, "system", true, true, 180, false, null, "EN ISO 20345"),
            PPECategory.Create("Hi-Vis Vests", "HIVIS", "High visibility clothing", PPEType.HighVisibility, "system", false, true, 90, false, null, "EN ISO 20471"),
            PPECategory.Create("Fall Harness", "HARNESS", "Fall protection equipment", PPEType.FallProtection, "system", true, true, 180, false, null, "EN 361"),
            PPECategory.Create("Lab Coats", "LABCOAT", "Body protection for laboratories", PPEType.BodyProtection, "system", false, true, 30, false, null, "EN 14126"),
            PPECategory.Create("Emergency Shower", "SHOWER", "Emergency decontamination equipment", PPEType.EmergencyEquipment, "system", true, true, 30, false, null, "ANSI Z358.1")
        };

        await _context.PPECategories.AddRangeAsync(categories);
        _logger.LogInformation("Seeded {Count} PPE categories", categories.Count);

        // Seed PPE Sizes
        var sizes = new List<PPESize>
        {
            PPESize.Create("Extra Small", "XS", "Extra Small size", "system", sortOrder: 1),
            PPESize.Create("Small", "S", "Small size", "system", sortOrder: 2),
            PPESize.Create("Medium", "M", "Medium size", "system", sortOrder: 3),
            PPESize.Create("Large", "L", "Large size", "system", sortOrder: 4),
            PPESize.Create("Extra Large", "XL", "Extra Large size", "system", sortOrder: 5),
            PPESize.Create("Double Extra Large", "XXL", "Double Extra Large size", "system", sortOrder: 6),
            PPESize.Create("Triple Extra Large", "XXXL", "Triple Extra Large size", "system", sortOrder: 7),
            PPESize.Create("One Size", "OS", "One size fits all", "system", sortOrder: 8),
            PPESize.Create("6", "6", "Size 6", "system", sortOrder: 10),
            PPESize.Create("7", "7", "Size 7", "system", sortOrder: 11),
            PPESize.Create("8", "8", "Size 8", "system", sortOrder: 12),
            PPESize.Create("9", "9", "Size 9", "system", sortOrder: 13),
            PPESize.Create("10", "10", "Size 10", "system", sortOrder: 14),
            PPESize.Create("11", "11", "Size 11", "system", sortOrder: 15),
            PPESize.Create("12", "12", "Size 12", "system", sortOrder: 16)
        };

        await _context.PPESizes.AddRangeAsync(sizes);
        _logger.LogInformation("Seeded {Count} PPE sizes", sizes.Count);

        // Seed PPE Storage Locations
        var storageLocations = new List<PPEStorageLocation>
        {
            PPEStorageLocation.Create("Main Safety Office", "MSO", "system", "Primary safety equipment storage", "Building A, Ground Floor", "HSE Manager", "+62-21-1234567", 2000),
            PPEStorageLocation.Create("Chemistry Lab Storage", "CHEM", "system", "Chemistry laboratory PPE storage", "Science Building, 2nd Floor", "Lab Supervisor", "+62-21-1234568", 500),
            PPEStorageLocation.Create("Maintenance Workshop", "MAINT", "system", "Maintenance department storage", "Workshop Building", "Maintenance Supervisor", "+62-21-1234569", 1000),
            PPEStorageLocation.Create("PE Equipment Room", "PE", "system", "Physical education equipment storage", "Sports Complex", "PE Coordinator", "+62-21-1234570", 300),
            PPEStorageLocation.Create("Emergency Response Station", "ERS", "system", "Emergency response equipment", "Multiple Locations", "Security Chief", "+62-21-1234571", 200),
            PPEStorageLocation.Create("Warehouse A", "WH-A", "system", "Primary warehouse storage", "External Warehouse Complex", "Warehouse Manager", "+62-21-1234572", 5000),
            PPEStorageLocation.Create("Cafeteria Storage", "CAF", "system", "Food service PPE storage", "Cafeteria Building", "Food Service Manager", "+62-21-1234573", 150)
        };

        await _context.PPEStorageLocations.AddRangeAsync(storageLocations);
        _logger.LogInformation("Seeded {Count} PPE storage locations", storageLocations.Count);
    }

    private async Task SeedPPEItemsAsync()
    {
        // Check if we should re-seed PPE items even if they exist
        var reSeedPPEItems = _configuration["DataSeeding:ReSeedPPEItems"] == "true";

        if (!reSeedPPEItems && await _context.PPEItems.AnyAsync())
        {
            _logger.LogInformation("PPE items already exist and ReSeedPPEItems is false, skipping PPE items seeding");
            return;
        }

        _logger.LogInformation("Starting PPE items seeding...");

        // If re-seeding is enabled, clear existing items first
        if (reSeedPPEItems && await _context.PPEItems.AnyAsync())
        {
            _logger.LogInformation("Clearing existing PPE items for re-seeding...");
            _context.PPEItems.RemoveRange(_context.PPEItems);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Existing PPE items cleared");
        }

        // Get reference data
        var categories = await _context.PPECategories.ToListAsync();
        var sizes = await _context.PPESizes.ToListAsync();
        var storageLocations = await _context.PPEStorageLocations.ToListAsync();

        if (!categories.Any() || !sizes.Any() || !storageLocations.Any())
        {
            _logger.LogWarning("Cannot seed PPE items because categories, sizes, or storage locations are missing");
            return;
        }

        // Create PPE items
        var ppeItems = new List<PPEItem>();
        var random = new Random();

        // Helper method to get random element
        T GetRandomElement<T>(List<T> list) => list[random.Next(list.Count)];

        // Seed various PPE items for each category
        foreach (var category in categories)
        {
            var itemCount = random.Next(5, 15); // 5-15 items per category
            
            for (int i = 1; i <= itemCount; i++)
            {
                var size = GetRandomElement(sizes);
                var storageLocation = GetRandomElement(storageLocations);
                
                var itemCode = $"{category.Code}-{i:D3}";
                var purchaseDate = DateTime.UtcNow.AddDays(-random.Next(30, 365));
                var cost = (decimal)(random.NextDouble() * 200 + 50); // $50-$250 range
                
                DateTime? expiryDate = null;
                if (category.RequiresExpiry && category.DefaultExpiryDays.HasValue)
                {
                    expiryDate = purchaseDate.AddDays(category.DefaultExpiryDays.Value);
                }

                var ppeItem = PPEItem.Create(
                    itemCode,
                    $"{category.Name} - Model {i}",
                    $"Standard {category.Name.ToLower()} for {category.Description.ToLower()}",
                    category.Id,
                    GetManufacturerForCategory(category.Type),
                    $"Model-{category.Code}-{i}",
                    purchaseDate,
                    cost,
                    "system",
                    sizeId: size.Id,
                    storageLocationId: storageLocation.Id,
                    location: storageLocation.Name,
                    color: GetRandomColor(),
                    expiryDate: expiryDate,
                    notes: $"Seeded item for {category.Name}"
                );

                // Randomly assign some conditions and statuses
                var conditionChance = random.NextDouble();
                if (conditionChance < 0.7) // 70% new/excellent
                {
                    // Keep as new
                }
                else if (conditionChance < 0.9) // 20% good/fair
                {
                    ppeItem.UpdateCondition(PPECondition.Good, "system");
                }
                else // 10% poor/damaged
                {
                    ppeItem.UpdateCondition(PPECondition.Fair, "system");
                }

                ppeItems.Add(ppeItem);
            }
        }

        await _context.PPEItems.AddRangeAsync(ppeItems);
        _logger.LogInformation("Seeded {Count} PPE items", ppeItems.Count);
    }

    private static string GetManufacturerForCategory(PPEType type) => type switch
    {
        PPEType.HeadProtection => GetRandomElement(new[] { "3M", "MSA", "Bullard", "Honeywell" }),
        PPEType.EyeProtection => GetRandomElement(new[] { "Uvex", "3M", "Oakley", "Pyramex" }),
        PPEType.HearingProtection => GetRandomElement(new[] { "3M", "Howard Leight", "Moldex", "Honeywell" }),
        PPEType.RespiratoryProtection => GetRandomElement(new[] { "3M", "Honeywell", "MSA", "Moldex" }),
        PPEType.HandProtection => GetRandomElement(new[] { "Ansell", "Showa", "HexArmor", "MCR Safety" }),
        PPEType.FootProtection => GetRandomElement(new[] { "Red Wing", "Timberland PRO", "Caterpillar", "Dr. Martens" }),
        PPEType.HighVisibility => GetRandomElement(new[] { "ML Kishigo", "Radians", "OccuNomix", "Portwest" }),
        PPEType.FallProtection => GetRandomElement(new[] { "Miller", "3M DBI-SALA", "Guardian", "MSA" }),
        PPEType.BodyProtection => GetRandomElement(new[] { "DuPont", "Lakeland", "Ansell", "Kimberly-Clark" }),
        PPEType.EmergencyEquipment => GetRandomElement(new[] { "Haws", "Guardian", "Speakman", "Bradley" }),
        _ => "Generic Manufacturer"
    };

    private static string? GetRandomColor()
    {
        var colors = new[] { "Blue", "Yellow", "Orange", "Green", "Red", "White", "Black", "Gray", null };
        var random = new Random();
        return colors[random.Next(colors.Length)];
    }

    private static T GetRandomElement<T>(T[] array)
    {
        var random = new Random();
        return array[random.Next(array.Length)];
    }

    private async Task SeedHazardsAsync()
    {
        var hazardCount = await _context.Hazards.CountAsync();
        _logger.LogInformation("Current hazard count: {HazardCount}", hazardCount);

        if (hazardCount > 0)
        {
            _logger.LogInformation("Hazards already exist, skipping seeding");
            return;
        }

        _logger.LogInformation("Starting hazard seeding...");

        // Get a user to use as reporter (preferably HSE Manager or Admin)
        var reporterUser = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "HSEManager" || ur.Role.Name == "Admin"))
            .FirstOrDefaultAsync() ?? await _context.Users.FirstAsync();

        var hazards = new List<Hazard>();
        var random = new Random();

        // Sample hazard data representing common school hazards
        var hazardTemplates = new[]
        {
            new { Title = "Wet floor in main corridor", Category = HazardCategory.Physical, Type = HazardType.Slip, Severity = HazardSeverity.Moderate, Location = "Main Corridor - Building A", Description = "Water accumulation from roof leak creates slippery conditions during rainy weather" },
            new { Title = "Loose handrail on staircase", Category = HazardCategory.Physical, Type = HazardType.Fall, Severity = HazardSeverity.Major, Location = "Science Building - 2nd Floor Staircase", Description = "Handrail is loose and wobbles when pressure is applied, creating fall risk" },
            new { Title = "Chemical storage ventilation inadequate", Category = HazardCategory.Chemical, Type = HazardType.Exposure, Severity = HazardSeverity.Major, Location = "Chemistry Lab - Room 201", Description = "Ventilation system not functioning properly for chemical storage area" },
            new { Title = "Electrical outlet sparking", Category = HazardCategory.Electrical, Type = HazardType.Burn, Severity = HazardSeverity.Major, Location = "IT Lab - Room 105", Description = "Electrical outlet showing signs of arcing and sparking when devices are plugged in" },
            new { Title = "Heavy equipment storage unstable", Category = HazardCategory.Mechanical, Type = HazardType.Collision, Severity = HazardSeverity.Major, Location = "Maintenance Workshop", Description = "Heavy equipment stored on unstable shelving that could fall" },
            new { Title = "Fire exit blocked by equipment", Category = HazardCategory.Fire, Type = HazardType.Fire, Severity = HazardSeverity.Major, Location = "Gymnasium - Emergency Exit", Description = "Sports equipment blocking emergency exit preventing safe evacuation" },
            new { Title = "Poor lighting in parking area", Category = HazardCategory.Physical, Type = HazardType.Trip, Severity = HazardSeverity.Minor, Location = "Staff Parking Area", Description = "Several light fixtures not working creating poor visibility in evening hours" },
            new { Title = "Asbestos in ceiling tiles", Category = HazardCategory.Environmental, Type = HazardType.Exposure, Severity = HazardSeverity.Catastrophic, Location = "Old Library Building - Main Hall", Description = "Potential asbestos-containing materials identified in deteriorating ceiling tiles" },
            new { Title = "Playground equipment wear", Category = HazardCategory.Mechanical, Type = HazardType.Cut, Severity = HazardSeverity.Moderate, Location = "Primary School Playground", Description = "Sharp edges and worn surfaces on playground equipment pose injury risk" },
            new { Title = "Kitchen equipment overheating", Category = HazardCategory.Physical, Type = HazardType.Burn, Severity = HazardSeverity.Moderate, Location = "Main Kitchen - Cooking Area", Description = "Industrial oven running hotter than normal and lacks proper insulation" },
            new { Title = "Mold growth in storage room", Category = HazardCategory.Biological, Type = HazardType.Exposure, Severity = HazardSeverity.Moderate, Location = "Storage Room - Basement Level", Description = "Visible mold growth due to moisture issues in basement storage area" },
            new { Title = "Ergonomic issues in computer lab", Category = HazardCategory.Ergonomic, Type = HazardType.Other, Severity = HazardSeverity.Minor, Location = "Computer Lab - Room 301", Description = "Improperly adjusted workstations causing strain for students and staff" }
        };

        var departments = new[] { "Facilities", "Academic", "Administration", "Safety", "Maintenance" };

        foreach (var template in hazardTemplates)
        {
            var geoLocation = random.NextDouble() < 0.7 ? 
                GeoLocation.Create(-6.2 + (random.NextDouble() * 0.01), 106.8 + (random.NextDouble() * 0.01)) : 
                null; // 70% chance of having geo location

            var hazard = Hazard.Create(
                title: template.Title,
                description: template.Description,
                category: template.Category,
                type: template.Type,
                location: template.Location,
                severity: template.Severity,
                reporterId: reporterUser.Id,
                reporterDepartment: GetRandomElement(departments),
                geoLocation: geoLocation
            );

            // Add some variety to creation dates (last 30 days)
            var daysAgo = random.Next(1, 31);
            var createdDate = DateTime.UtcNow.AddDays(-daysAgo);
            
            // Use reflection to set the creation date for variety in testing
            var createdAtProperty = typeof(Hazard).GetProperty("CreatedAt");
            createdAtProperty?.SetValue(hazard, createdDate);

            hazards.Add(hazard);
        }

        await _context.Hazards.AddRangeAsync(hazards);
        _logger.LogInformation("Seeded {Count} hazards", hazards.Count);

        // Save hazards first to get IDs
        await _context.SaveChangesAsync();

        // Now create risk assessments for some hazards
        await SeedRiskAssessmentsAsync(hazards);
        
        // Create mitigation actions for some hazards
        await SeedMitigationActionsAsync(hazards);
    }

    private async Task SeedRiskAssessmentsAsync(List<Hazard> hazards)
    {
        _logger.LogInformation("Starting risk assessment seeding...");

        // Get an HSE Manager or Admin to use as assessor
        var assessorUser = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "HSEManager" || ur.Role.Name == "Admin"))
            .FirstOrDefaultAsync() ?? await _context.Users.FirstAsync();

        var random = new Random();
        var riskAssessments = new List<RiskAssessment>();

        // Create risk assessments for 70% of hazards
        var hazardsToAssess = hazards.Take((int)(hazards.Count * 0.7)).ToList();

        foreach (var hazard in hazardsToAssess)
        {
            var probabilityScore = hazard.Severity switch
            {
                HazardSeverity.Catastrophic => random.Next(3, 6), // 3-5
                HazardSeverity.Major => random.Next(2, 5), // 2-4
                HazardSeverity.Moderate => random.Next(2, 4), // 2-3
                HazardSeverity.Minor => random.Next(1, 3), // 1-2
                _ => random.Next(1, 4) // 1-3
            };

            var severityScore = hazard.Severity switch
            {
                HazardSeverity.Catastrophic => 5,
                HazardSeverity.Major => 4,
                HazardSeverity.Moderate => 3,
                HazardSeverity.Minor => 2,
                _ => 1
            };

            var assessmentType = GetRandomElement(new[] { 
                RiskAssessmentType.General, 
                RiskAssessmentType.HIRA, 
                RiskAssessmentType.JSA 
            });

            var potentialConsequences = hazard.Category switch
            {
                HazardCategory.Chemical => "Chemical exposure, respiratory issues, skin irritation, potential long-term health effects",
                HazardCategory.Physical => "Slips, trips, falls, bruises, fractures, sprains",
                HazardCategory.Fire => "Burns, smoke inhalation, property damage, potential fatalities",
                HazardCategory.Electrical => "Electric shock, burns, cardiac arrest, equipment damage",
                HazardCategory.Biological => "Infections, allergic reactions, respiratory problems",
                HazardCategory.Environmental => "Long-term health effects, respiratory issues, environmental contamination",
                _ => "Physical injury, property damage, operational disruption"
            };

            var existingControls = hazard.Category switch
            {
                HazardCategory.Chemical => "PPE required, ventilation systems, chemical storage protocols, MSDS available",
                HazardCategory.Physical => "Warning signs posted, regular inspections, maintenance schedules",
                HazardCategory.Fire => "Fire detection systems, extinguishers available, evacuation procedures",
                HazardCategory.Electrical => "Circuit breakers, electrical inspections, lockout procedures",
                _ => "Regular inspections, staff training, safety procedures in place"
            };

            var recommendedActions = GenerateRecommendedActions(hazard);

            var riskAssessment = RiskAssessment.Create(
                hazardId: hazard.Id,
                type: assessmentType,
                assessorId: assessorUser.Id,
                probabilityScore: probabilityScore,
                severityScore: severityScore,
                potentialConsequences: potentialConsequences,
                existingControls: existingControls,
                recommendedActions: recommendedActions,
                additionalNotes: $"Assessment completed as part of routine {assessmentType} evaluation for {hazard.Category} hazards."
            );

            // 80% chance of approval for completed assessments
            if (random.NextDouble() < 0.8)
            {
                riskAssessment.Approve(assessorUser.Id, "Assessment reviewed and approved - actions recommended for implementation");
            }

            riskAssessments.Add(riskAssessment);
        }

        await _context.RiskAssessments.AddRangeAsync(riskAssessments);
        _logger.LogInformation("Seeded {Count} risk assessments", riskAssessments.Count);
    }

    private async Task SeedMitigationActionsAsync(List<Hazard> hazards)
    {
        _logger.LogInformation("Starting mitigation action seeding...");

        // Get users to assign actions to
        var users = await _context.Users.ToListAsync();
        var random = new Random();
        var mitigationActions = new List<HazardMitigationAction>();

        // Create mitigation actions for 60% of hazards
        var hazardsWithActions = hazards.Take((int)(hazards.Count * 0.6)).ToList();

        foreach (var hazard in hazardsWithActions)
        {
            var actionsForHazard = GenerateMitigationActionsForHazard(hazard, users, random);
            mitigationActions.AddRange(actionsForHazard);
        }

        await _context.HazardMitigationActions.AddRangeAsync(mitigationActions);
        _logger.LogInformation("Seeded {Count} mitigation actions", mitigationActions.Count);
    }

    private static string GenerateRecommendedActions(Hazard hazard)
    {
        return hazard.Category switch
        {
            HazardCategory.Chemical => "Improve ventilation system, enhance PPE requirements, provide additional training on chemical handling, review storage procedures",
            HazardCategory.Physical => "Install better lighting, repair/replace damaged surfaces, improve signage, implement regular maintenance schedule",
            HazardCategory.Fire => "Clear fire exits, improve fire detection systems, conduct fire drills, review evacuation procedures",
            HazardCategory.Electrical => "Immediate electrical inspection, repair/replacement of faulty equipment, implement lockout procedures",
            HazardCategory.Biological => "Professional remediation, improve ventilation, implement regular cleaning protocols, health monitoring",
            HazardCategory.Environmental => "Professional assessment and remediation, air quality monitoring, implement containment measures",
            _ => "Immediate safety assessment, implement interim controls, develop long-term mitigation strategy, staff training"
        };
    }

    private static List<HazardMitigationAction> GenerateMitigationActionsForHazard(Hazard hazard, List<User> users, Random random)
    {
        var actions = new List<HazardMitigationAction>();
        var assignedUser = GetRandomElement(users.ToArray());

        // Primary mitigation action based on hazard category
        var primaryAction = hazard.Category switch
        {
            HazardCategory.Chemical => HazardMitigationAction.Create(
                hazard.Id,
                "Upgrade ventilation system and implement proper chemical storage procedures",
                MitigationActionType.Engineering,
                DateTime.UtcNow.AddDays(30),
                assignedUser.Id,
                MitigationPriority.High,
                estimatedCost: 5000m,
                requiresVerification: true
            ),
            HazardCategory.Physical => HazardMitigationAction.Create(
                hazard.Id,
                "Repair structural issues and improve surface conditions",
                MitigationActionType.Engineering,
                DateTime.UtcNow.AddDays(14),
                assignedUser.Id,
                MitigationPriority.Medium,
                estimatedCost: 2000m
            ),
            HazardCategory.Fire => HazardMitigationAction.Create(
                hazard.Id,
                "Clear fire exits and install additional fire safety equipment",
                MitigationActionType.Engineering,
                DateTime.UtcNow.AddDays(7),
                assignedUser.Id,
                MitigationPriority.Critical,
                estimatedCost: 3000m,
                requiresVerification: true
            ),
            HazardCategory.Electrical => HazardMitigationAction.Create(
                hazard.Id,
                "Complete electrical inspection and repair/replace faulty equipment",
                MitigationActionType.Engineering,
                DateTime.UtcNow.AddDays(3),
                assignedUser.Id,
                MitigationPriority.Critical,
                estimatedCost: 1500m,
                requiresVerification: true
            ),
            _ => HazardMitigationAction.Create(
                hazard.Id,
                "Implement immediate safety controls and develop long-term solution",
                MitigationActionType.Administrative,
                DateTime.UtcNow.AddDays(21),
                assignedUser.Id,
                MitigationPriority.Medium,
                estimatedCost: 500m
            )
        };

        actions.Add(primaryAction);

        // Add administrative action for training/procedures
        var adminAction = HazardMitigationAction.Create(
            hazard.Id,
            "Provide safety training and update procedures for staff",
            MitigationActionType.Administrative,
            DateTime.UtcNow.AddDays(14),
            GetRandomElement(users.ToArray()).Id,
            MitigationPriority.Medium,
            estimatedCost: 200m
        );

        actions.Add(adminAction);

        // 50% chance of adding PPE requirement
        if (random.NextDouble() < 0.5)
        {
            var ppeAction = HazardMitigationAction.Create(
                hazard.Id,
                "Provide and enforce use of appropriate personal protective equipment",
                MitigationActionType.PPE,
                DateTime.UtcNow.AddDays(7),
                GetRandomElement(users.ToArray()).Id,
                MitigationPriority.High,
                estimatedCost: 300m
            );

            actions.Add(ppeAction);
        }

        return actions;
    }

    #region Health Monitoring Module Seeding

    private async Task SeedHealthDataAsync()
    {
        // Check if we should re-seed health data even if it exists
        var reSeedHealthData = _configuration["DataSeeding:ReSeedHealthData"] == "true";

        if (!reSeedHealthData && await _context.HealthRecords.AnyAsync())
        {
            _logger.LogInformation("Health data already exists and ReSeedHealthData is false, skipping health data seeding");
            return;
        }

        _logger.LogInformation("Starting health data seeding...");

        // If re-seeding is enabled, clear existing data first
        if (reSeedHealthData && await _context.HealthRecords.AnyAsync())
        {
            _logger.LogInformation("Clearing existing health data for re-seeding...");
            _context.HealthRecords.RemoveRange(_context.HealthRecords);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Existing health data cleared");
        }

        await SeedHealthRecordsAsync();
        await SeedMedicalConditionsAsync();
        await SeedVaccinationRecordsAsync();
        await SeedEmergencyContactsAsync();
        await SeedHealthIncidentsAsync();
    }

    private async Task SeedHealthRecordsAsync()
    {
        var healthRecordCount = await _context.HealthRecords.CountAsync();
        _logger.LogInformation("Current health record count: {HealthRecordCount}", healthRecordCount);

        if (healthRecordCount > 0)
        {
            _logger.LogInformation("Health records already exist, skipping seeding");
            return;
        }

        _logger.LogInformation("Starting health record seeding...");

        // Get all users to create health records for them
        var users = await _context.Users.ToListAsync();
        var healthRecords = new List<HealthRecord>();
        var random = new Random();

        foreach (var user in users)
        {
            // Determine person type based on email domain and role
            var personType = DeterminePersonType(user.Email);
            
            // Generate random date of birth (18-65 years old)
            var age = random.Next(18, 66);
            var dateOfBirth = DateTime.UtcNow.AddYears(-age).AddDays(random.Next(-365, 365));
            
            // Random blood type (some may not have it recorded)
            var bloodType = random.NextDouble() < 0.8 ? GetRandomBloodType(random) : (BloodType?)null;
            
            // Some medical notes
            var medicalNotes = GenerateMedicalNotes(personType, random);

            var healthRecord = HealthRecord.Create(
                personId: user.Id,
                personType: personType,
                dateOfBirth: dateOfBirth,
                bloodType: bloodType,
                medicalNotes: medicalNotes
            );

            healthRecords.Add(healthRecord);
        }

        await _context.HealthRecords.AddRangeAsync(healthRecords);
        _logger.LogInformation("Seeded {Count} health records", healthRecords.Count);
    }

    private async Task SeedMedicalConditionsAsync()
    {
        var medicalConditionCount = await _context.MedicalConditions.CountAsync();
        _logger.LogInformation("Current medical condition count: {MedicalConditionCount}", medicalConditionCount);

        if (medicalConditionCount > 0)
        {
            _logger.LogInformation("Medical conditions already exist, skipping seeding");
            return;
        }

        _logger.LogInformation("Starting medical condition seeding...");

        // Get health records
        var healthRecords = await _context.HealthRecords.ToListAsync();
        var medicalConditions = new List<MedicalCondition>();
        var random = new Random();

        // Common medical conditions for seeding
        var conditionTemplates = new[]
        {
            new { Type = ConditionType.Allergy, Name = "Peanut Allergy", Severity = ConditionSeverity.Severe, Emergency = true, Instructions = "Use EpiPen immediately, call 911" },
            new { Type = ConditionType.Allergy, Name = "Bee Sting Allergy", Severity = ConditionSeverity.Moderate, Emergency = true, Instructions = "Monitor for swelling, seek medical attention if severe" },
            new { Type = ConditionType.ChronicCondition, Name = "Asthma", Severity = ConditionSeverity.Moderate, Emergency = false, Instructions = "Use inhaler as prescribed" },
            new { Type = ConditionType.ChronicCondition, Name = "Type 1 Diabetes", Severity = ConditionSeverity.Severe, Emergency = true, Instructions = "Monitor blood sugar, have glucose tablets available" },
            new { Type = ConditionType.MedicationDependency, Name = "Daily Insulin", Severity = ConditionSeverity.Severe, Emergency = true, Instructions = "Ensure medication schedule is maintained" },
            new { Type = ConditionType.PhysicalLimitation, Name = "Mobility Issues", Severity = ConditionSeverity.Mild, Emergency = false, Instructions = "Ensure accessible facilities" },
            new { Type = ConditionType.Dietary, Name = "Celiac Disease", Severity = ConditionSeverity.Moderate, Emergency = false, Instructions = "Strict gluten-free diet required" },
            new { Type = ConditionType.MentalHealthCondition, Name = "Anxiety Disorder", Severity = ConditionSeverity.Mild, Emergency = false, Instructions = "Provide calm environment during episodes" },
            new { Type = ConditionType.Allergy, Name = "Latex Allergy", Severity = ConditionSeverity.Moderate, Emergency = false, Instructions = "Use non-latex gloves and equipment" },
            new { Type = ConditionType.Other, Name = "Epilepsy", Severity = ConditionSeverity.Severe, Emergency = true, Instructions = "Do not restrain during seizure, time episode, call for help" }
        };

        // Assign medical conditions to some health records (about 30% have conditions)
        var recordsWithConditions = healthRecords.Take((int)(healthRecords.Count * 0.3)).ToList();

        foreach (var healthRecord in recordsWithConditions)
        {
            // Each person gets 1-3 conditions
            var conditionCount = random.Next(1, 4);
            var selectedConditions = conditionTemplates.OrderBy(x => random.Next()).Take(conditionCount);

            foreach (var template in selectedConditions)
            {
                var diagnosedDate = DateTime.UtcNow.AddDays(-random.Next(30, 1825)); // 1 month to 5 years ago
                
                var medicalCondition = MedicalCondition.Create(
                    healthRecordId: healthRecord.Id,
                    type: template.Type,
                    name: template.Name,
                    description: $"Diagnosed {template.Name.ToLower()} requiring ongoing management",
                    severity: template.Severity,
                    treatmentPlan: GenerateTreatmentPlan(template.Name),
                    diagnosedDate: diagnosedDate,
                    requiresEmergencyAction: template.Emergency,
                    emergencyInstructions: template.Emergency ? template.Instructions : null
                );

                medicalConditions.Add(medicalCondition);
            }
        }

        await _context.MedicalConditions.AddRangeAsync(medicalConditions);
        _logger.LogInformation("Seeded {Count} medical conditions", medicalConditions.Count);
    }

    private async Task SeedVaccinationRecordsAsync()
    {
        var vaccinationCount = await _context.VaccinationRecords.CountAsync();
        _logger.LogInformation("Current vaccination record count: {VaccinationCount}", vaccinationCount);

        if (vaccinationCount > 0)
        {
            _logger.LogInformation("Vaccination records already exist, skipping seeding");
            return;
        }

        _logger.LogInformation("Starting vaccination record seeding...");

        // Get health records
        var healthRecords = await _context.HealthRecords.ToListAsync();
        var vaccinationRecords = new List<VaccinationRecord>();
        var random = new Random();

        // Common vaccinations required for school/work
        var requiredVaccines = new[]
        {
            "COVID-19", "Influenza (Annual)", "Hepatitis B", "MMR (Measles, Mumps, Rubella)",
            "DPT (Diphtheria, Pertussis, Tetanus)", "Polio", "Varicella (Chickenpox)"
        };

        var optionalVaccines = new[]
        {
            "HPV", "Meningococcal", "Pneumococcal", "Hepatitis A", "Japanese Encephalitis"
        };

        foreach (var healthRecord in healthRecords)
        {
            // All records get required vaccines
            foreach (var vaccine in requiredVaccines)
            {
                var isAnnual = vaccine.Contains("Annual");
                var administered = random.NextDouble() < 0.9; // 90% have required vaccines

                DateTime? dateAdministered = null;
                DateTime? expiryDate = null;
                
                if (administered)
                {
                    // Administered within last 1-3 years for most vaccines
                    var daysBack = isAnnual ? random.Next(30, 365) : random.Next(365, 1095);
                    dateAdministered = DateTime.UtcNow.AddDays(-daysBack);
                    
                    // Set expiry for vaccines that expire
                    if (isAnnual)
                        expiryDate = dateAdministered.Value.AddDays(365);
                    else if (vaccine.Contains("Tetanus") || vaccine.Contains("DPT"))
                        expiryDate = dateAdministered.Value.AddYears(10);
                }

                var vaccination = VaccinationRecord.Create(
                    healthRecordId: healthRecord.Id,
                    vaccineName: vaccine,
                    isRequired: true,
                    dateAdministered: dateAdministered,
                    expiryDate: expiryDate,
                    batchNumber: administered ? $"BATCH-{random.Next(1000, 9999)}" : null,
                    administeredBy: administered ? GetRandomProvider(random) : null,
                    administrationLocation: administered ? GetRandomLocation(random) : null,
                    notes: GenerateVaccineNotes(vaccine, administered, random)
                );

                vaccinationRecords.Add(vaccination);
            }

            // Some get optional vaccines (30% chance)
            if (random.NextDouble() < 0.3)
            {
                var optionalCount = random.Next(1, 3);
                var selectedOptional = optionalVaccines.OrderBy(x => random.Next()).Take(optionalCount);

                foreach (var vaccine in selectedOptional)
                {
                    var administered = random.NextDouble() < 0.7; // 70% completion rate for optional
                    
                    DateTime? dateAdministered = null;
                    if (administered)
                    {
                        dateAdministered = DateTime.UtcNow.AddDays(-random.Next(180, 1095));
                    }

                    var vaccination = VaccinationRecord.Create(
                        healthRecordId: healthRecord.Id,
                        vaccineName: vaccine,
                        isRequired: false,
                        dateAdministered: dateAdministered,
                        batchNumber: administered ? $"OPT-{random.Next(1000, 9999)}" : null,
                        administeredBy: administered ? GetRandomProvider(random) : null,
                        administrationLocation: administered ? GetRandomLocation(random) : null,
                        notes: $"Optional vaccination for {vaccine}"
                    );

                    vaccinationRecords.Add(vaccination);
                }
            }
        }

        await _context.VaccinationRecords.AddRangeAsync(vaccinationRecords);
        _logger.LogInformation("Seeded {Count} vaccination records", vaccinationRecords.Count);
    }

    private async Task SeedEmergencyContactsAsync()
    {
        var emergencyContactCount = await _context.EmergencyContacts.CountAsync();
        _logger.LogInformation("Current emergency contact count: {EmergencyContactCount}", emergencyContactCount);

        if (emergencyContactCount > 0)
        {
            _logger.LogInformation("Emergency contacts already exist, skipping seeding");
            return;
        }

        _logger.LogInformation("Starting emergency contact seeding...");

        // Get health records
        var healthRecords = await _context.HealthRecords.ToListAsync();
        var emergencyContacts = new List<EmergencyContact>();
        var random = new Random();

        foreach (var healthRecord in healthRecords)
        {
            // Each person gets 1-3 emergency contacts
            var contactCount = random.Next(1, 4);
            
            for (int i = 1; i <= contactCount; i++)
            {
                var relationship = GetRandomRelationship(random, i == 1);
                var isPrimary = i == 1;
                
                var contact = EmergencyContact.Create(
                    healthRecordId: healthRecord.Id,
                    name: GenerateContactName(relationship, random),
                    relationship: relationship,
                    primaryPhone: GeneratePhoneNumber(random),
                    email: random.NextDouble() < 0.8 ? GenerateEmail(random) : null,
                    secondaryPhone: random.NextDouble() < 0.3 ? GeneratePhoneNumber(random) : null,
                    address: random.NextDouble() < 0.6 ? GenerateAddress(random) : null,
                    isPrimaryContact: isPrimary,
                    authorizedForPickup: relationship == ContactRelationship.Parent || relationship == ContactRelationship.Guardian || random.NextDouble() < 0.5,
                    authorizedForMedicalDecisions: relationship == ContactRelationship.Parent || relationship == ContactRelationship.Guardian || (relationship == ContactRelationship.Spouse && random.NextDouble() < 0.8),
                    customRelationship: relationship == ContactRelationship.Other ? "Family Friend" : null,
                    notes: random.NextDouble() < 0.3 ? "Available 24/7 for emergencies" : null
                );

                contact.SetContactOrder(i);
                emergencyContacts.Add(contact);
            }
        }

        await _context.EmergencyContacts.AddRangeAsync(emergencyContacts);
        _logger.LogInformation("Seeded {Count} emergency contacts", emergencyContacts.Count);
    }

    private async Task SeedHealthIncidentsAsync()
    {
        var healthIncidentCount = await _context.HealthIncidents.CountAsync();
        _logger.LogInformation("Current health incident count: {HealthIncidentCount}", healthIncidentCount);

        if (healthIncidentCount > 0)
        {
            _logger.LogInformation("Health incidents already exist, skipping seeding");
            return;
        }

        _logger.LogInformation("Starting health incident seeding...");

        // Get health records
        var healthRecords = await _context.HealthRecords.ToListAsync();
        var healthIncidents = new List<HealthIncident>();
        var random = new Random();

        // Sample health incident templates
        var incidentTemplates = new[]
        {
            new { Type = HealthIncidentType.Injury, Severity = HealthIncidentSeverity.Minor, Symptoms = "Minor cut on hand", Treatment = "Cleaned wound, applied bandage, tetanus shot up to date", Location = TreatmentLocation.SchoolNurse },
            new { Type = HealthIncidentType.AllergicReaction, Severity = HealthIncidentSeverity.Serious, Symptoms = "Swelling, difficulty breathing after eating peanuts", Treatment = "EpiPen administered, emergency services called", Location = TreatmentLocation.Hospital },
            new { Type = HealthIncidentType.Illness, Severity = HealthIncidentSeverity.Moderate, Symptoms = "Fever, headache, nausea", Treatment = "Rest, fluids, fever reducer, sent home", Location = TreatmentLocation.SchoolNurse },
            new { Type = HealthIncidentType.MedicationIssue, Severity = HealthIncidentSeverity.Minor, Symptoms = "Forgot daily medication", Treatment = "Contacted parent, medication administered", Location = TreatmentLocation.SchoolNurse },
            new { Type = HealthIncidentType.ChronicConditionFlareUp, Severity = HealthIncidentSeverity.Moderate, Symptoms = "Asthma attack during PE", Treatment = "Inhaler used, rest period, monitored breathing", Location = TreatmentLocation.Classroom },
            new { Type = HealthIncidentType.Injury, Severity = HealthIncidentSeverity.Serious, Symptoms = "Sprained ankle during sports", Treatment = "Ice applied, ankle wrapped, x-ray taken", Location = TreatmentLocation.Clinic },
            new { Type = HealthIncidentType.MentalHealthEpisode, Severity = HealthIncidentSeverity.Moderate, Symptoms = "Anxiety attack before exam", Treatment = "Counseling, breathing exercises, parents notified", Location = TreatmentLocation.SchoolNurse },
            new { Type = HealthIncidentType.Other, Severity = HealthIncidentSeverity.Minor, Symptoms = "Headache and dizziness", Treatment = "Rest, water, blood pressure checked", Location = TreatmentLocation.SchoolNurse }
        };

        // Create incidents for some health records (about 20% have had health incidents)
        var recordsWithIncidents = healthRecords.Take((int)(healthRecords.Count * 0.2)).ToList();

        foreach (var healthRecord in recordsWithIncidents)
        {
            // Each person gets 1-2 health incidents
            var incidentCount = random.Next(1, 3);
            var selectedIncidents = incidentTemplates.OrderBy(x => random.Next()).Take(incidentCount);

            foreach (var template in selectedIncidents)
            {
                var incidentDate = DateTime.UtcNow.AddDays(-random.Next(7, 365)); // 1 week to 1 year ago
                var requiredHospitalization = template.Severity >= HealthIncidentSeverity.Serious && random.NextDouble() < 0.3;

                var healthIncident = HealthIncident.Create(
                    healthRecordId: healthRecord.Id,
                    type: template.Type,
                    severity: template.Severity,
                    symptoms: template.Symptoms,
                    treatmentProvided: template.Treatment,
                    treatmentLocation: template.Location,
                    incidentDateTime: incidentDate,
                    requiredHospitalization: requiredHospitalization,
                    treatedBy: GetRandomProvider(random)
                );

                // Add realistic follow-up based on severity
                if (template.Severity >= HealthIncidentSeverity.Moderate)
                {
                    healthIncident.NotifyParents();
                    
                    if (template.Severity >= HealthIncidentSeverity.Serious)
                    {
                        healthIncident.SetFollowUpRequired("Follow up with family doctor within 48 hours");
                        
                        if (requiredHospitalization)
                        {
                            healthIncident.SetReturnToSchoolDate(incidentDate.AddDays(3));
                        }
                    }
                }

                // Most incidents are resolved
                if (random.NextDouble() < 0.8)
                {
                    healthIncident.Resolve("Incident fully resolved, no ongoing concerns");
                }

                healthIncidents.Add(healthIncident);
            }
        }

        await _context.HealthIncidents.AddRangeAsync(healthIncidents);
        _logger.LogInformation("Seeded {Count} health incidents", healthIncidents.Count);
    }

    #endregion

    #region Extended Data Seeding (PPE Workflow, Incident Extensions, Notifications)

    private async Task SeedExtendedDataAsync()
    {
        _logger.LogInformation("Starting extended data seeding...");

        await SeedPPEWorkflowDataAsync();
        await SeedIncidentExtensionsAsync();
        await SeedNotificationSystemDataAsync();
    }

    private async Task SeedPPEWorkflowDataAsync()
    {
        _logger.LogInformation("Seeding PPE workflow data...");
        
        await SeedPPEAssignmentsAsync();
        await SeedPPEInspectionsAsync();
        await SeedPPERequestsAsync();
        await SeedPPEComplianceRequirementsAsync();
        
        _logger.LogInformation("PPE workflow data seeding completed");
    }

    private async Task SeedPPEAssignmentsAsync()
    {
        var assignmentCount = await _context.PPEAssignments.CountAsync();
        if (assignmentCount > 0)
        {
            _logger.LogInformation("PPE assignments already exist, skipping seeding");
            return;
        }

        _logger.LogInformation("Starting PPE assignment seeding...");

        // Get required data
        var users = await _context.Users.ToListAsync();
        var ppeItems = await _context.PPEItems.Include(p => p.Category).ToListAsync();
        var assignments = new List<PPEAssignment>();
        var random = new Random();

        if (!users.Any() || !ppeItems.Any())
        {
            _logger.LogWarning("Cannot seed PPE assignments - users or PPE items missing");
            return;
        }

        // Assign PPE to users based on their roles/departments
        foreach (var user in users)
        {
            var assignmentCountPerUser = random.Next(2, 6); // 2-5 PPE items per user
            var availableItems = ppeItems.Where(p => p.Status == PPEStatus.Available).ToList();
            
            if (!availableItems.Any()) continue;

            var selectedItems = availableItems.OrderBy(x => random.Next()).Take(assignmentCountPerUser);

            foreach (var item in selectedItems)
            {
                var assignedDate = DateTime.UtcNow.AddDays(-random.Next(1, 180)); // Assigned in last 6 months
                var expectedReturnDate = assignedDate.AddDays(GetAssignmentDuration(item.Category.Type, random));
                
                var assignment = PPEAssignment.Create(
                    ppeItemId: item.Id,
                    assignedToId: user.Id,
                    assignedBy: "System",
                    purpose: GetAssignmentPurpose(item.Category.Type, user.Department)
                );

                // Some assignments are returned
                if (random.NextDouble() < 0.4) // 40% are returned
                {
                    assignment.Return("System", "Returned during seeding process");
                }

                assignments.Add(assignment);
            }
        }

        await _context.PPEAssignments.AddRangeAsync(assignments);
        _logger.LogInformation("Seeded {Count} PPE assignments", assignments.Count);
    }

    private async Task SeedPPEInspectionsAsync()
    {
        var inspectionCount = await _context.PPEInspections.CountAsync();
        if (inspectionCount > 0)
        {
            _logger.LogInformation("PPE inspections already exist, skipping seeding");
            return;
        }

        _logger.LogInformation("Starting PPE inspection seeding...");

        // Get PPE items that require inspection
        var ppeItems = await _context.PPEItems.Include(p => p.Category)
            .Where(p => p.Category.RequiresInspection)
            .ToListAsync();
        var users = await _context.Users.ToListAsync();
        var inspections = new List<PPEInspection>();
        var random = new Random();

        if (!ppeItems.Any())
        {
            _logger.LogInformation("No PPE items require inspection, skipping inspection seeding");
            return;
        }

        foreach (var item in ppeItems)
        {
            // Create 1-3 historical inspections per item
            var inspectionCountPerItem = random.Next(1, 4);
            
            for (int i = 0; i < inspectionCountPerItem; i++)
            {
                var inspectionDate = DateTime.UtcNow.AddDays(-random.Next(7, 180)); // Last 6 months
                var inspectionResult = GetRandomInspectionResult(random);
                var condition = GetRandomPPECondition(random);
                
                // Get a random user as inspector
                var inspector = GetRandomElement(users.ToArray());
                
                var inspection = PPEInspection.Create(
                    ppeItemId: item.Id,
                    inspectorId: inspector.Id,
                    inspectionDate: inspectionDate,
                    result: inspectionResult,
                    createdBy: "System",
                    findings: GenerateInspectionFindings(inspectionResult, condition, item.Category.Name, random),
                    correctiveActions: inspectionResult == InspectionResult.Failed ? GenerateCorrectiveActions(item.Category.Name, random) : null,
                    recommendedCondition: condition,
                    nextInspectionIntervalDays: item.Category.InspectionIntervalDays ?? 30
                );

                inspections.Add(inspection);
            }
        }

        await _context.PPEInspections.AddRangeAsync(inspections);
        _logger.LogInformation("Seeded {Count} PPE inspections", inspections.Count);
    }

    private async Task SeedPPERequestsAsync()
    {
        var requestCount = await _context.PPERequests.CountAsync();
        if (requestCount > 0)
        {
            _logger.LogInformation("PPE requests already exist, skipping seeding");
            return;
        }

        _logger.LogInformation("Starting PPE request seeding...");

        // Get users and PPE categories
        var users = await _context.Users.ToListAsync();
        var categories = await _context.PPECategories.ToListAsync();
        var sizes = await _context.PPESizes.ToListAsync();
        var requests = new List<PPERequest>();
        var random = new Random();

        if (!users.Any() || !categories.Any())
        {
            _logger.LogWarning("Cannot seed PPE requests - users or categories missing");
            return;
        }

        // Create requests from users
        var requestingUsers = users.Take((int)(users.Count * 0.6)).ToList(); // 60% of users made requests

        foreach (var user in requestingUsers)
        {
            // Each user makes 1-2 requests
            var requestCountPerUser = random.Next(1, 3);
            
            for (int i = 0; i < requestCountPerUser; i++)
            {
                var category = GetRandomElement(categories.ToArray());
                var size = sizes.Any() ? GetRandomElement(sizes.ToArray()) : null;
                var requestDate = DateTime.UtcNow.AddDays(-random.Next(1, 90)); // Last 3 months
                var urgency = GetRandomUrgency(random);
                
                var request = PPERequest.Create(
                    requesterId: user.Id,
                    categoryId: category.Id,
                    justification: GenerateRequestJustification(category.Name, user.Department, random),
                    priority: urgency,
                    createdBy: "System"
                );
                
                // Add request item
                request.AddRequestItem(
                    itemDescription: $"{category.Name} - {(size?.Name ?? "Standard")}",
                    size: size?.Name,
                    quantity: random.Next(1, 4)
                );
                
                // Submit the request
                request.Submit("System");

                // Assign reviewer and process some requests
                var reviewer = GetRandomElement(users.Where(u => u.Department == "Health & Safety" || u.Department == "IT").ToArray());
                if (reviewer != null)
                {
                    request.AssignReviewer(reviewer.Id, "System");
                    
                    var processChance = random.NextDouble();
                    if (processChance < 0.3) // 30% approved
                    {
                        request.Approve("System", "Request approved during seeding");
                    }
                    else if (processChance < 0.5) // 20% rejected
                    {
                        request.Reject("System", GetRandomRejectionReason(random));
                    }
                    // 50% remain under review
                }

                requests.Add(request);
            }
        }

        await _context.PPERequests.AddRangeAsync(requests);
        _logger.LogInformation("Seeded {Count} PPE requests", requests.Count);
    }

    private async Task SeedPPEComplianceRequirementsAsync()
    {
        var complianceCount = await _context.PPEComplianceRequirements.CountAsync();
        if (complianceCount > 0)
        {
            _logger.LogInformation("PPE compliance requirements already exist, skipping seeding");
            return;
        }

        _logger.LogInformation("Starting PPE compliance requirement seeding...");

        // Get PPE categories
        var categories = await _context.PPECategories.ToListAsync();
        var requirements = new List<PPEComplianceRequirement>();

        if (!categories.Any())
        {
            _logger.LogWarning("Cannot seed PPE compliance requirements - categories missing");
            return;
        }
        
        // Get existing requirements to avoid duplicates
        var existingRequirements = await _context.PPEComplianceRequirements
            .Select(r => new { r.RoleId, r.CategoryId })
            .ToListAsync();

        // Define compliance requirements for different departments/roles
        var departmentRequirements = new[]
        {
            new { Department = "Science", RequiredTypes = new[] { PPEType.EyeProtection, PPEType.BodyProtection, PPEType.HandProtection } },
            new { Department = "Maintenance", RequiredTypes = new[] { PPEType.HeadProtection, PPEType.FootProtection, PPEType.HandProtection, PPEType.EyeProtection } },
            new { Department = "Facilities", RequiredTypes = new[] { PPEType.HighVisibility, PPEType.FootProtection, PPEType.HandProtection } },
            new { Department = "Chemistry Lab", RequiredTypes = new[] { PPEType.EyeProtection, PPEType.RespiratoryProtection, PPEType.BodyProtection, PPEType.HandProtection } },
            new { Department = "Workshop", RequiredTypes = new[] { PPEType.HeadProtection, PPEType.EyeProtection, PPEType.HearingProtection, PPEType.FootProtection } },
            new { Department = "General", RequiredTypes = new[] { PPEType.EyeProtection, PPEType.FootProtection } }
        };

        foreach (var deptReq in departmentRequirements)
        {
            foreach (var requiredType in deptReq.RequiredTypes)
            {
                var category = categories.FirstOrDefault(c => c.Type == requiredType);
                if (category == null) continue;

                // Find a role that matches the department (simplified mapping)
                var roles = await _context.Roles.ToListAsync();
                var role = roles.FirstOrDefault(r => r.Name.Contains("Manager")) ?? roles.FirstOrDefault();
                if (role == null) continue;
                
                // Check if this requirement already exists in database or in current batch
                var alreadyExists = existingRequirements.Any(r => r.RoleId == role.Id && r.CategoryId == category.Id) ||
                                   requirements.Any(r => r.RoleId == role.Id && r.CategoryId == category.Id);
                if (!alreadyExists)
                {
                    var requirement = PPEComplianceRequirement.Create(
                        roleId: role.Id,
                        categoryId: category.Id,
                        isMandatory: true,
                        createdBy: "System",
                        complianceNote: $"All {deptReq.Department} personnel must use {category.Name} when performing duties"
                    );

                    requirements.Add(requirement);
                }
            }
        }

        await _context.PPEComplianceRequirements.AddRangeAsync(requirements);
        _logger.LogInformation("Seeded {Count} PPE compliance requirements", requirements.Count);
    }

    private async Task SeedIncidentExtensionsAsync()
    {
        _logger.LogInformation("Seeding incident extensions...");
        
        await SeedIncidentAttachmentsAsync();
        await SeedIncidentInvolvedPersonsAsync();
        await SeedIncidentAuditLogsAsync();
        
        _logger.LogInformation("Incident extensions seeding completed");
    }

    private async Task SeedIncidentAttachmentsAsync()
    {
        var attachmentCount = await _context.IncidentAttachments.CountAsync();
        if (attachmentCount > 0)
        {
            _logger.LogInformation("Incident attachments already exist, skipping seeding");
            return;
        }

        _logger.LogInformation("Starting incident attachment seeding...");

        // Get incidents
        var incidents = await _context.Incidents.ToListAsync();
        var attachments = new List<IncidentAttachment>();
        var random = new Random();

        if (!incidents.Any())
        {
            _logger.LogWarning("Cannot seed incident attachments - no incidents found");
            return;
        }

        foreach (var incident in incidents)
        {
            // 60% of incidents have attachments
            if (random.NextDouble() < 0.6)
            {
                var attachmentCountPerIncident = random.Next(1, 4); // 1-3 attachments per incident
                
                for (int i = 0; i < attachmentCountPerIncident; i++)
                {
                    var fileName = GenerateAttachmentFileName(incident.Severity, i + 1, random);
                    var fileSize = random.Next(50000, 5000000); // 50KB to 5MB
                    var filePath = $"/uploads/incidents/{incident.Id}/{fileName}";
                    
                    var attachment = new IncidentAttachment(
                        incidentId: incident.Id,
                        fileName: fileName,
                        filePath: filePath,
                        fileSize: fileSize,
                        uploadedBy: incident.ReporterName
                    );

                    attachments.Add(attachment);
                }
            }
        }

        await _context.IncidentAttachments.AddRangeAsync(attachments);
        _logger.LogInformation("Seeded {Count} incident attachments", attachments.Count);
    }

    private async Task SeedIncidentInvolvedPersonsAsync()
    {
        var involvedPersonCount = await _context.IncidentInvolvedPersons.CountAsync();
        if (involvedPersonCount > 0)
        {
            _logger.LogInformation("Incident involved persons already exist, skipping seeding");
            return;
        }

        _logger.LogInformation("Starting incident involved person seeding...");

        // Get incidents and users
        var incidents = await _context.Incidents.ToListAsync();
        var users = await _context.Users.ToListAsync();
        var involvedPersons = new List<IncidentInvolvedPerson>();
        var random = new Random();

        if (!incidents.Any() || !users.Any())
        {
            _logger.LogWarning("Cannot seed incident involved persons - incidents or users missing");
            return;
        }

        foreach (var incident in incidents)
        {
            // 70% of incidents have involved persons
            if (random.NextDouble() < 0.7)
            {
                var involvedCount = random.Next(1, 4); // 1-3 involved persons per incident
                var availableUsers = users.Where(u => u.Id != incident.ReporterId).ToList();
                
                if (!availableUsers.Any()) continue;
                
                var selectedUsers = availableUsers.OrderBy(x => random.Next()).Take(involvedCount);

                foreach (var user in selectedUsers)
                {
                    var involvementType = GetRandomInvolvementType(random);
                    var injuryDescription = involvementType == InvolvementType.Victim && random.NextDouble() < 0.4 ?
                        GenerateInjuryDescription(incident.Severity, random) : null;
                    
                    var involvedPerson = new IncidentInvolvedPerson(
                        incidentId: incident.Id,
                        personId: user.Id,
                        involvementType: involvementType,
                        injuryDescription: injuryDescription
                    );

                    involvedPersons.Add(involvedPerson);
                }
            }
        }

        await _context.IncidentInvolvedPersons.AddRangeAsync(involvedPersons);
        _logger.LogInformation("Seeded {Count} incident involved persons", involvedPersons.Count);
    }

    private async Task SeedIncidentAuditLogsAsync()
    {
        var auditLogCount = await _context.IncidentAuditLogs.CountAsync();
        if (auditLogCount > 0)
        {
            _logger.LogInformation("Incident audit logs already exist, skipping seeding");
            return;
        }

        _logger.LogInformation("Starting incident audit log seeding...");

        // Get incidents
        var incidents = await _context.Incidents.ToListAsync();
        var users = await _context.Users.ToListAsync();
        var auditLogs = new List<IncidentAuditLog>();
        var random = new Random();

        if (!incidents.Any())
        {
            _logger.LogWarning("Cannot seed incident audit logs - no incidents found");
            return;
        }

        foreach (var incident in incidents)
        {
            // Create initial audit log for incident creation
            var creationLog = IncidentAuditLog.CreateAction(
                incidentId: incident.Id,
                action: AuditActions.Created,
                changedBy: incident.ReporterName,
                changeDescription: "Incident created and reported"
            );
            auditLogs.Add(creationLog);

            // Add status change logs
            var statusChanges = GenerateStatusChangeHistory(incident, random);
            foreach (var change in statusChanges)
            {
                var user = GetRandomElement(users.ToArray());
                var statusLog = IncidentAuditLog.Create(
                    incidentId: incident.Id,
                    action: AuditActions.StatusChanged,
                    fieldName: "Status",
                    oldValue: change.OldStatus,
                    newValue: change.Status.ToString(),
                    changedBy: user.Name,
                    changeDescription: $"Status changed to {change.Status}"
                );
                auditLogs.Add(statusLog);
            }

            // Add random actions (assignments, updates, etc.)
            var actionCount = random.Next(1, 4);
            for (int i = 0; i < actionCount; i++)
            {
                var user = GetRandomElement(users.ToArray());
                var actionType = GetRandomAuditActionType(random);
                var actionDate = incident.CreatedAt.AddDays(random.Next(1, 30));
                
                var actionLog = IncidentAuditLog.CreateAction(
                    incidentId: incident.Id,
                    action: actionType,
                    changedBy: user.Name,
                    changeDescription: GenerateAuditActionDescription(actionType, user.Name, random)
                );
                auditLogs.Add(actionLog);
            }
        }

        await _context.IncidentAuditLogs.AddRangeAsync(auditLogs);
        _logger.LogInformation("Seeded {Count} incident audit logs", auditLogs.Count);
    }

    private async Task SeedNotificationSystemDataAsync()
    {
        _logger.LogInformation("Seeding notification system data...");
        
        await SeedEscalationRulesAsync();
        await SeedNotificationHistoryAsync();
        
        _logger.LogInformation("Notification system seeding completed");
    }

    private async Task SeedEscalationRulesAsync()
    {
        var escalationRuleCount = await _context.EscalationRules.CountAsync();
        if (escalationRuleCount > 0)
        {
            _logger.LogInformation("Escalation rules already exist, skipping seeding");
            return;
        }

        _logger.LogInformation("Starting escalation rule seeding...");

        var escalationRules = new List<EscalationRule>();

        // Critical Incident Escalation
        var criticalRule = new EscalationRule
        {
            Name = "Critical Incident Immediate Escalation",
            Description = "Immediate escalation for critical and emergency incidents",
            IsActive = true,
            TriggerSeverities = new List<IncidentSeverity> { IncidentSeverity.Critical, IncidentSeverity.Emergency },
            TriggerStatuses = new List<IncidentStatus> { IncidentStatus.Reported },
            TriggerDepartments = new List<string>(),
            TriggerLocations = new List<string>(),
            Priority = 1,
            CreatedBy = "System",
            Actions = new List<EscalationAction>
            {
                new EscalationAction
                {
                    Type = EscalationActionType.NotifyRole,
                    Target = "HSEManager",
                    Channels = new List<NotificationChannel> { NotificationChannel.Email, NotificationChannel.Sms },
                    Delay = TimeSpan.Zero
                },
                new EscalationAction
                {
                    Type = EscalationActionType.SendEmergencyAlert,
                    Target = "Emergency Response Team",
                    Channels = new List<NotificationChannel> { NotificationChannel.Sms, NotificationChannel.Push },
                    Delay = TimeSpan.FromMinutes(5)
                }
            }
        };
        escalationRules.Add(criticalRule);

        // Delayed Response Escalation
        var delayedRule = new EscalationRule
        {
            Name = "Delayed Response Escalation",
            Description = "Escalation for incidents that haven't been responded to within time limits",
            IsActive = true,
            TriggerSeverities = new List<IncidentSeverity> { IncidentSeverity.Serious, IncidentSeverity.Major },
            TriggerStatuses = new List<IncidentStatus> { IncidentStatus.Reported, IncidentStatus.UnderInvestigation },
            TriggerAfterDuration = TimeSpan.FromHours(4),
            Priority = 2,
            CreatedBy = "System",
            Actions = new List<EscalationAction>
            {
                new EscalationAction
                {
                    Type = EscalationActionType.EscalateToManager,
                    Target = "Department Manager",
                    Channels = new List<NotificationChannel> { NotificationChannel.Email },
                    Delay = TimeSpan.Zero
                }
            }
        };
        escalationRules.Add(delayedRule);

        // Department-specific Rules
        var labRule = new EscalationRule
        {
            Name = "Laboratory Incident Protocol",
            Description = "Special escalation for laboratory-related incidents",
            IsActive = true,
            TriggerSeverities = new List<IncidentSeverity> { IncidentSeverity.Moderate, IncidentSeverity.Serious, IncidentSeverity.Major },
            TriggerDepartments = new List<string> { "Science", "Chemistry Lab" },
            Priority = 3,
            CreatedBy = "System",
            Actions = new List<EscalationAction>
            {
                new EscalationAction
                {
                    Type = EscalationActionType.NotifyDepartment,
                    Target = "Science Department",
                    Channels = new List<NotificationChannel> { NotificationChannel.Email, NotificationChannel.InApp },
                    Delay = TimeSpan.Zero
                },
                new EscalationAction
                {
                    Type = EscalationActionType.NotifyExternal,
                    Target = "safety@external-consultant.com",
                    Channels = new List<NotificationChannel> { NotificationChannel.Email },
                    Delay = TimeSpan.FromMinutes(15)
                }
            }
        };
        escalationRules.Add(labRule);

        await _context.EscalationRules.AddRangeAsync(escalationRules);
        _logger.LogInformation("Seeded {Count} escalation rules", escalationRules.Count);
    }

    private async Task SeedNotificationHistoryAsync()
    {
        var notificationHistoryCount = await _context.NotificationHistories.CountAsync();
        if (notificationHistoryCount > 0)
        {
            _logger.LogInformation("Notification history already exists, skipping seeding");
            return;
        }

        _logger.LogInformation("Starting notification history seeding...");

        // Get incidents and users
        var incidents = await _context.Incidents.ToListAsync();
        var users = await _context.Users.ToListAsync();
        var notifications = new List<NotificationHistory>();
        var random = new Random();

        if (!incidents.Any() || !users.Any())
        {
            _logger.LogWarning("Cannot seed notification history - incidents or users missing");
            return;
        }

        foreach (var incident in incidents)
        {
            // Each incident generates 2-5 notifications
            var notificationCountPerIncident = random.Next(2, 6);
            
            for (int i = 0; i < notificationCountPerIncident; i++)
            {
                var recipient = GetRandomElement(users.ToArray());
                var channel = GetRandomNotificationChannel(random);
                var priority = GetNotificationPriority(incident.Severity, random);
                var status = GetRandomNotificationStatus(random);
                var templateId = GetNotificationTemplate(incident.Severity, channel, random);
                
                var notification = new NotificationHistory
                {
                    IncidentId = incident.Id,
                    RecipientId = recipient.Id.ToString(),
                    RecipientType = "User",
                    TemplateId = templateId,
                    Channel = channel,
                    Priority = priority,
                    Subject = GenerateNotificationSubject(incident, templateId),
                    Content = GenerateNotificationContent(incident, recipient.Name, templateId),
                    Status = status,
                    CreatedBy = "System",
                    Metadata = new Dictionary<string, string>
                    {
                        { "IncidentSeverity", incident.Severity.ToString() },
                        { "IncidentLocation", incident.Location },
                        { "NotificationReason", GetNotificationReason(templateId) }
                    }
                };

                // Set delivery timestamps based on status
                var baseTime = incident.CreatedAt.AddMinutes(random.Next(0, 60));
                if (status >= NotificationStatus.Sent)
                {
                    notification.SentAt = baseTime;
                }
                if (status >= NotificationStatus.Delivered)
                {
                    notification.DeliveredAt = baseTime.AddMinutes(random.Next(1, 5));
                }
                if (status == NotificationStatus.Read)
                {
                    notification.ReadAt = notification.DeliveredAt?.AddMinutes(random.Next(1, 30));
                }
                if (status == NotificationStatus.Failed)
                {
                    notification.ErrorMessage = GetRandomNotificationError(channel, random);
                }

                notifications.Add(notification);
            }
        }

        await _context.NotificationHistories.AddRangeAsync(notifications);
        _logger.LogInformation("Seeded {Count} notification history records", notifications.Count);
    }

    #endregion

    #region Helper Methods for PPE Workflow Data Generation

    private static int GetAssignmentDuration(PPEType type, Random random)
    {
        return type switch
        {
            PPEType.BodyProtection => random.Next(30, 90), // Lab coats: 1-3 months
            PPEType.FootProtection => random.Next(180, 365), // Safety boots: 6-12 months
            PPEType.HeadProtection => random.Next(90, 180), // Hard hats: 3-6 months
            PPEType.HandProtection => random.Next(7, 30), // Gloves: 1 week to 1 month
            PPEType.EyeProtection => random.Next(90, 180), // Safety glasses: 3-6 months
            PPEType.HearingProtection => random.Next(30, 90), // Ear protection: 1-3 months
            PPEType.RespiratoryProtection => random.Next(30, 90), // Respirators: 1-3 months
            PPEType.HighVisibility => random.Next(90, 180), // Hi-vis vests: 3-6 months
            PPEType.FallProtection => random.Next(365, 730), // Harnesses: 1-2 years
            _ => random.Next(30, 180) // Default: 1-6 months
        };
    }

    private static string GetAssignmentPurpose(PPEType type, string department)
    {
        var purposes = type switch
        {
            PPEType.BodyProtection => new[] { "Laboratory work", "Chemical handling", "General protection" },
            PPEType.FootProtection => new[] { "Construction work", "Maintenance duties", "General safety" },
            PPEType.HeadProtection => new[] { "Construction sites", "Maintenance work", "Overhead hazards" },
            PPEType.HandProtection => new[] { "Chemical handling", "Maintenance work", "General protection" },
            PPEType.EyeProtection => new[] { "Laboratory work", "Workshop activities", "Chemical protection" },
            PPEType.HearingProtection => new[] { "Noisy environments", "Workshop activities", "Machinery operation" },
            PPEType.RespiratoryProtection => new[] { "Chemical exposure", "Dust protection", "Respiratory safety" },
            PPEType.HighVisibility => new[] { "Outdoor work", "Traffic areas", "Emergency response" },
            PPEType.FallProtection => new[] { "Height work", "Roof access", "Fall prevention" },
            _ => new[] { "General safety", "Department duties", "Protective equipment" }
        };

        var random = new Random();
        var basePurpose = purposes[random.Next(purposes.Length)];
        return $"{basePurpose} - {department} department";
    }

    private static PPECondition GetRandomReturnCondition(Random random)
    {
        var conditions = Enum.GetValues<PPECondition>();
        var weights = new[] { 0.5, 0.3, 0.15, 0.05 }; // Higher chance for better conditions
        var totalWeight = weights.Sum();
        var randomValue = random.NextDouble() * totalWeight;
        
        var cumulativeWeight = 0.0;
        for (int i = 0; i < weights.Length && i < conditions.Length; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue <= cumulativeWeight)
                return conditions[i];
        }
        
        return PPECondition.Good;
    }

    private static string GetRandomInspector(Random random)
    {
        var inspectors = new[]
        {
            "Safety Officer Johnson", "Maintenance Supervisor Lee", "HSE Manager Chen",
            "Quality Inspector Davis", "Facilities Manager Kim", "Safety Coordinator Brown"
        };
        return inspectors[random.Next(inspectors.Length)];
    }

    private static InspectionResult GetRandomInspectionResult(Random random)
    {
        var weights = new[] { 0.7, 0.2, 0.1 }; // 70% pass, 20% conditional, 10% fail
        var results = new[] { InspectionResult.Passed, InspectionResult.PassedWithObservations, InspectionResult.Failed };
        
        var randomValue = random.NextDouble();
        var cumulativeWeight = 0.0;
        
        for (int i = 0; i < weights.Length; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue <= cumulativeWeight)
                return results[i];
        }
        
        return InspectionResult.Passed;
    }

    private static PPECondition GetRandomPPECondition(Random random)
    {
        var conditions = Enum.GetValues<PPECondition>();
        var weights = new[] { 0.4, 0.35, 0.2, 0.05 }; // Favor better conditions
        var totalWeight = weights.Sum();
        var randomValue = random.NextDouble() * totalWeight;
        
        var cumulativeWeight = 0.0;
        for (int i = 0; i < weights.Length && i < conditions.Length; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue <= cumulativeWeight)
                return conditions[i];
        }
        
        return PPECondition.Good;
    }

    private static string GenerateInspectionFindings(InspectionResult result, PPECondition condition, string itemName, Random random)
    {
        return result switch
        {
            InspectionResult.Passed => condition switch
            {
                PPECondition.New => $"{itemName} in excellent condition, no issues found",
                PPECondition.Good => $"{itemName} in good working order, minor wear acceptable",
                PPECondition.Fair => $"{itemName} functional but showing wear, monitor for replacement",
                _ => $"{itemName} inspection passed"
            },
            InspectionResult.PassedWithObservations => $"{itemName} has minor issues but is safe for continued use with monitoring",
            InspectionResult.Failed => condition switch
            {
                PPECondition.Poor => $"{itemName} shows significant wear and damage, immediate replacement required",
                PPECondition.Damaged => $"{itemName} has structural damage, unsafe for use",
                _ => $"{itemName} failed inspection, safety concerns identified"
            },
            _ => $"Inspection completed for {itemName}"
        };
    }

    private static string? GenerateCorrectiveActions(string itemName, Random random)
    {
        var actions = new[]
        {
            $"Replace {itemName} immediately",
            $"Repair or refurbish {itemName} before next use",
            $"Remove {itemName} from service until corrective action completed",
            $"Investigate cause of damage to {itemName}",
            $"Review usage procedures for {itemName}"
        };
        
        return actions[random.Next(actions.Length)];
    }

    private static RequestPriority GetRandomUrgency(Random random)
    {
        var urgencies = Enum.GetValues<RequestPriority>();
        var weights = new[] { 0.1, 0.3, 0.4, 0.2 }; // Most requests are normal to high
        var totalWeight = weights.Sum();
        var randomValue = random.NextDouble() * totalWeight;
        
        var cumulativeWeight = 0.0;
        for (int i = 0; i < weights.Length && i < urgencies.Length; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue <= cumulativeWeight)
                return urgencies[i];
        }
        
        return RequestPriority.Medium;
    }

    private static string GenerateRequestJustification(string categoryName, string department, Random random)
    {
        var justifications = new[]
        {
            $"Replacement needed for worn out {categoryName}",
            $"Additional {categoryName} required for new team member",
            $"Current {categoryName} damaged during use",
            $"Upgrade to better quality {categoryName} for improved safety",
            $"{categoryName} needed for upcoming project work",
            $"Lost {categoryName} needs replacement",
            $"Seasonal increase in {department} activities requires additional {categoryName}"
        };
        
        return justifications[random.Next(justifications.Length)];
    }

    private static string GetRandomRejectionReason(Random random)
    {
        var reasons = new[]
        {
            "Insufficient budget allocation for this period",
            "Similar PPE already available in inventory",
            "Request does not meet minimum quantity requirements",
            "Alternative PPE solution recommended",
            "Request requires supervisor approval",
            "Current inventory sufficient for department needs"
        };
        
        return reasons[random.Next(reasons.Length)];
    }

    #endregion

    #region Helper Methods for Incident Extensions Data Generation

    private static string GenerateAttachmentFileName(IncidentSeverity severity, int index, Random random)
    {
        var fileTypes = new[]
        {
            ".jpg", ".png", ".pdf", ".doc", ".docx", ".mp4", ".mov"
        };
        
        var fileType = fileTypes[random.Next(fileTypes.Length)];
        var baseName = severity switch
        {
            IncidentSeverity.Critical or IncidentSeverity.Emergency => new[] { "emergency_response", "damage_assessment", "safety_report", "incident_photos" },
            IncidentSeverity.Serious or IncidentSeverity.Major => new[] { "incident_documentation", "witness_statement", "safety_analysis", "investigation_notes" },
            _ => new[] { "incident_report", "photo_evidence", "maintenance_log", "safety_checklist" }
        };
        
        var name = baseName[random.Next(baseName.Length)];
        return $"{name}_{index:D2}{fileType}";
    }

    private static InvolvementType GetRandomInvolvementType(Random random)
    {
        var types = Enum.GetValues<InvolvementType>();
        var weights = new[] { 0.5, 0.3, 0.2 }; // Witness most common, then victim, then first responder
        var totalWeight = weights.Sum();
        var randomValue = random.NextDouble() * totalWeight;
        
        var cumulativeWeight = 0.0;
        for (int i = 0; i < weights.Length && i < types.Length; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue <= cumulativeWeight)
                return types[i];
        }
        
        return InvolvementType.Witness;
    }

    private static string? GenerateInjuryDescription(IncidentSeverity severity, Random random)
    {
        var injuries = severity switch
        {
            IncidentSeverity.Critical or IncidentSeverity.Emergency => new[]
            {
                "Severe head trauma requiring immediate medical attention",
                "Multiple fractures and internal bleeding",
                "Third-degree burns covering significant body area",
                "Unconscious with signs of spinal injury"
            },
            IncidentSeverity.Serious or IncidentSeverity.Major => new[]
            {
                "Broken arm requiring cast and surgery",
                "Second-degree burns requiring medical treatment",
                "Concussion with temporary loss of consciousness",
                "Deep laceration requiring stitches"
            },
            _ => new[]
            {
                "Minor cuts and bruising",
                "Sprained ankle with swelling",
                "Small burn on hand",
                "Minor headache and dizziness"
            }
        };
        
        return injuries[random.Next(injuries.Length)];
    }

    private static List<(IncidentStatus Status, string OldStatus, DateTime Timestamp)> GenerateStatusChangeHistory(Incident incident, Random random)
    {
        var changes = new List<(IncidentStatus Status, string OldStatus, DateTime Timestamp)>();
        var currentDate = incident.CreatedAt;
        var currentStatus = "Reported";
        
        // Most incidents go through: Reported -> UnderInvestigation -> AwaitingAction -> Resolved/Closed
        var statusProgression = new[] { IncidentStatus.UnderInvestigation, IncidentStatus.AwaitingAction };
        
        foreach (var status in statusProgression)
        {
            if (random.NextDouble() < 0.8) // 80% chance of each status change
            {
                currentDate = currentDate.AddDays(random.Next(1, 7));
                changes.Add((status, currentStatus, currentDate));
                currentStatus = status.ToString();
            }
        }
        
        // Final resolution (80% resolved, 20% closed)
        if (random.NextDouble() < 0.8)
        {
            var finalStatus = random.NextDouble() < 0.8 ? IncidentStatus.Resolved : IncidentStatus.Closed;
            currentDate = currentDate.AddDays(random.Next(1, 14));
            changes.Add((finalStatus, currentStatus, currentDate));
        }
        
        return changes;
    }

    private static string GetRandomAuditActionType(Random random)
    {
        var actionTypes = new[]
        {
            AuditActions.Updated,
            AuditActions.CommentAdded,
            AuditActions.AttachmentAdded,
            AuditActions.CorrectiveActionAdded,
            AuditActions.InvolvedPersonAdded,
            AuditActions.Viewed
        };
        
        return actionTypes[random.Next(actionTypes.Length)];
    }

    private static string GenerateAuditActionDescription(string actionType, string userName, Random random)
    {
        return actionType switch
        {
            AuditActions.Updated => $"Incident details updated by {userName}",
            AuditActions.CommentAdded => $"Investigation notes added by {userName}",
            AuditActions.AttachmentAdded => $"Supporting documentation attached by {userName}",
            AuditActions.CorrectiveActionAdded => $"Corrective action plan developed by {userName}",
            AuditActions.InvolvedPersonAdded => $"Involved person added to incident by {userName}",
            AuditActions.Viewed => $"Incident reviewed by {userName}",
            _ => $"Action performed by {userName}"
        };
    }

    #endregion

    #region Helper Methods for Notification System Data Generation

    private static NotificationChannel GetRandomNotificationChannel(Random random)
    {
        var channels = Enum.GetValues<NotificationChannel>();
        var weights = new[] { 0.5, 0.2, 0.1, 0.1, 0.1 }; // Email most common
        var totalWeight = weights.Sum();
        var randomValue = random.NextDouble() * totalWeight;
        
        var cumulativeWeight = 0.0;
        for (int i = 0; i < weights.Length && i < channels.Length; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue <= cumulativeWeight)
                return channels[i];
        }
        
        return NotificationChannel.Email;
    }

    private static NotificationPriority GetNotificationPriority(IncidentSeverity severity, Random random)
    {
        return severity switch
        {
            IncidentSeverity.Critical or IncidentSeverity.Emergency => NotificationPriority.Emergency,
            IncidentSeverity.Serious or IncidentSeverity.Major => NotificationPriority.High,
            IncidentSeverity.Moderate => NotificationPriority.Normal,
            _ => NotificationPriority.Low
        };
    }

    private static NotificationStatus GetRandomNotificationStatus(Random random)
    {
        var statuses = Enum.GetValues<NotificationStatus>();
        var weights = new[] { 0.05, 0.15, 0.6, 0.05, 0.15 }; // Most delivered successfully
        var totalWeight = weights.Sum();
        var randomValue = random.NextDouble() * totalWeight;
        
        var cumulativeWeight = 0.0;
        for (int i = 0; i < weights.Length && i < statuses.Length; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue <= cumulativeWeight)
                return statuses[i];
        }
        
        return NotificationStatus.Delivered;
    }

    private static string GetNotificationTemplate(IncidentSeverity severity, NotificationChannel channel, Random random)
    {
        var templates = severity switch
        {
            IncidentSeverity.Critical or IncidentSeverity.Emergency => new[] { "emergency_alert", "critical_incident", "immediate_response" },
            IncidentSeverity.Serious or IncidentSeverity.Major => new[] { "serious_incident", "investigation_required", "management_alert" },
            _ => new[] { "incident_reported", "status_update", "routine_notification" }
        };
        
        var baseTemplate = templates[random.Next(templates.Length)];
        var channelSuffix = channel switch
        {
            NotificationChannel.Sms => "_sms",
            NotificationChannel.Push => "_push",
            NotificationChannel.WhatsApp => "_whatsapp",
            _ => "_email"
        };
        
        return $"{baseTemplate}{channelSuffix}";
    }

    private static string GenerateNotificationSubject(Incident incident, string templateId)
    {
        if (templateId.Contains("emergency") || templateId.Contains("critical"))
        {
            return $"[URGENT] Critical Incident Reported: {incident.Title}";
        }
        
        if (templateId.Contains("serious") || templateId.Contains("investigation"))
        {
            return $"[HIGH PRIORITY] Incident Requires Attention: {incident.Title}";
        }
        
        return $"Incident Notification: {incident.Title}";
    }

    private static string GenerateNotificationContent(Incident incident, string recipientName, string templateId)
    {
        var urgencyLevel = templateId.Contains("emergency") || templateId.Contains("critical") ? "IMMEDIATE ACTION REQUIRED" : "Please Review";
        
        return $"Dear {recipientName},\n\n" +
               $"{urgencyLevel}\n\n" +
               $"Incident: {incident.Title}\n" +
               $"Severity: {incident.Severity}\n" +
               $"Location: {incident.Location}\n" +
               $"Reported: {incident.IncidentDate:yyyy-MM-dd HH:mm}\n" +
               $"Reporter: {incident.ReporterName}\n\n" +
               $"Description: {incident.Description}\n\n" +
               $"Please log into the HSE system to review and take appropriate action.\n\n" +
               $"This is an automated notification from the Harmoni360 system.";
    }

    private static string GetNotificationReason(string templateId)
    {
        return templateId switch
        {
            var t when t.Contains("emergency") => "Emergency escalation protocol",
            var t when t.Contains("critical") => "Critical incident notification",
            var t when t.Contains("investigation") => "Investigation assignment",
            var t when t.Contains("status") => "Status update notification",
            _ => "Standard incident notification"
        };
    }

    private static string GetRandomNotificationError(NotificationChannel channel, Random random)
    {
        var errors = channel switch
        {
            NotificationChannel.Email => new[] {
                "Invalid email address", "Mail server timeout", "Recipient mailbox full", "SMTP authentication failed"
            },
            NotificationChannel.Sms => new[] {
                "Invalid phone number", "SMS service unavailable", "Message delivery failed", "Network timeout"
            },
            NotificationChannel.Push => new[] {
                "Device not registered", "Push service error", "Invalid token", "Device offline"
            },
            NotificationChannel.WhatsApp => new[] {
                "WhatsApp number not verified", "API rate limit exceeded", "Message template not approved", "Service unavailable"
            },
            _ => new[] { "Unknown error", "Service temporarily unavailable", "Network connectivity issue" }
        };
        
        return errors[random.Next(errors.Length)];
    }

    #endregion

    #region Helper Methods for Health Data Generation

    private static PersonType DeterminePersonType(string email)
    {
        if (email.Contains("student") || email.Contains("pupil"))
            return PersonType.Student;
        
        if (email.Contains("@bsj.sch.id") || email.Contains("@harmoni360.com"))
            return PersonType.Staff;
        
        if (email.Contains("contractor") || email.Contains("vendor"))
            return PersonType.Contractor;
        
        if (email.Contains("visitor") || email.Contains("guest"))
            return PersonType.Visitor;
        
        return PersonType.Staff; // Default
    }

    private static BloodType GetRandomBloodType(Random random)
    {
        var bloodTypes = Enum.GetValues<BloodType>();
        return bloodTypes[random.Next(bloodTypes.Length)];
    }

    private static string? GenerateMedicalNotes(PersonType personType, Random random)
    {
        if (random.NextDouble() < 0.7) // 70% have some medical notes
        {
            var notes = new[]
            {
                "No known allergies or medical conditions",
                "Regular health checkups up to date",
                "Wears glasses for reading",
                "Previous minor sports injuries, fully recovered",
                "Family history of diabetes - monitoring recommended",
                "Seasonal allergies - may need medication during spring",
                "No current medications",
                "Physical fitness level: good"
            };
            
            return notes[random.Next(notes.Length)];
        }
        
        return null;
    }

    private static string GenerateTreatmentPlan(string conditionName)
    {
        return conditionName.ToLower() switch
        {
            var name when name.Contains("allergy") => "Avoid allergen, carry emergency medication, inform all staff",
            var name when name.Contains("asthma") => "Daily inhaler, rescue inhaler available, monitor during physical activity",
            var name when name.Contains("diabetes") => "Monitor blood sugar regularly, maintain medication schedule, healthy diet",
            var name when name.Contains("anxiety") => "Stress management techniques, counseling support, quiet environment when needed",
            var name when name.Contains("celiac") => "Strict gluten-free diet, read all food labels, separate food preparation",
            var name when name.Contains("epilepsy") => "Medication compliance, avoid triggers, seizure action plan in place",
            _ => "Follow medical provider recommendations, regular monitoring"
        };
    }

    private static string GetRandomProvider(Random random)
    {
        var providers = new[]
        {
            "Dr. Sarah Johnson", "Nurse Patricia Lee", "Dr. Michael Chen", "Dr. Lisa Wang",
            "Nurse Jennifer Adams", "Dr. Robert Kim", "Dr. Maria Garcia", "Nurse David Liu",
            "Dr. Emily Brown", "Nurse Thomas Wilson"
        };
        
        return providers[random.Next(providers.Length)];
    }

    private static string GetRandomLocation(Random random)
    {
        var locations = new[]
        {
            "School Health Office", "Jakarta Medical Center", "Siloam Hospital", 
            "Community Health Clinic", "RSPI Pondok Indah", "Jakarta International Hospital",
            "Mayapada Hospital", "RS Premier Jatinegara", "Hermina Hospital"
        };
        
        return locations[random.Next(locations.Length)];
    }

    private static string? GenerateVaccineNotes(string vaccineName, bool administered, Random random)
    {
        if (!administered)
            return "Vaccination scheduled but not yet administered";
        
        if (random.NextDouble() < 0.3) // 30% have notes
        {
            var notes = new[]
            {
                "No adverse reactions reported",
                "Mild soreness at injection site for 24 hours",
                "Patient tolerated well",
                "Reminder given for next dose",
                "Documentation provided to family"
            };
            
            return notes[random.Next(notes.Length)];
        }
        
        return null;
    }

    private static ContactRelationship GetRandomRelationship(Random random, bool isPrimary)
    {
        if (isPrimary)
        {
            // Primary contacts are usually parents or guardians
            var primaryRelationships = new[] { ContactRelationship.Parent, ContactRelationship.Guardian, ContactRelationship.Spouse };
            return primaryRelationships[random.Next(primaryRelationships.Length)];
        }
        
        var relationships = Enum.GetValues<ContactRelationship>();
        return relationships[random.Next(relationships.Length)];
    }

    private static string GenerateContactName(ContactRelationship relationship, Random random)
    {
        var firstNames = new[] { "John", "Jane", "Michael", "Sarah", "David", "Lisa", "Robert", "Maria", "William", "Jennifer", "James", "Patricia", "Richard", "Linda", "Thomas", "Barbara" };
        var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas" };
        
        var firstName = firstNames[random.Next(firstNames.Length)];
        var lastName = lastNames[random.Next(lastNames.Length)];
        
        return $"{firstName} {lastName}";
    }

    private static string GeneratePhoneNumber(Random random)
    {
        // Generate Indonesian phone number format
        var formats = new[]
        {
            $"+62-8{random.Next(1, 9)}-{random.Next(1000, 9999)}-{random.Next(1000, 9999)}",
            $"08{random.Next(10, 99)}-{random.Next(1000, 9999)}-{random.Next(1000, 9999)}",
            $"+62-21-{random.Next(1000, 9999)}-{random.Next(1000, 9999)}" // Jakarta landline
        };
        
        return formats[random.Next(formats.Length)];
    }

    private static string GenerateEmail(Random random)
    {
        var usernames = new[] { "john.doe", "jane.smith", "contact", "emergency", "family", "parent", "guardian" };
        var domains = new[] { "@gmail.com", "@yahoo.com", "@hotmail.com", "@outlook.com", "@bsj.sch.id" };
        
        var username = usernames[random.Next(usernames.Length)];
        var domain = domains[random.Next(domains.Length)];
        var number = random.Next(1, 100);
        
        return $"{username}{number}{domain}";
    }

    private static string GenerateAddress(Random random)
    {
        var streets = new[] { "Jl. Sudirman", "Jl. Thamrin", "Jl. Kuningan", "Jl. Kemang", "Jl. Menteng", "Jl. Senopati", "Jl. Cipete" };
        var areas = new[] { "Jakarta Selatan", "Jakarta Pusat", "Jakarta Barat", "Jakarta Utara", "Jakarta Timur", "Tangerang", "Bekasi" };
        
        var street = streets[random.Next(streets.Length)];
        var area = areas[random.Next(areas.Length)];
        var number = random.Next(1, 200);
        
        return $"{street} No. {number}, {area}, Jakarta, Indonesia";
    }

    #endregion
}