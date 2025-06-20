using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities;

/// <summary>
/// Entity representing dependencies between modules
/// </summary>
public class ModuleDependency : BaseEntity, IAuditableEntity
{
    /// <summary>
    /// The module that has a dependency
    /// </summary>
    public ModuleType ModuleType { get; set; }
    
    /// <summary>
    /// The module that this module depends on
    /// </summary>
    public ModuleType DependsOnModuleType { get; set; }
    
    /// <summary>
    /// Whether this dependency is required (hard dependency) or optional (soft dependency)
    /// </summary>
    public bool IsRequired { get; set; } = true;
    
    /// <summary>
    /// Description of the dependency relationship
    /// </summary>
    public string? Description { get; set; }
    
    // Navigation properties
    
    /// <summary>
    /// The module that has the dependency
    /// </summary>
    public virtual ModuleConfiguration Module { get; set; } = null!;
    
    /// <summary>
    /// The module that this module depends on
    /// </summary>
    public virtual ModuleConfiguration DependsOnModule { get; set; } = null!;
    
    // IAuditableEntity implementation
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    
    /// <summary>
    /// Validates that the dependency relationship is valid
    /// </summary>
    /// <returns>True if the dependency is valid</returns>
    public bool IsValidDependency()
    {
        // Cannot depend on itself
        if (ModuleType == DependsOnModuleType)
        {
            return false;
        }
        
        // Check for circular dependencies would require more complex logic
        // For now, we'll implement basic validation
        return true;
    }
    
    /// <summary>
    /// Gets a human-readable description of the dependency
    /// </summary>
    /// <returns>Dependency description</returns>
    public string GetDependencyDescription()
    {
        if (!string.IsNullOrEmpty(Description))
        {
            return Description;
        }
        
        var dependencyType = IsRequired ? "requires" : "optionally uses";
        return $"{ModuleType} {dependencyType} {DependsOnModuleType}";
    }
}