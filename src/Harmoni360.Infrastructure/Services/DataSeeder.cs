using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Infrastructure.Persistence;
using Harmoni360.Infrastructure.Services.DataSeeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services;

public class DataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataSeeder> _logger;
    private readonly IConfiguration _configuration;
    
    // Individual data seeders
    private readonly RoleDataSeeder _roleDataSeeder;
    private readonly ModulePermissionDataSeeder _modulePermissionDataSeeder;
    private readonly RoleModulePermissionDataSeeder _roleModulePermissionDataSeeder;
    private readonly UserDataSeeder _userDataSeeder;
    private readonly IncidentDataSeeder _incidentDataSeeder;
    private readonly PPEItemDataSeeder _ppeItemDataSeeder;
    private readonly HazardDataSeeder _hazardDataSeeder;
    private readonly HealthDataSeeder _healthDataSeeder;
    private readonly SecurityDataSeeder _securityDataSeeder;
    private readonly ConfigurationDataSeeder _configurationDataSeeder;

    public DataSeeder(
        ApplicationDbContext context, 
        ILogger<DataSeeder> logger, 
        IConfiguration configuration,
        RoleDataSeeder roleDataSeeder,
        ModulePermissionDataSeeder modulePermissionDataSeeder,
        RoleModulePermissionDataSeeder roleModulePermissionDataSeeder,
        UserDataSeeder userDataSeeder,
        IncidentDataSeeder incidentDataSeeder,
        PPEItemDataSeeder ppeItemDataSeeder,
        HazardDataSeeder hazardDataSeeder,
        HealthDataSeeder healthDataSeeder,
        SecurityDataSeeder securityDataSeeder,
        ConfigurationDataSeeder configurationDataSeeder)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        
        _roleDataSeeder = roleDataSeeder;
        _modulePermissionDataSeeder = modulePermissionDataSeeder;
        _roleModulePermissionDataSeeder = roleModulePermissionDataSeeder;
        _userDataSeeder = userDataSeeder;
        _incidentDataSeeder = incidentDataSeeder;
        _ppeItemDataSeeder = ppeItemDataSeeder;
        _hazardDataSeeder = hazardDataSeeder;
        _healthDataSeeder = healthDataSeeder;
        _securityDataSeeder = securityDataSeeder;
        _configurationDataSeeder = configurationDataSeeder;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting comprehensive data seeding using modular seeders...");

            // Get seeding configuration - handle both boolean and string values
            var forceReseedValue = _configuration["DataSeeding:ForceReseed"];
            var forceReseed = string.Equals(forceReseedValue, "true", StringComparison.OrdinalIgnoreCase) || 
                             string.Equals(forceReseedValue, "True", StringComparison.OrdinalIgnoreCase) ||
                             (bool.TryParse(forceReseedValue, out var boolResult) && boolResult);
            
            var essentialValue = _configuration["DataSeeding:Categories:Essential"];
            var seedEssential = essentialValue == null || string.Equals(essentialValue, "true", StringComparison.OrdinalIgnoreCase) || 
                               (bool.TryParse(essentialValue, out var essentialBool) && essentialBool);
            
            var sampleDataValue = _configuration["DataSeeding:Categories:SampleData"];
            var seedSampleData = string.Equals(sampleDataValue, "true", StringComparison.OrdinalIgnoreCase) || 
                                (bool.TryParse(sampleDataValue, out var sampleDataBool) && sampleDataBool);
            
            var userAccountsValue = _configuration["DataSeeding:Categories:UserAccounts"];
            var seedUserAccounts = string.Equals(userAccountsValue, "true", StringComparison.OrdinalIgnoreCase) || 
                                  (bool.TryParse(userAccountsValue, out var userAccountsBool) && userAccountsBool);
            
            _logger.LogInformation("Seeding configuration - Essential: {Essential}, SampleData: {SampleData}, UserAccounts: {UserAccounts}, ForceReseed: {ForceReseed}",
                seedEssential, seedSampleData, seedUserAccounts, forceReseed);

            // Handle ForceReseed - completely reset everything including identity columns
            if (forceReseed)
            {
                _logger.LogInformation("ForceReseed enabled - completely resetting database...");
                await ResetDatabaseAsync();
                
                // Set environment variable to tell other seeders to skip existence checks
                Environment.SetEnvironmentVariable("HARMONI_FORCE_RESEED", "true");
            }

            // ESSENTIAL: Core application settings for ALL modules
            if (seedEssential)
            {
                _logger.LogInformation("Phase 1: Seeding ESSENTIAL data...");
                _logger.LogInformation("  - Roles and Permissions");
                _logger.LogInformation("  - ALL Module Configuration Settings (PPE, Incident, Risk Management, etc.)");
                _logger.LogInformation("  - 3 Essential Admin Users (superadmin@harmoni360.com, developer@harmoni360.com, admin@harmoni360.com)");
                
                // 1. Roles and Permissions
                await _roleDataSeeder.SeedAsync();
                await _context.SaveChangesAsync();

                await _modulePermissionDataSeeder.SeedAsync();
                await _context.SaveChangesAsync();

                await _roleModulePermissionDataSeeder.SeedAsync();
                await _context.SaveChangesAsync();

                // 2. ALL Configuration Data for ALL modules
                await _configurationDataSeeder.SeedAsync(forceReseed);
                await _context.SaveChangesAsync();

                // 3. Essential admin users (superadmin, developer, admin)
                await _userDataSeeder.SeedEssentialAdminUsersAsync(forceReseed);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Essential data seeding completed");
            }

            // USER ACCOUNTS: Sample user accounts (separate from essential admins)
            // NOTE: This must come BEFORE SampleData because sample data references these users
            if (seedUserAccounts)
            {
                _logger.LogInformation("Phase 2: Seeding SAMPLE USER ACCOUNTS...");
                _logger.LogInformation("DEBUG: seedUserAccounts={SeedUserAccounts}, forceReseed={ForceReseed}", seedUserAccounts, forceReseed);
                _logger.LogInformation("  - Module-specific manager accounts");
                _logger.LogInformation("  - Reporter and viewer accounts");
                _logger.LogInformation("  - Legacy compatibility accounts");
                
                await _userDataSeeder.SeedSampleUserAccountsAsync(forceReseed);
                await _context.SaveChangesAsync();
                
                var userCount = await _context.Users.CountAsync();
                _logger.LogInformation("Sample user accounts seeding completed. Total users now: {UserCount}", userCount);
            }

            // SAMPLE DATA: All sample/transaction data for all modules with real relationships
            // NOTE: This comes AFTER UserAccounts because it references the users created above
            if (seedSampleData)
            {
                _logger.LogInformation("Phase 3: Seeding SAMPLE DATA for all modules...");
                _logger.LogInformation("DEBUG: seedSampleData={SeedSampleData}, forceReseed={ForceReseed}", seedSampleData, forceReseed);
                _logger.LogInformation("  - Incident Management sample data (View Incidents, My Reports)");
                _logger.LogInformation("  - Risk Management sample data (Hazard Register, My Hazards, Risk Assessments)");
                _logger.LogInformation("  - PPE Management sample data (PPE Inventory, PPE Management)");
                _logger.LogInformation("  - Health Management sample data");
                _logger.LogInformation("  - Security Management sample data");
                
                // Sample data with real relationships between modules
                _logger.LogInformation("DEBUG: Calling IncidentDataSeeder...");
                await _incidentDataSeeder.SeedAsync();
                await _context.SaveChangesAsync();
                var incidentCount = await _context.Incidents.CountAsync();
                _logger.LogInformation("DEBUG: Incidents seeded. Count: {IncidentCount}", incidentCount);

                _logger.LogInformation("DEBUG: Calling HazardDataSeeder...");
                await _hazardDataSeeder.SeedAsync();
                await _context.SaveChangesAsync();
                var hazardCount = await _context.Hazards.CountAsync();
                _logger.LogInformation("DEBUG: Hazards seeded. Count: {HazardCount}", hazardCount);
                
                _logger.LogInformation("DEBUG: Calling PPEItemDataSeeder...");
                await _ppeItemDataSeeder.SeedAsync();
                await _context.SaveChangesAsync();
                var ppeCount = await _context.PPEItems.CountAsync();
                _logger.LogInformation("DEBUG: PPE Items seeded. Count: {PPECount}", ppeCount);

                _logger.LogInformation("DEBUG: Calling HealthDataSeeder...");
                await _healthDataSeeder.SeedAsync();
                await _context.SaveChangesAsync();
                var healthCount = await _context.HealthRecords.CountAsync();
                _logger.LogInformation("DEBUG: Health Records seeded. Count: {HealthCount}", healthCount);

                _logger.LogInformation("DEBUG: Calling SecurityDataSeeder...");
                await _securityDataSeeder.SeedAsync();
                await _context.SaveChangesAsync();
                var securityCount = await _context.SecurityIncidents.CountAsync();
                _logger.LogInformation("DEBUG: Security Incidents seeded. Count: {SecurityCount}", securityCount);
                
                _logger.LogInformation("Sample data seeding completed");
            }

            _logger.LogInformation("Database seeding completed successfully");
            
            // Clear the ForceReseed environment variable
            if (forceReseed)
            {
                Environment.SetEnvironmentVariable("HARMONI_FORCE_RESEED", null);
                _logger.LogInformation("ForceReseed environment variable cleared");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding database");
            // Clear the ForceReseed environment variable even if there's an error
            var forceReseedValue = _configuration["DataSeeding:ForceReseed"];
            var wasForceReseed = string.Equals(forceReseedValue, "true", StringComparison.OrdinalIgnoreCase) || 
                                string.Equals(forceReseedValue, "True", StringComparison.OrdinalIgnoreCase) ||
                                (bool.TryParse(forceReseedValue, out var boolResult) && boolResult);
            if (wasForceReseed)
            {
                Environment.SetEnvironmentVariable("HARMONI_FORCE_RESEED", null);
            }
            throw;
        }
    }

    private async Task ResetDatabaseAsync()
    {
        _logger.LogInformation("Resetting database - removing all data and resetting identity columns...");
        
        try
        {
            // Remove data in proper dependency order
            
            // 1. Remove dependent data first
            _logger.LogInformation("Removing dependent data...");
            
            // PPE dependencies
            if (await _context.PPEAssignments.AnyAsync())
            {
                _context.PPEAssignments.RemoveRange(_context.PPEAssignments);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.PPEInspections.AnyAsync())
            {
                _context.PPEInspections.RemoveRange(_context.PPEInspections);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.PPERequests.AnyAsync())
            {
                _context.PPERequests.RemoveRange(_context.PPERequests);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.PPEItems.AnyAsync())
            {
                _context.PPEItems.RemoveRange(_context.PPEItems);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.PPEComplianceRequirements.AnyAsync())
            {
                _context.PPEComplianceRequirements.RemoveRange(_context.PPEComplianceRequirements);
                await _context.SaveChangesAsync();
            }
            
            // Incident dependencies
            if (await _context.IncidentAttachments.AnyAsync())
            {
                _context.IncidentAttachments.RemoveRange(_context.IncidentAttachments);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.IncidentInvolvedPersons.AnyAsync())
            {
                _context.IncidentInvolvedPersons.RemoveRange(_context.IncidentInvolvedPersons);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.CorrectiveActions.AnyAsync())
            {
                _context.CorrectiveActions.RemoveRange(_context.CorrectiveActions);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.Incidents.AnyAsync())
            {
                _context.Incidents.RemoveRange(_context.Incidents);
                await _context.SaveChangesAsync();
            }
            
            // Hazard dependencies
            if (await _context.HazardAttachments.AnyAsync())
            {
                _context.HazardAttachments.RemoveRange(_context.HazardAttachments);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.HazardMitigationActions.AnyAsync())
            {
                _context.HazardMitigationActions.RemoveRange(_context.HazardMitigationActions);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.RiskAssessments.AnyAsync())
            {
                _context.RiskAssessments.RemoveRange(_context.RiskAssessments);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.Hazards.AnyAsync())
            {
                _context.Hazards.RemoveRange(_context.Hazards);
                await _context.SaveChangesAsync();
            }
            
            // Health records
            if (await _context.HealthRecords.AnyAsync())
            {
                _context.HealthRecords.RemoveRange(_context.HealthRecords);
                await _context.SaveChangesAsync();
            }
            
            // Security-related data (must be removed before users due to FK constraints)
            _logger.LogInformation("Removing security-related data...");
            
            if (await _context.SecurityIncidentAttachments.AnyAsync())
            {
                _context.SecurityIncidentAttachments.RemoveRange(_context.SecurityIncidentAttachments);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.SecurityIncidentInvolvedPersons.AnyAsync())
            {
                _context.SecurityIncidentInvolvedPersons.RemoveRange(_context.SecurityIncidentInvolvedPersons);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.SecurityIncidentResponses.AnyAsync())
            {
                _context.SecurityIncidentResponses.RemoveRange(_context.SecurityIncidentResponses);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.ThreatAssessments.AnyAsync())
            {
                _context.ThreatAssessments.RemoveRange(_context.ThreatAssessments);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.ThreatIndicators.AnyAsync())
            {
                _context.ThreatIndicators.RemoveRange(_context.ThreatIndicators);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.SecurityAuditLogs.AnyAsync())
            {
                _context.SecurityAuditLogs.RemoveRange(_context.SecurityAuditLogs);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.SecurityIncidents.AnyAsync())
            {
                _context.SecurityIncidents.RemoveRange(_context.SecurityIncidents);
                await _context.SaveChangesAsync();
            }
            
            // Security controls (must be removed before users due to ImplementedById FK)
            if (await _context.SecurityControls.AnyAsync())
            {
                _context.SecurityControls.RemoveRange(_context.SecurityControls);
                await _context.SaveChangesAsync();
            }
            
            // 2. Remove configuration data
            _logger.LogInformation("Removing configuration data...");
            
            if (await _context.HazardTypes.AnyAsync())
            {
                _context.HazardTypes.RemoveRange(_context.HazardTypes);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.HazardCategories.AnyAsync())
            {
                _context.HazardCategories.RemoveRange(_context.HazardCategories);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.PPECategories.AnyAsync())
            {
                _context.PPECategories.RemoveRange(_context.PPECategories);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.PPESizes.AnyAsync())
            {
                _context.PPESizes.RemoveRange(_context.PPESizes);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.PPEStorageLocations.AnyAsync())
            {
                _context.PPEStorageLocations.RemoveRange(_context.PPEStorageLocations);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.Departments.AnyAsync())
            {
                _context.Departments.RemoveRange(_context.Departments);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.IncidentCategories.AnyAsync())
            {
                _context.IncidentCategories.RemoveRange(_context.IncidentCategories);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.IncidentLocations.AnyAsync())
            {
                _context.IncidentLocations.RemoveRange(_context.IncidentLocations);
                await _context.SaveChangesAsync();
            }
            
            // 3. Remove users (before roles)
            _logger.LogInformation("Removing users...");
            if (await _context.Users.AnyAsync())
            {
                _context.Users.RemoveRange(_context.Users);
                await _context.SaveChangesAsync();
            }
            
            // 4. Remove roles and permissions
            _logger.LogInformation("Removing roles and permissions...");
            
            if (await _context.RoleModulePermissions.AnyAsync())
            {
                _context.RoleModulePermissions.RemoveRange(_context.RoleModulePermissions);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.ModulePermissions.AnyAsync())
            {
                _context.ModulePermissions.RemoveRange(_context.ModulePermissions);
                await _context.SaveChangesAsync();
            }
            
            if (await _context.Roles.AnyAsync())
            {
                _context.Roles.RemoveRange(_context.Roles);
                await _context.SaveChangesAsync();
            }
            
            // 5. Reset identity columns
            await ResetIdentityColumnsAsync();
            
            _logger.LogInformation("Database reset completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database reset");
            throw;
        }
    }

    private async Task ResetIdentityColumnsAsync()
    {
        _logger.LogInformation("Resetting identity columns...");
        
        var tables = new[]
        {
            "Users", "Roles", "ModulePermissions", "RoleModulePermissions",
            "Departments", "IncidentCategories", "IncidentLocations", 
            "HazardCategories", "HazardTypes", "PPECategories", "PPESizes", "PPEStorageLocations",
            "Incidents", "Hazards", "PPEItems", "PPEAssignments", "PPEInspections", "PPERequests", 
            "PPEComplianceRequirements", "HealthRecords", 
            "SecurityIncidents", "SecurityControls", "SecurityIncidentAttachments", "SecurityIncidentInvolvedPersons",
            "SecurityIncidentResponses", "ThreatAssessments", "ThreatIndicators", "SecurityAuditLogs"
        };

        foreach (var table in tables)
        {
            var sql = $"ALTER SEQUENCE \"{table}_Id_seq\" RESTART WITH 1";
            await _context.Database.ExecuteSqlRawAsync(sql);
        }
        
        _logger.LogInformation("Identity columns reset completed");
    }
}