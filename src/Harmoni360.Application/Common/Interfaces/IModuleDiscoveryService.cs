using Harmoni360.Application.Common.Attributes;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Common.Interfaces;

/// <summary>
/// Service for discovering modules via assembly scanning
/// </summary>
public interface IModuleDiscoveryService
{
    /// <summary>
    /// Discovers all modules in the application assemblies
    /// </summary>
    Task<IEnumerable<ModuleDiscoveryResult>> DiscoverModulesAsync();

    /// <summary>
    /// Gets module configuration from discovered modules
    /// </summary>
    Task<IEnumerable<ModuleConfiguration>> GetModuleConfigurationsAsync();

    /// <summary>
    /// Gets module dependencies from discovered modules
    /// </summary>
    Task<IEnumerable<ModuleDependency>> GetModuleDependenciesAsync();
}

/// <summary>
/// Result of module discovery
/// </summary>
public class ModuleDiscoveryResult
{
    public Type ModuleType { get; set; } = null!;
    public ModuleAttribute ModuleAttribute { get; set; } = null!;
    public string AssemblyName { get; set; } = null!;
    public string Namespace { get; set; } = null!;
}