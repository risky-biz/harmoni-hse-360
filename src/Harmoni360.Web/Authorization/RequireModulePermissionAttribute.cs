using Microsoft.AspNetCore.Authorization;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Web.Authorization;

/// <summary>
/// Authorization attribute for requiring specific module permissions.
/// This attribute can be applied to controllers or actions to enforce module-based access control.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireModulePermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// The module that access is required for
    /// </summary>
    public ModuleType Module { get; }

    /// <summary>
    /// The specific permission required within the module
    /// </summary>
    public PermissionType Permission { get; }

    /// <summary>
    /// Initializes a new module permission requirement attribute
    /// </summary>
    /// <param name="module">The module that access is required for</param>
    /// <param name="permission">The specific permission required</param>
    public RequireModulePermissionAttribute(ModuleType module, PermissionType permission)
    {
        Module = module;
        Permission = permission;
        Policy = $"ModulePermission.{module}.{permission}";
    }

    /// <summary>
    /// Gets a description of this permission requirement
    /// </summary>
    public string GetDescription()
    {
        return $"Requires {Permission} permission in {Module} module";
    }
}

/// <summary>
/// Authorization attribute for requiring access to a specific module (any permission level).
/// Useful for module-level access checks where specific permission doesn't matter.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireModuleAccessAttribute : AuthorizeAttribute
{
    /// <summary>
    /// The module that access is required for
    /// </summary>
    public ModuleType Module { get; }

    /// <summary>
    /// Initializes a new module access requirement attribute
    /// </summary>
    /// <param name="module">The module that access is required for</param>
    public RequireModuleAccessAttribute(ModuleType module)
    {
        Module = module;
        Policy = $"ModuleAccess.{module}";
    }

    /// <summary>
    /// Gets a description of this access requirement
    /// </summary>
    public string GetDescription()
    {
        return $"Requires access to {Module} module";
    }
}

/// <summary>
/// Authorization attribute for requiring specific roles.
/// Enhances the standard Authorize attribute with better logging and role management.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireRolesAttribute : AuthorizeAttribute
{
    /// <summary>
    /// The roles that are allowed access
    /// </summary>
    public RoleType[] AllowedRoles { get; }

    /// <summary>
    /// Initializes a new role requirement attribute
    /// </summary>
    /// <param name="roles">The roles that are allowed access</param>
    public RequireRolesAttribute(params RoleType[] roles)
    {
        AllowedRoles = roles;
        var roleNames = roles.Select(r => r.ToString()).ToArray();
        Roles = string.Join(",", roleNames);
        Policy = $"RequireRoles.{string.Join(".", roleNames)}";
    }

    /// <summary>
    /// Gets a description of this role requirement
    /// </summary>
    public string GetDescription()
    {
        var roleNames = AllowedRoles.Select(r => r.ToString());
        return $"Requires one of the following roles: {string.Join(", ", roleNames)}";
    }
}

/// <summary>
/// Authorization attribute for system administrative functions.
/// Restricts access to SuperAdmin and Developer roles only.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireSystemAdminAttribute : AuthorizeAttribute
{
    public RequireSystemAdminAttribute()
    {
        Policy = "RequireSystemAdmin";
        Roles = "SuperAdmin,Developer";
    }

    public string GetDescription()
    {
        return "Requires SuperAdmin or Developer role for system administration";
    }
}

/// <summary>
/// Authorization attribute for functional administrative functions.
/// Allows SuperAdmin, Developer, and Admin roles.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireFunctionalAdminAttribute : AuthorizeAttribute
{
    public RequireFunctionalAdminAttribute()
    {
        Policy = "RequireFunctionalAdmin";
        Roles = "SuperAdmin,Developer,Admin";
    }

    public string GetDescription()
    {
        return "Requires SuperAdmin, Developer, or Admin role for functional administration";
    }
}