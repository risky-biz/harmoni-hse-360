using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Infrastructure.Persistence;
using Harmoni360.Infrastructure.Services.DataSeeders;
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
    private readonly PPEDataSeeder _ppeDataSeeder;
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
        PPEDataSeeder ppeDataSeeder,
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
        _ppeDataSeeder = ppeDataSeeder;
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

            // Phase 1: Core Authorization System
            _logger.LogInformation("Phase 1: Seeding core authorization system...");
            
            await _roleDataSeeder.SeedAsync();
            await _context.SaveChangesAsync();

            await _modulePermissionDataSeeder.SeedAsync();
            await _context.SaveChangesAsync();

            await _roleModulePermissionDataSeeder.SeedAsync();
            await _context.SaveChangesAsync();

            await _userDataSeeder.SeedAsync();
            await _context.SaveChangesAsync();

            // Phase 2: Configuration Data
            _logger.LogInformation("Phase 2: Seeding configuration data...");
            await _configurationDataSeeder.SeedAsync();

            // Phase 3: Core Business Data (conditional based on configuration)
            _logger.LogInformation("Phase 3: Seeding core business data...");

            // Seed PPE data if enabled in configuration
            var seedPPEData = _configuration["DataSeeding:SeedPPEData"] != "false";
            if (seedPPEData)
            {
                await _ppeDataSeeder.SeedAsync();
                await _context.SaveChangesAsync();

                // Seed PPE items after PPE base data
                var seedPPEItems = _configuration["DataSeeding:SeedPPEItems"] != "false";
                if (seedPPEItems)
                {
                    await _ppeItemDataSeeder.SeedAsync();
                    await _context.SaveChangesAsync();
                }
            }

            // Seed incidents if enabled in configuration
            var seedIncidents = _configuration["DataSeeding:SeedIncidents"] != "false";
            if (seedIncidents)
            {
                await _incidentDataSeeder.SeedAsync();
                await _context.SaveChangesAsync();
            }

            // Seed hazards if enabled in configuration
            var seedHazards = _configuration["DataSeeding:SeedHazards"] != "false";
            if (seedHazards)
            {
                await _hazardDataSeeder.SeedAsync();
                await _context.SaveChangesAsync();
            }

            // Seed health data if enabled in configuration
            var seedHealthData = _configuration["DataSeeding:SeedHealthData"] != "false";
            if (seedHealthData)
            {
                await _healthDataSeeder.SeedAsync();
                await _context.SaveChangesAsync();
            }

            // Seed security data if enabled in configuration
            var seedSecurityData = _configuration["DataSeeding:SeedSecurityData"] != "false";
            if (seedSecurityData)
            {
                await _securityDataSeeder.SeedAsync();
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Database seeding completed successfully using modular approach");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding database");
            throw;
        }
    }
}