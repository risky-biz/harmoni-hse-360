using Microsoft.AspNetCore.Authorization;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Web.Authorization;

/// <summary>
/// Authorization requirement for module-based permissions.
/// Defines what module and permission is required for access.
/// </summary>
public class ModulePermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// The module that the user must have access to
    /// </summary>
    public ModuleType Module { get; }

    /// <summary>
    /// The specific permission required within the module
    /// </summary>
    public PermissionType Permission { get; }

    /// <summary>
    /// Optional description for debugging and logging
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Initializes a new module permission requirement
    /// </summary>
    /// <param name="module">The required module</param>
    /// <param name="permission">The required permission</param>
    /// <param name="description">Optional description</param>
    public ModulePermissionRequirement(ModuleType module, PermissionType permission, string? description = null)
    {
        Module = module;
        Permission = permission;
        Description = description ?? $"Requires {permission} permission in {module} module";
    }

    /// <summary>
    /// Returns a string representation of this requirement
    /// </summary>
    public override string ToString()
    {
        return $"ModulePermission: {Module}.{Permission}";
    }
}