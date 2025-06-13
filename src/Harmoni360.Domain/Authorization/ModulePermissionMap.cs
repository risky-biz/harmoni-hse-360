using Harmoni360.Domain.Constants;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Authorization;

/// <summary>
/// Defines the mapping between roles and their permitted modules with specific permissions.
/// This class centralizes the authorization rules for the Harmoni360 system.
/// </summary>
public static class ModulePermissionMap
{
    /// <summary>
    /// Gets the complete role-to-module permission mapping for the system
    /// </summary>
    public static Dictionary<RoleType, Dictionary<ModuleType, List<PermissionType>>> GetRoleModulePermissions()
    {
        return new Dictionary<RoleType, Dictionary<ModuleType, List<PermissionType>>>
        {
            // SuperAdmin - Complete system access including ALL modules AND application settings/configuration AND user management
            [RoleType.SuperAdmin] = new Dictionary<ModuleType, List<PermissionType>>
            {
                [ModuleType.Dashboard] = AllPermissions(),
                [ModuleType.WorkPermitManagement] = AllPermissions(),
                [ModuleType.InspectionManagement] = AllPermissions(),
                [ModuleType.IncidentManagement] = AllPermissions(),
                [ModuleType.RiskManagement] = AllPermissions(),
                [ModuleType.PPEManagement] = AllPermissions(),
                [ModuleType.HealthMonitoring] = AllPermissions(),
                [ModuleType.PhysicalSecurity] = AllPermissions(),
                [ModuleType.InformationSecurity] = AllPermissions(),
                [ModuleType.PersonnelSecurity] = AllPermissions(),
                [ModuleType.SecurityIncidentManagement] = AllPermissions(),
                [ModuleType.ComplianceManagement] = AllPermissions(),
                [ModuleType.Reporting] = AllPermissions(),
                [ModuleType.UserManagement] = AllPermissions(),
                [ModuleType.ApplicationSettings] = AllPermissions()
            },

            // Developer - Complete system access including ALL modules AND application settings/configuration AND user management
            [RoleType.Developer] = new Dictionary<ModuleType, List<PermissionType>>
            {
                [ModuleType.Dashboard] = AllPermissions(),
                [ModuleType.WorkPermitManagement] = AllPermissions(),
                [ModuleType.InspectionManagement] = AllPermissions(),
                [ModuleType.IncidentManagement] = AllPermissions(),
                [ModuleType.RiskManagement] = AllPermissions(),
                [ModuleType.PPEManagement] = AllPermissions(),
                [ModuleType.HealthMonitoring] = AllPermissions(),
                [ModuleType.PhysicalSecurity] = AllPermissions(),
                [ModuleType.InformationSecurity] = AllPermissions(),
                [ModuleType.PersonnelSecurity] = AllPermissions(),
                [ModuleType.SecurityIncidentManagement] = AllPermissions(),
                [ModuleType.ComplianceManagement] = AllPermissions(),
                [ModuleType.Reporting] = AllPermissions(),
                [ModuleType.UserManagement] = AllPermissions(),
                [ModuleType.ApplicationSettings] = AllPermissions()
            },

            // Admin - Access to ALL functional modules BUT EXCLUDED from application settings/configuration
            [RoleType.Admin] = new Dictionary<ModuleType, List<PermissionType>>
            {
                [ModuleType.Dashboard] = AllPermissions(),
                [ModuleType.WorkPermitManagement] = AllPermissions(),
                [ModuleType.InspectionManagement] = AllPermissions(),
                [ModuleType.IncidentManagement] = AllPermissions(),
                [ModuleType.RiskManagement] = AllPermissions(),
                [ModuleType.PPEManagement] = AllPermissions(),
                [ModuleType.HealthMonitoring] = AllPermissions(),
                [ModuleType.PhysicalSecurity] = AllPermissions(),
                [ModuleType.InformationSecurity] = AllPermissions(),
                [ModuleType.PersonnelSecurity] = AllPermissions(),
                [ModuleType.SecurityIncidentManagement] = AllPermissions(),
                [ModuleType.ComplianceManagement] = AllPermissions(),
                [ModuleType.Reporting] = AllPermissions(),
                [ModuleType.UserManagement] = CrudPermissions() // Can manage users but not configure system
                // ApplicationSettings - NO ACCESS
            },

            // IncidentManager - RESTRICTED access ONLY to Incident Management module
            [RoleType.IncidentManager] = new Dictionary<ModuleType, List<PermissionType>>
            {
                [ModuleType.Dashboard] = ReadOnlyPermissions(),
                [ModuleType.IncidentManagement] = AllPermissions(),
                [ModuleType.Reporting] = ReadOnlyPermissions() // Can view reports for incidents
                // All other modules - NO ACCESS
            },

            // RiskManager - RESTRICTED access ONLY to Risk Management module
            [RoleType.RiskManager] = new Dictionary<ModuleType, List<PermissionType>>
            {
                [ModuleType.Dashboard] = ReadOnlyPermissions(),
                [ModuleType.RiskManagement] = AllPermissions(),
                [ModuleType.Reporting] = ReadOnlyPermissions() // Can view reports for risks
                // All other modules - NO ACCESS
            },

            // PPEManager - RESTRICTED access ONLY to PPE Management module
            [RoleType.PPEManager] = new Dictionary<ModuleType, List<PermissionType>>
            {
                [ModuleType.Dashboard] = ReadOnlyPermissions(),
                [ModuleType.PPEManagement] = AllPermissions(),
                [ModuleType.Reporting] = ReadOnlyPermissions() // Can view reports for PPE
                // All other modules - NO ACCESS
            },

            // HealthMonitor - RESTRICTED access ONLY to Health Monitoring module
            [RoleType.HealthMonitor] = new Dictionary<ModuleType, List<PermissionType>>
            {
                [ModuleType.Dashboard] = ReadOnlyPermissions(),
                [ModuleType.HealthMonitoring] = AllPermissions(),
                [ModuleType.Reporting] = ReadOnlyPermissions() // Can view reports for health
                // All other modules - NO ACCESS
            },

            // InspectionManager - RESTRICTED access ONLY to Inspection Management module
            [RoleType.InspectionManager] = new Dictionary<ModuleType, List<PermissionType>>
            {
                [ModuleType.Dashboard] = ReadOnlyPermissions(),
                [ModuleType.InspectionManagement] = AllPermissions(),
                [ModuleType.Reporting] = ReadOnlyPermissions() // Can view reports for inspections
                // All other modules - NO ACCESS
            },

            // SecurityManager - COMPREHENSIVE access to ALL Security modules
            [RoleType.SecurityManager] = new Dictionary<ModuleType, List<PermissionType>>
            {
                [ModuleType.Dashboard] = AllPermissions(),
                [ModuleType.PhysicalSecurity] = AllPermissions(),
                [ModuleType.InformationSecurity] = AllPermissions(),
                [ModuleType.PersonnelSecurity] = AllPermissions(),
                [ModuleType.SecurityIncidentManagement] = AllPermissions(),
                [ModuleType.ComplianceManagement] = AllPermissions(), // Security compliance
                [ModuleType.Reporting] = AllPermissions() // Can view and generate security reports
                // HSE modules and Application Settings - NO ACCESS (Security domain focus)
            },

            // SecurityOfficer - OPERATIONAL access to day-to-day Security operations
            [RoleType.SecurityOfficer] = new Dictionary<ModuleType, List<PermissionType>>
            {
                [ModuleType.Dashboard] = ReadOnlyPermissions(),
                [ModuleType.PhysicalSecurity] = CrudPermissions(), // Can manage daily operations
                [ModuleType.InformationSecurity] = CrudPermissions(), // Can handle security policies and incidents
                [ModuleType.PersonnelSecurity] = CrudPermissions(), // Can manage personnel security
                [ModuleType.SecurityIncidentManagement] = AllPermissions(), // Full access to security incidents
                [ModuleType.ComplianceManagement] = ReadOnlyPermissions(), // Can view compliance status
                [ModuleType.Reporting] = ReadOnlyPermissions() // Can view security reports
                // HSE modules and strategic configuration - NO ACCESS
            },

            // ComplianceOfficer - ENHANCED access to HSSE compliance management across ALL domains
            [RoleType.ComplianceOfficer] = new Dictionary<ModuleType, List<PermissionType>>
            {
                [ModuleType.Dashboard] = AllPermissions(),
                [ModuleType.InspectionManagement] = ReadOnlyPermissions(), // Can view inspections for compliance
                [ModuleType.IncidentManagement] = ReadOnlyPermissions(), // Can view HSE incidents for compliance
                [ModuleType.RiskManagement] = ReadOnlyPermissions(), // Can view risk assessments for compliance
                [ModuleType.PPEManagement] = ReadOnlyPermissions(), // Can view PPE compliance
                [ModuleType.HealthMonitoring] = ReadOnlyPermissions(), // Can view health compliance
                [ModuleType.PhysicalSecurity] = ReadOnlyPermissions(), // Can view security compliance
                [ModuleType.InformationSecurity] = ReadOnlyPermissions(), // Can view information security compliance
                [ModuleType.PersonnelSecurity] = ReadOnlyPermissions(), // Can view personnel security compliance
                [ModuleType.SecurityIncidentManagement] = ReadOnlyPermissions(), // Can view security incidents for compliance
                [ModuleType.ComplianceManagement] = AllPermissions(), // Full access to compliance management
                [ModuleType.Reporting] = AllPermissions() // Can generate comprehensive HSSE compliance reports
                // User Management and Application Settings - NO ACCESS
            },

            // Reporter - READ-ONLY access to reporting functionality across modules they have permission for
            [RoleType.Reporter] = new Dictionary<ModuleType, List<PermissionType>>
            {
                [ModuleType.Dashboard] = ReadOnlyPermissions(),
                [ModuleType.InspectionManagement] = ReadOnlyPermissions(),
                [ModuleType.IncidentManagement] = ReadOnlyPermissions(),
                [ModuleType.RiskManagement] = ReadOnlyPermissions(),
                [ModuleType.PPEManagement] = ReadOnlyPermissions(),
                [ModuleType.HealthMonitoring] = ReadOnlyPermissions(),
                [ModuleType.PhysicalSecurity] = ReadOnlyPermissions(),
                [ModuleType.InformationSecurity] = ReadOnlyPermissions(),
                [ModuleType.PersonnelSecurity] = ReadOnlyPermissions(),
                [ModuleType.SecurityIncidentManagement] = ReadOnlyPermissions(),
                [ModuleType.ComplianceManagement] = ReadOnlyPermissions(),
                [ModuleType.Reporting] = ReadOnlyPermissions()
                // User Management and Application Settings - NO ACCESS
            },

            // Viewer - READ-ONLY access to basic dashboard and summary information
            [RoleType.Viewer] = new Dictionary<ModuleType, List<PermissionType>>
            {
                [ModuleType.Dashboard] = new List<PermissionType> { PermissionType.Read }
                // All other modules - NO ACCESS
            }
        };
    }

    /// <summary>
    /// Gets all available permissions for a module
    /// </summary>
    private static List<PermissionType> AllPermissions()
    {
        return new List<PermissionType>
        {
            PermissionType.Read,
            PermissionType.Create,
            PermissionType.Update,
            PermissionType.Delete,
            PermissionType.Export,
            PermissionType.Configure,
            PermissionType.Approve,
            PermissionType.Assign
        };
    }

    /// <summary>
    /// Gets CRUD permissions (Create, Read, Update, Delete, Export)
    /// </summary>
    private static List<PermissionType> CrudPermissions()
    {
        return new List<PermissionType>
        {
            PermissionType.Read,
            PermissionType.Create,
            PermissionType.Update,
            PermissionType.Delete,
            PermissionType.Export
        };
    }

    /// <summary>
    /// Gets read-only permissions (Read, Export)
    /// </summary>
    private static List<PermissionType> ReadOnlyPermissions()
    {
        return new List<PermissionType>
        {
            PermissionType.Read,
            PermissionType.Export
        };
    }

    /// <summary>
    /// Checks if a role has a specific permission for a module
    /// </summary>
    /// <param name="role">The role to check</param>
    /// <param name="module">The module to check</param>
    /// <param name="permission">The permission to check</param>
    /// <returns>True if the role has the permission for the module</returns>
    public static bool HasPermission(RoleType role, ModuleType module, PermissionType permission)
    {
        var rolePermissions = GetRoleModulePermissions();
        
        if (!rolePermissions.ContainsKey(role))
            return false;
            
        if (!rolePermissions[role].ContainsKey(module))
            return false;
            
        return rolePermissions[role][module].Contains(permission);
    }

    /// <summary>
    /// Gets all modules accessible by a role
    /// </summary>
    /// <param name="role">The role to check</param>
    /// <returns>List of accessible modules</returns>
    public static List<ModuleType> GetAccessibleModules(RoleType role)
    {
        var rolePermissions = GetRoleModulePermissions();
        
        if (!rolePermissions.ContainsKey(role))
            return new List<ModuleType>();
            
        return rolePermissions[role].Keys.ToList();
    }

    /// <summary>
    /// Gets all permissions for a role within a specific module
    /// </summary>
    /// <param name="role">The role to check</param>
    /// <param name="module">The module to check</param>
    /// <returns>List of permissions for the role in the module</returns>
    public static List<PermissionType> GetModulePermissions(RoleType role, ModuleType module)
    {
        var rolePermissions = GetRoleModulePermissions();
        
        if (!rolePermissions.ContainsKey(role))
            return new List<PermissionType>();
            
        if (!rolePermissions[role].ContainsKey(module))
            return new List<PermissionType>();
            
        return rolePermissions[role][module];
    }

    /// <summary>
    /// Gets roles that have access to a specific module
    /// </summary>
    /// <param name="module">The module to check</param>
    /// <returns>List of roles with access to the module</returns>
    public static List<RoleType> GetRolesWithModuleAccess(ModuleType module)
    {
        var rolePermissions = GetRoleModulePermissions();
        var rolesWithAccess = new List<RoleType>();

        foreach (var roleMapping in rolePermissions)
        {
            if (roleMapping.Value.ContainsKey(module))
            {
                rolesWithAccess.Add(roleMapping.Key);
            }
        }

        return rolesWithAccess;
    }

    /// <summary>
    /// Validates if a role assignment is valid according to system rules
    /// </summary>
    /// <param name="assignerRole">The role of the user making the assignment</param>
    /// <param name="targetRole">The role being assigned</param>
    /// <returns>True if the assignment is valid</returns>
    public static bool CanAssignRole(RoleType assignerRole, RoleType targetRole)
    {
        // SuperAdmin and Developer can assign any role
        if (assignerRole == RoleType.SuperAdmin || assignerRole == RoleType.Developer)
            return true;

        // Admin can assign manager roles and below, but not other admin roles
        if (assignerRole == RoleType.Admin)
        {
            return targetRole != RoleType.SuperAdmin && 
                   targetRole != RoleType.Developer && 
                   targetRole != RoleType.Admin;
        }

        // SecurityManager can assign SecurityOfficer and ComplianceOfficer (within their domain)
        if (assignerRole == RoleType.SecurityManager)
        {
            return targetRole == RoleType.SecurityOfficer || 
                   targetRole == RoleType.ComplianceOfficer;
        }

        // Other roles cannot assign roles
        return false;
    }
}