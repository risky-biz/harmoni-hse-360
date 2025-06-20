using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities;

/// <summary>
/// Entity for managing module configurations and settings
/// </summary>
public class ModuleConfiguration : BaseEntity, IAuditableEntity
{
    /// <summary>
    /// The type of module this configuration applies to
    /// </summary>
    public ModuleType ModuleType { get; set; }
    
    /// <summary>
    /// Whether the module is enabled for users
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Display name for the module in the UI
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the module's functionality
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// CSS class for the module's icon
    /// </summary>
    public string? IconClass { get; set; }
    
    /// <summary>
    /// Display order for sorting modules
    /// </summary>
    public int DisplayOrder { get; set; }
    
    /// <summary>
    /// Parent module type for hierarchical modules
    /// </summary>
    public ModuleType? ParentModuleType { get; set; }
    
    /// <summary>
    /// JSON string containing module-specific settings
    /// </summary>
    public string? Settings { get; set; }
    
    // Navigation properties
    
    /// <summary>
    /// Parent module configuration
    /// </summary>
    public virtual ModuleConfiguration? ParentModule { get; set; }
    
    /// <summary>
    /// Collection of sub-modules
    /// </summary>
    public virtual ICollection<ModuleConfiguration> SubModules { get; set; } = new List<ModuleConfiguration>();
    
    /// <summary>
    /// Modules that this module depends on
    /// </summary>
    public virtual ICollection<ModuleDependency> Dependencies { get; set; } = new List<ModuleDependency>();
    
    /// <summary>
    /// Modules that depend on this module
    /// </summary>
    public virtual ICollection<ModuleDependency> DependentModules { get; set; } = new List<ModuleDependency>();
    
    // IAuditableEntity implementation
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    
    // Business logic methods
    
    /// <summary>
    /// Determines if this module can be disabled
    /// Critical modules cannot be disabled to maintain system functionality
    /// </summary>
    /// <returns>True if the module can be disabled</returns>
    public bool CanBeDisabled()
    {
        // Critical modules that cannot be disabled
        return ModuleType != ModuleType.Dashboard &&
               ModuleType != ModuleType.UserManagement &&
               ModuleType != ModuleType.ApplicationSettings;
    }
    
    /// <summary>
    /// Gets warnings that should be displayed when disabling this module
    /// </summary>
    /// <returns>List of warning messages</returns>
    public List<string> GetDisableWarnings()
    {
        var warnings = new List<string>();
        
        if (SubModules?.Any() == true)
        {
            var activeSubModules = SubModules.Count(sm => sm.IsEnabled);
            if (activeSubModules > 0)
            {
                warnings.Add($"This will disable {activeSubModules} active sub-modules");
            }
        }
        
        if (DependentModules?.Any() == true)
        {
            var activeDependents = DependentModules.Count(dm => dm.Module.IsEnabled && dm.IsRequired);
            if (activeDependents > 0)
            {
                warnings.Add($"This may affect {activeDependents} dependent modules");
            }
        }
        
        // Add module-specific warnings
        switch (ModuleType)
        {
            case ModuleType.Reporting:
                warnings.Add("Reports from all modules will be unavailable");
                break;
            case ModuleType.ComplianceManagement:
                warnings.Add("Compliance monitoring and audit tracking will be disabled");
                break;
            case ModuleType.SecurityIncidentManagement:
                warnings.Add("Security incident reporting and response will be unavailable");
                break;
        }
        
        return warnings;
    }
    
    /// <summary>
    /// Validates module dependencies before disabling
    /// </summary>
    /// <returns>True if the module can be safely disabled</returns>
    public bool ValidateDisable()
    {
        if (!CanBeDisabled())
        {
            return false;
        }
        
        // Check for required dependencies
        if (DependentModules?.Any(dm => dm.IsRequired && dm.Module.IsEnabled) == true)
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Enables the module and all its dependencies
    /// </summary>
    public void Enable()
    {
        IsEnabled = true;
        
        // Enable all required dependencies
        if (Dependencies?.Any() == true)
        {
            foreach (var dependency in Dependencies.Where(d => d.IsRequired))
            {
                dependency.DependsOnModule.Enable();
            }
        }
    }
    
    /// <summary>
    /// Disables the module and all its sub-modules
    /// </summary>
    public void Disable()
    {
        if (!ValidateDisable())
        {
            throw new InvalidOperationException($"Module {ModuleType} cannot be disabled due to dependencies or business rules");
        }
        
        IsEnabled = false;
        
        // Disable all sub-modules
        if (SubModules?.Any() == true)
        {
            foreach (var subModule in SubModules)
            {
                subModule.Disable();
            }
        }
    }
}