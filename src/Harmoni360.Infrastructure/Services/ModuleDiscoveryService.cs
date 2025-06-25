using System.Reflection;
using Harmoni360.Application.Common.Attributes;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services;

/// <summary>
/// Service for discovering modules via assembly scanning
/// </summary>
public class ModuleDiscoveryService : IModuleDiscoveryService
{
    private readonly ILogger<ModuleDiscoveryService> _logger;
    private readonly ICurrentUserService _currentUserService;

    public ModuleDiscoveryService(ILogger<ModuleDiscoveryService> logger, ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<ModuleDiscoveryResult>> DiscoverModulesAsync()
    {
        _logger.LogInformation("Starting module discovery via assembly scanning...");
        
        var results = new List<ModuleDiscoveryResult>();
        
        // Get all loaded assemblies that might contain modules
        var assemblies = GetRelevantAssemblies();
        
        foreach (var assembly in assemblies)
        {
            try
            {
                _logger.LogDebug("Scanning assembly: {AssemblyName}", assembly.GetName().Name);
                
                var types = assembly.GetTypes()
                    .Where(t => t.GetCustomAttribute<ModuleAttribute>() != null);
                
                foreach (var type in types)
                {
                    var moduleAttribute = type.GetCustomAttribute<ModuleAttribute>()!;
                    
                    results.Add(new ModuleDiscoveryResult
                    {
                        ModuleType = type,
                        ModuleAttribute = moduleAttribute,
                        AssemblyName = assembly.GetName().Name ?? "Unknown",
                        Namespace = type.Namespace ?? "Unknown"
                    });
                    
                    _logger.LogDebug("Discovered module: {ModuleType} in {Assembly}", 
                        moduleAttribute.ModuleType, assembly.GetName().Name);
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                _logger.LogWarning("Failed to scan assembly {AssemblyName}: {Error}", 
                    assembly.GetName().Name, ex.Message);
                
                // Log which types failed to load
                foreach (var loaderException in ex.LoaderExceptions.Where(e => e != null))
                {
                    _logger.LogDebug("Type loading error: {Error}", loaderException!.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning assembly {AssemblyName}", assembly.GetName().Name);
            }
        }
        
        _logger.LogInformation("Module discovery completed. Found {Count} modules", results.Count);
        
        return results;
    }

    public async Task<IEnumerable<ModuleConfiguration>> GetModuleConfigurationsAsync()
    {
        var discoveredModules = await DiscoverModulesAsync();
        var configurations = new List<ModuleConfiguration>();
        var currentUser = _currentUserService?.UserId.ToString() ?? "system";

        foreach (var discoveredModule in discoveredModules)
        {
            var attr = discoveredModule.ModuleAttribute;
            
            var configuration = new ModuleConfiguration
            {
                ModuleType = attr.ModuleType,
                DisplayName = attr.DisplayName,
                Description = attr.Description,
                IconClass = attr.IconClass,
                DisplayOrder = attr.DisplayOrder,
                IsEnabled = attr.IsEnabledByDefault,
                ParentModuleType = null, // No parent module support for now
                CreatedBy = currentUser,
                CreatedAt = DateTime.UtcNow
            };

            configurations.Add(configuration);
        }

        return configurations.OrderBy(c => c.DisplayOrder);
    }

    public async Task<IEnumerable<ModuleDependency>> GetModuleDependenciesAsync()
    {
        var discoveredModules = await DiscoverModulesAsync();
        var dependencies = new List<ModuleDependency>();
        var currentUser = _currentUserService?.UserId.ToString() ?? "system";

        // Get all discovered module types for validation
        var discoveredModuleTypes = new HashSet<ModuleType>(discoveredModules.Select(m => m.ModuleAttribute.ModuleType));

        foreach (var discoveredModule in discoveredModules)
        {
            var attr = discoveredModule.ModuleAttribute;

            // Add required dependencies - only if both modules are discovered
            foreach (var requiredDep in attr.RequiredDependencies)
            {
                if (Enum.TryParse<ModuleType>(requiredDep, out var depModuleType))
                {
                    // Only add dependency if the target module is also discovered
                    if (discoveredModuleTypes.Contains(depModuleType))
                    {
                        dependencies.Add(new ModuleDependency
                        {
                            ModuleType = attr.ModuleType,
                            DependsOnModuleType = depModuleType,
                            IsRequired = true,
                            Description = $"Required dependency: {attr.DisplayName} requires {requiredDep}",
                            CreatedBy = currentUser,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        _logger.LogWarning("Skipping dependency from {ModuleType} to {DependencyType} - target module not discovered", 
                            attr.ModuleType, requiredDep);
                    }
                }
                else
                {
                    _logger.LogWarning("Invalid required dependency '{Dependency}' for module {ModuleType}", 
                        requiredDep, attr.ModuleType);
                }
            }

            // Add optional dependencies - only if both modules are discovered
            foreach (var optionalDep in attr.OptionalDependencies)
            {
                if (Enum.TryParse<ModuleType>(optionalDep, out var depModuleType))
                {
                    // Only add dependency if the target module is also discovered
                    if (discoveredModuleTypes.Contains(depModuleType))
                    {
                        dependencies.Add(new ModuleDependency
                        {
                            ModuleType = attr.ModuleType,
                            DependsOnModuleType = depModuleType,
                            IsRequired = false,
                            Description = $"Optional dependency: {attr.DisplayName} can integrate with {optionalDep}",
                            CreatedBy = currentUser,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        _logger.LogWarning("Skipping dependency from {ModuleType} to {DependencyType} - target module not discovered", 
                            attr.ModuleType, optionalDep);
                    }
                }
                else
                {
                    _logger.LogWarning("Invalid optional dependency '{Dependency}' for module {ModuleType}", 
                        optionalDep, attr.ModuleType);
                }
            }
        }

        return dependencies;
    }

    private static Assembly[] GetRelevantAssemblies()
    {
        // Get assemblies that are likely to contain modules
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        
        return loadedAssemblies
            .Where(a => 
            {
                var name = a.GetName().Name;
                return name != null && (
                    name.StartsWith("Harmoni360", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("Application", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("Features", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("Modules", StringComparison.OrdinalIgnoreCase)
                );
            })
            .ToArray();
    }
}