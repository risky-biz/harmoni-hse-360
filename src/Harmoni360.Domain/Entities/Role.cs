using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    
    /// <summary>
    /// The role type enum for programmatic access
    /// </summary>
    public RoleType RoleType { get; private set; }
    
    /// <summary>
    /// Whether this role is currently active
    /// </summary>
    public bool IsActive { get; private set; } = true;
    
    /// <summary>
    /// Display order for UI purposes
    /// </summary>
    public int DisplayOrder { get; private set; }

    // Legacy permissions (will be phased out in favor of module permissions)
    private readonly List<Permission> _permissions = new();
    public IReadOnlyCollection<Permission> Permissions => _permissions.AsReadOnly();

    // Module-based permissions
    private readonly List<RoleModulePermission> _roleModulePermissions = new();
    public IReadOnlyCollection<RoleModulePermission> RoleModulePermissions => _roleModulePermissions.AsReadOnly();

    // User assignments
    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    protected Role() { } // For EF Core

    /// <summary>
    /// Creates a new role with the specified type
    /// </summary>
    /// <param name="roleType">The role type enum</param>
    /// <param name="name">The role name</param>
    /// <param name="description">The role description</param>
    /// <param name="displayOrder">Display order for UI</param>
    /// <returns>New role instance</returns>
    public static Role Create(RoleType roleType, string name, string description, int displayOrder = 0)
    {
        return new Role
        {
            RoleType = roleType,
            Name = name,
            Description = description,
            IsActive = true,
            DisplayOrder = displayOrder
        };
    }

    /// <summary>
    /// Creates a role using string name (for backward compatibility)
    /// </summary>
    /// <param name="name">The role name</param>
    /// <param name="description">The role description</param>
    /// <returns>New role instance</returns>
    public static Role Create(string name, string description)
    {
        // Try to parse the name to a RoleType enum
        if (Enum.TryParse<RoleType>(name, out var roleType))
        {
            return Create(roleType, name, description);
        }
        
        // Fallback to Viewer role if name doesn't match any enum
        return Create(RoleType.Viewer, name, description);
    }

    /// <summary>
    /// Updates the role details
    /// </summary>
    /// <param name="name">New name</param>
    /// <param name="description">New description</param>
    /// <param name="displayOrder">New display order</param>
    public void UpdateDetails(string name, string description, int displayOrder = 0)
    {
        Name = name;
        Description = description;
        DisplayOrder = displayOrder;
    }

    /// <summary>
    /// Activates this role
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates this role
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    // Legacy permission methods (maintained for backward compatibility)
    public void AddPermission(Permission permission)
    {
        if (!_permissions.Contains(permission))
            _permissions.Add(permission);
    }

    public void RemovePermission(Permission permission)
    {
        _permissions.Remove(permission);
    }

    /// <summary>
    /// Checks if this role has the specified module permission
    /// </summary>
    /// <param name="moduleType">The module to check</param>
    /// <param name="permissionType">The permission type to check</param>
    /// <returns>True if the role has the permission</returns>
    public bool HasModulePermission(ModuleType moduleType, PermissionType permissionType)
    {
        return _roleModulePermissions.Any(rmp => 
            rmp.IsActive && 
            rmp.ModulePermission.IsActive &&
            rmp.ModulePermission.Module == moduleType && 
            rmp.ModulePermission.Permission == permissionType);
    }

    /// <summary>
    /// Gets all active module permissions for this role
    /// </summary>
    /// <returns>Collection of active module permissions</returns>
    public IEnumerable<ModulePermission> GetActiveModulePermissions()
    {
        return _roleModulePermissions
            .Where(rmp => rmp.IsActive && rmp.ModulePermission.IsActive)
            .Select(rmp => rmp.ModulePermission);
    }

    /// <summary>
    /// Gets all modules this role has access to
    /// </summary>
    /// <returns>Collection of accessible modules</returns>
    public IEnumerable<ModuleType> GetAccessibleModules()
    {
        return _roleModulePermissions
            .Where(rmp => rmp.IsActive && rmp.ModulePermission.IsActive)
            .Select(rmp => rmp.ModulePermission.Module)
            .Distinct();
    }
}

public class UserRole : BaseEntity, IAuditableEntity
{
    public int UserId { get; private set; }
    public int RoleId { get; private set; }
    public Role Role { get; private set; } = null!;

    // Audit properties
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    protected UserRole() { } // For EF Core

    public UserRole(int userId, int roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }
}