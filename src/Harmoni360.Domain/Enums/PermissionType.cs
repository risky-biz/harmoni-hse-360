namespace Harmoni360.Domain.Enums;

/// <summary>
/// Defines the types of permissions that can be granted within modules.
/// These permissions are applied to modules to control user actions.
/// </summary>
public enum PermissionType
{
    /// <summary>
    /// Read permission - Allows viewing/reading data within a module
    /// </summary>
    Read = 1,

    /// <summary>
    /// Create permission - Allows creating new records within a module
    /// </summary>
    Create = 2,

    /// <summary>
    /// Update permission - Allows modifying existing records within a module
    /// </summary>
    Update = 3,

    /// <summary>
    /// Delete permission - Allows removing records within a module
    /// </summary>
    Delete = 4,

    /// <summary>
    /// Export permission - Allows exporting data from a module
    /// </summary>
    Export = 5,

    /// <summary>
    /// Configure permission - Allows modifying module configuration and settings
    /// (typically restricted to SuperAdmin/Developer roles)
    /// </summary>
    Configure = 6,

    /// <summary>
    /// Approve permission - Allows approving or authorizing actions within a module
    /// (for workflow-based operations)
    /// </summary>
    Approve = 7,

    /// <summary>
    /// Assign permission - Allows assigning users or resources within a module
    /// (for user management and resource allocation)
    /// </summary>
    Assign = 8
}