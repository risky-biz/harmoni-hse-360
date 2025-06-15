using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities;

/// <summary>
/// Junction entity representing the assignment of a module permission to a role.
/// This enables the many-to-many relationship between roles and module permissions.
/// </summary>
public class RoleModulePermission : BaseEntity
{
    /// <summary>
    /// The role that has this permission
    /// </summary>
    public int RoleId { get; private set; }

    /// <summary>
    /// The module permission being granted
    /// </summary>
    public int ModulePermissionId { get; private set; }

    /// <summary>
    /// Whether this permission assignment is currently active
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// When this permission was granted to the role
    /// </summary>
    public DateTime GrantedAt { get; private set; }

    /// <summary>
    /// Who granted this permission (user ID)
    /// </summary>
    public int? GrantedByUserId { get; private set; }

    /// <summary>
    /// Optional reason for granting this permission
    /// </summary>
    public string? GrantReason { get; private set; }

    /// <summary>
    /// Navigation property to the role
    /// </summary>
    public Role Role { get; private set; } = null!;

    /// <summary>
    /// Navigation property to the module permission
    /// </summary>
    public ModulePermission ModulePermission { get; private set; } = null!;

    /// <summary>
    /// Navigation property to the user who granted this permission
    /// </summary>
    public User? GrantedByUser { get; private set; }

    protected RoleModulePermission() { } // For EF Core

    /// <summary>
    /// Creates a new role-module-permission assignment
    /// </summary>
    /// <param name="roleId">The role ID</param>
    /// <param name="modulePermissionId">The module permission ID</param>
    /// <param name="grantedByUserId">Who is granting this permission</param>
    /// <param name="grantReason">Optional reason for the grant</param>
    /// <returns>New role-module-permission instance</returns>
    public static RoleModulePermission Create(int roleId, int modulePermissionId, int? grantedByUserId = null, string? grantReason = null)
    {
        return new RoleModulePermission
        {
            RoleId = roleId,
            ModulePermissionId = modulePermissionId,
            IsActive = true,
            GrantedAt = DateTime.UtcNow,
            GrantedByUserId = grantedByUserId,
            GrantReason = grantReason
        };
    }

    /// <summary>
    /// Activates this permission assignment
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates this permission assignment
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Updates the grant reason
    /// </summary>
    /// <param name="reason">New grant reason</param>
    public void UpdateGrantReason(string? reason)
    {
        GrantReason = reason;
    }
}