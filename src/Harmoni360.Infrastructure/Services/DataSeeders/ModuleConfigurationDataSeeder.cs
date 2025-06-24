using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class ModuleConfigurationDataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ModuleConfigurationDataSeeder> _logger;
    private readonly IConfiguration _configuration;
    private readonly IModuleDiscoveryService _moduleDiscoveryService;

    public ModuleConfigurationDataSeeder(
        ApplicationDbContext context, 
        ILogger<ModuleConfigurationDataSeeder> logger, 
        IConfiguration configuration,
        IModuleDiscoveryService moduleDiscoveryService)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _moduleDiscoveryService = moduleDiscoveryService;
    }

    public async Task SeedAsync()
    {
        var moduleConfigurationCount = await _context.ModuleConfigurations.CountAsync();
        var forceReseedValue = _configuration["DataSeeding:ForceReseed"];
        var forceReseed = string.Equals(forceReseedValue, "true", StringComparison.OrdinalIgnoreCase) || 
                         string.Equals(forceReseedValue, "True", StringComparison.OrdinalIgnoreCase) ||
                         (bool.TryParse(forceReseedValue, out var boolResult) && boolResult);
        var reSeedModuleConfigurations = bool.Parse(_configuration["DataSeeding:ReSeedModuleConfigurations"] ?? "false");
        _logger.LogInformation("Current module configuration count: {ModuleConfigurationCount}, ForceReseed: {ForceReseed}, ReSeed: {ReSeed}", 
            moduleConfigurationCount, forceReseed, reSeedModuleConfigurations);
        
        if (!forceReseed && moduleConfigurationCount > 0 && !reSeedModuleConfigurations)
        {
            _logger.LogInformation("Module configurations already exist, skipping seeding");
            return;
        }
        
        if ((reSeedModuleConfigurations || forceReseed) && moduleConfigurationCount > 0)
        {
            _logger.LogInformation("ReSeedModuleConfigurations or ForceReseed is enabled, clearing existing module configurations...");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"ModuleDependencies\"");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"ModuleConfigurations\"");
            _logger.LogInformation("Existing module configurations cleared");
        }

        _logger.LogInformation("Starting dynamic module configuration seeding via assembly discovery...");

        try
        {
            // Use dynamic discovery to get module configurations
            var discoveredConfigurations = await _moduleDiscoveryService.GetModuleConfigurationsAsync();
            var discoveredDependencies = await _moduleDiscoveryService.GetModuleDependenciesAsync();

            var moduleConfigurations = discoveredConfigurations.ToList();
            var moduleDependencies = discoveredDependencies.ToList();

            _logger.LogInformation("Discovered {ConfigCount} modules and {DepCount} dependencies via assembly scanning", 
                moduleConfigurations.Count, moduleDependencies.Count);

            // Add core modules that might not be discovered (fallback)
            await EnsureCoreModulesExist(moduleConfigurations);

            // Add all configurations to context
            await _context.ModuleConfigurations.AddRangeAsync(moduleConfigurations);
            await _context.SaveChangesAsync(); // Save configurations first to establish foreign key relationships
            
            // Filter dependencies to only include those where both modules exist
            var validDependencies = moduleDependencies.Where(dep => 
                moduleConfigurations.Any(m => m.ModuleType == dep.ModuleType) &&
                moduleConfigurations.Any(m => m.ModuleType == dep.DependsOnModuleType)
            ).ToList();
            
            if (validDependencies.Any())
            {
                await _context.ModuleDependencies.AddRangeAsync(validDependencies);
                await _context.SaveChangesAsync(); // Save dependencies
            }

            _logger.LogInformation("Successfully seeded {ConfigCount} module configurations and {DepCount} module dependencies via dynamic discovery", 
                moduleConfigurations.Count, moduleDependencies.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during dynamic module seeding, falling back to static seeding");
            await FallbackToStaticSeeding();
        }
    }

    /// <summary>
    /// Ensures core system modules exist even if not discovered
    /// </summary>
    private async Task EnsureCoreModulesExist(List<ModuleConfiguration> moduleConfigurations)
    {
        var coreModules = new[]
        {
            (ModuleType.Dashboard, "Dashboard", "Main dashboard and overview", "fas fa-tachometer-alt", 0),
            (ModuleType.UserManagement, "User Management", "User accounts, roles, and permissions", "fas fa-users", 1),
            (ModuleType.ApplicationSettings, "Application Settings", "System configuration and settings", "fas fa-cog", 2)
        };

        foreach (var (moduleType, displayName, description, iconClass, displayOrder) in coreModules)
        {
            if (!moduleConfigurations.Any(m => m.ModuleType == moduleType))
            {
                _logger.LogInformation("Adding core module {ModuleType} as it was not discovered", moduleType);
                
                moduleConfigurations.Add(new ModuleConfiguration
                {
                    ModuleType = moduleType,
                    DisplayName = displayName,
                    Description = description,
                    IconClass = iconClass,
                    DisplayOrder = displayOrder,
                    IsEnabled = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
    }

    /// <summary>
    /// Fallback to minimal static seeding if dynamic discovery fails
    /// </summary>
    private async Task FallbackToStaticSeeding()
    {
        _logger.LogWarning("Using fallback static module seeding...");
        
        // Clear the context to avoid tracking conflicts
        _context.ChangeTracker.Clear();
        
        // Check if modules already exist to avoid duplicates
        var existingModules = await _context.ModuleConfigurations
            .Select(m => m.ModuleType)
            .ToListAsync();
        
        var coreModuleTypes = new[] { ModuleType.Dashboard, ModuleType.UserManagement, ModuleType.ApplicationSettings };
        var missingModules = coreModuleTypes.Where(m => !existingModules.Contains(m)).ToList();
        
        if (!missingModules.Any())
        {
            _logger.LogInformation("All core modules already exist, skipping fallback seeding");
            return;
        }
        
        var moduleConfigurations = new List<ModuleConfiguration>();
        
        foreach (var moduleType in missingModules)
        {
            var (displayName, description, iconClass, displayOrder) = moduleType switch
            {
                ModuleType.Dashboard => ("Dashboard", "Main dashboard and overview", "fas fa-tachometer-alt", 0),
                ModuleType.UserManagement => ("User Management", "User accounts, roles, and permissions", "fas fa-users", 1),
                ModuleType.ApplicationSettings => ("Application Settings", "System configuration and settings", "fas fa-cog", 2),
                _ => ("Unknown", "Unknown module", "fas fa-question", 999)
            };
            
            moduleConfigurations.Add(new ModuleConfiguration
            {
                ModuleType = moduleType,
                DisplayName = displayName,
                Description = description,
                IconClass = iconClass,
                DisplayOrder = displayOrder,
                IsEnabled = true,
                CreatedBy = "system"
            });
        }

        if (moduleConfigurations.Any())
        {
            await _context.ModuleConfigurations.AddRangeAsync(moduleConfigurations);
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Fallback seeding completed with {Count} core modules", moduleConfigurations.Count);
    }
}