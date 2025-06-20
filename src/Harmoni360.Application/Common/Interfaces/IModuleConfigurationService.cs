using Harmoni360.Application.Features.ModuleConfiguration.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Common.Interfaces;

public interface IModuleConfigurationService
{
    // Module configuration management
    Task<ModuleConfigurationDto?> GetModuleConfigurationAsync(ModuleType moduleType);
    Task<IEnumerable<ModuleConfigurationDto>> GetAllModuleConfigurationsAsync();
    Task<IEnumerable<ModuleConfigurationDto>> GetEnabledModuleConfigurationsAsync();
    Task<ModuleConfigurationDashboardDto> GetModuleConfigurationDashboardAsync();
    
    // Module state management
    Task<bool> EnableModuleAsync(ModuleType moduleType);
    Task<bool> DisableModuleAsync(ModuleType moduleType);
    Task<bool> CanModuleBeDisabledAsync(ModuleType moduleType);
    Task<IEnumerable<string>> GetModuleDisableWarningsAsync(ModuleType moduleType);
    
    // Module dependencies
    Task<IEnumerable<ModuleDependencyDto>> GetModuleDependenciesAsync(ModuleType moduleType);
    Task<IEnumerable<ModuleDependencyDto>> GetDependentModulesAsync(ModuleType moduleType);
    Task<bool> ValidateModuleDependenciesAsync(ModuleType moduleType);
    
    // Module settings
    Task<bool> UpdateModuleSettingsAsync(ModuleType moduleType, string? settings);
    Task<string?> GetModuleSettingsAsync(ModuleType moduleType);
    
    // Audit logging
    Task LogModuleStateChangeAsync(ModuleType moduleType, bool oldState, bool newState, string? context = null);
    Task LogModuleSettingsChangeAsync(ModuleType moduleType, string? oldSettings, string? newSettings, string? context = null);
    Task<IEnumerable<ModuleConfigurationAuditLogDto>> GetModuleAuditTrailAsync(ModuleType moduleType);
    Task<IEnumerable<ModuleConfigurationAuditLogDto>> GetRecentModuleActivityAsync(int count = 10);
    
    // Module hierarchy
    Task<IEnumerable<ModuleConfigurationDto>> GetModuleHierarchyAsync();
    Task<IEnumerable<ModuleConfigurationDto>> GetChildModulesAsync(ModuleType parentModuleType);
}