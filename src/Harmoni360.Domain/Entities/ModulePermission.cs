using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities;

/// <summary>
/// Represents a permission within a specific module of the system.
/// Module permissions are the building blocks of the authorization system.
/// </summary>
public class ModulePermission : BaseEntity
{
    /// <summary>
    /// The module this permission applies to
    /// </summary>
    public ModuleType Module { get; private set; }

    /// <summary>
    /// The type of permission (Read, Create, Update, Delete, etc.)
    /// </summary>
    public PermissionType Permission { get; private set; }

    /// <summary>
    /// Human-readable name for this permission
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Detailed description of what this permission allows
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Whether this permission is currently active/enabled
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Navigation property for role-module-permission assignments
    /// </summary>
    private readonly List<RoleModulePermission> _roleModulePermissions = new();
    public IReadOnlyCollection<RoleModulePermission> RoleModulePermissions => _roleModulePermissions.AsReadOnly();

    protected ModulePermission() { } // For EF Core

    /// <summary>
    /// Creates a new module permission
    /// </summary>
    /// <param name="module">The module this permission applies to</param>
    /// <param name="permission">The type of permission</param>
    /// <param name="name">Human-readable name</param>
    /// <param name="description">Detailed description</param>
    /// <returns>New module permission instance</returns>
    public static ModulePermission Create(ModuleType module, PermissionType permission, string name, string description)
    {
        return new ModulePermission
        {
            Module = module,
            Permission = permission,
            Name = name,
            Description = description,
            IsActive = true
        };
    }

    /// <summary>
    /// Updates the permission details
    /// </summary>
    /// <param name="name">New name</param>
    /// <param name="description">New description</param>
    public void UpdateDetails(string name, string description)
    {
        Name = name;
        Description = description;
    }

    /// <summary>
    /// Activates this permission
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates this permission
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Gets the full permission identifier as "Module.Permission"
    /// </summary>
    /// <returns>Permission identifier string</returns>
    public string GetPermissionIdentifier()
    {
        return $"{Module}.{Permission}";
    }

    /// <summary>
    /// Checks if this permission matches the specified module and permission type
    /// </summary>
    /// <param name="module">Module to check</param>
    /// <param name="permission">Permission type to check</param>
    /// <returns>True if matches</returns>
    public bool Matches(ModuleType module, PermissionType permission)
    {
        return Module == module && Permission == permission && IsActive;
    }
}