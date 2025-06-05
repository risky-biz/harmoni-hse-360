using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Domain.Entities;
using HarmoniHSE360.Domain.ValueObjects;
using HarmoniHSE360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HarmoniHSE360.Infrastructure.Services;

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

        var superAdminRole = Role.Create("SuperAdmin", "Super Administrator with full system access");
        var developerRole = Role.Create("Developer", "Developer with system configuration access");
        var adminRole = Role.Create("Admin", "System Administrator");
        var hseManagerRole = Role.Create("HSEManager", "HSE Manager");
        var employeeRole = Role.Create("Employee", "Regular Employee");

        await _context.Roles.AddRangeAsync(superAdminRole, developerRole, adminRole, hseManagerRole, employeeRole);
        await _context.SaveChangesAsync(); // Save roles first to get IDs

        // Create unique permissions first
        var allPermissions = new List<Permission>
        {
            Permission.Create("incidents.view", "View Incidents", "Can view incident reports", "Incidents"),
            Permission.Create("incidents.create", "Create Incidents", "Can create new incident reports", "Incidents"),
            Permission.Create("incidents.update", "Update Incidents", "Can modify incident reports", "Incidents"),
            Permission.Create("incidents.delete", "Delete Incidents", "Can delete incident reports", "Incidents"),
            Permission.Create("incidents.investigate", "Investigate Incidents", "Can investigate and assign incidents", "Incidents"),
            Permission.Create("users.manage", "Manage Users", "Can manage user accounts and roles", "Users"),
            Permission.Create("reports.view", "View Reports", "Can view all reports and analytics", "Reports"),
            Permission.Create("reports.create", "Create Reports", "Can generate reports", "Reports"),
            Permission.Create("system.configure", "System Configuration", "Can access and modify system settings", "System"),
            Permission.Create("system.modules", "Module Management", "Can manage system modules and features", "System")
        };

        await _context.Permissions.AddRangeAsync(allPermissions);
        await _context.SaveChangesAsync(); // Save permissions first

        // Get permission references for role assignment
        var incidentsView = allPermissions.First(p => p.Name == "incidents.view");
        var incidentsCreate = allPermissions.First(p => p.Name == "incidents.create");
        var incidentsUpdate = allPermissions.First(p => p.Name == "incidents.update");
        var incidentsDelete = allPermissions.First(p => p.Name == "incidents.delete");
        var incidentsInvestigate = allPermissions.First(p => p.Name == "incidents.investigate");
        var usersManage = allPermissions.First(p => p.Name == "users.manage");
        var reportsView = allPermissions.First(p => p.Name == "reports.view");
        var reportsCreate = allPermissions.First(p => p.Name == "reports.create");
        var systemConfigure = allPermissions.First(p => p.Name == "system.configure");
        var systemModules = allPermissions.First(p => p.Name == "system.modules");

        // Assign permissions to roles
        // Super Admin gets all permissions including system configuration
        superAdminRole.AddPermission(incidentsView);
        superAdminRole.AddPermission(incidentsCreate);
        superAdminRole.AddPermission(incidentsUpdate);
        superAdminRole.AddPermission(incidentsDelete);
        superAdminRole.AddPermission(incidentsInvestigate);
        superAdminRole.AddPermission(usersManage);
        superAdminRole.AddPermission(reportsView);
        superAdminRole.AddPermission(reportsCreate);
        superAdminRole.AddPermission(systemConfigure);
        superAdminRole.AddPermission(systemModules);

        // Developer gets system configuration permissions and incidents
        developerRole.AddPermission(incidentsView);
        developerRole.AddPermission(incidentsCreate);
        developerRole.AddPermission(incidentsUpdate);
        developerRole.AddPermission(systemConfigure);
        developerRole.AddPermission(systemModules);

        // Admin gets all permissions except system configuration
        adminRole.AddPermission(incidentsView);
        adminRole.AddPermission(incidentsCreate);
        adminRole.AddPermission(incidentsUpdate);
        adminRole.AddPermission(incidentsDelete);
        adminRole.AddPermission(incidentsInvestigate);
        adminRole.AddPermission(usersManage);
        adminRole.AddPermission(reportsView);
        adminRole.AddPermission(reportsCreate);

        // HSE Manager gets incident and report permissions
        hseManagerRole.AddPermission(incidentsView);
        hseManagerRole.AddPermission(incidentsCreate);
        hseManagerRole.AddPermission(incidentsUpdate);
        hseManagerRole.AddPermission(incidentsInvestigate);
        hseManagerRole.AddPermission(reportsView);
        hseManagerRole.AddPermission(reportsCreate);

        // Employee gets basic incident permissions
        employeeRole.AddPermission(incidentsView);
        employeeRole.AddPermission(incidentsCreate);
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

        var superAdminRole = await _context.Roles.FirstAsync(r => r.Name == "SuperAdmin");
        var developerRole = await _context.Roles.FirstAsync(r => r.Name == "Developer");
        var adminRole = await _context.Roles.FirstAsync(r => r.Name == "Admin");
        var hseManagerRole = await _context.Roles.FirstAsync(r => r.Name == "HSEManager");
        var employeeRole = await _context.Roles.FirstAsync(r => r.Name == "Employee");

        // Hash demo passwords for production use
        var passwordHashService = new PasswordHashService();

        var users = new List<User>
        {
            User.Create("superadmin@bsj.sch.id", passwordHashService.HashPassword("SuperAdmin123!"), "Super Administrator", "SA001", "IT", "Super Administrator"),
            User.Create("developer@bsj.sch.id", passwordHashService.HashPassword("Developer123!"), "System Developer", "DEV001", "IT", "Software Developer"),
            User.Create("admin@bsj.sch.id", passwordHashService.HashPassword("Admin123!"), "System Administrator", "ADM001", "IT", "System Administrator"),
            User.Create("hse.manager@bsj.sch.id", passwordHashService.HashPassword("HSE123!"), "HSE Manager", "HSE001", "Health & Safety", "HSE Manager"),
            User.Create("john.doe@bsj.sch.id", passwordHashService.HashPassword("Employee123!"), "John Doe", "EMP001", "Facilities", "Maintenance Supervisor"),
            User.Create("jane.smith@bsj.sch.id", passwordHashService.HashPassword("Employee123!"), "Jane Smith", "EMP002", "Academic", "Teacher")
        };

        // Assign roles
        users[0].AssignRole(superAdminRole);
        users[1].AssignRole(developerRole);
        users[2].AssignRole(adminRole);
        users[3].AssignRole(hseManagerRole);
        users[4].AssignRole(employeeRole);
        users[5].AssignRole(employeeRole);

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
        var hseManager = await _context.Users.FirstAsync(u => u.Email == "hse.manager@bsj.sch.id");
        var employee1 = await _context.Users.FirstAsync(u => u.Email == "john.doe@bsj.sch.id");
        var employee2 = await _context.Users.FirstAsync(u => u.Email == "jane.smith@bsj.sch.id");

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
}