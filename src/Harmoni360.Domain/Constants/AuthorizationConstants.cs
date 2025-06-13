namespace Harmoni360.Domain.Constants;

/// <summary>
/// Centralized constants for authorization throughout the Harmoni360 system.
/// Provides consistent string values for roles, modules, and permissions.
/// </summary>
public static class AuthorizationConstants
{
    /// <summary>
    /// Role name constants for consistent reference throughout the application
    /// </summary>
    public static class Roles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Developer = "Developer";
        public const string Admin = "Admin";
        public const string IncidentManager = "IncidentManager";
        public const string RiskManager = "RiskManager";
        public const string PPEManager = "PPEManager";
        public const string HealthMonitor = "HealthMonitor";
        public const string Reporter = "Reporter";
        public const string Viewer = "Viewer";

        /// <summary>
        /// All available roles in the system
        /// </summary>
        public static readonly string[] AllRoles = 
        {
            SuperAdmin, Developer, Admin, IncidentManager, 
            RiskManager, PPEManager, HealthMonitor, Reporter, Viewer
        };

        /// <summary>
        /// Administrative roles with elevated privileges
        /// </summary>
        public static readonly string[] AdminRoles = 
        {
            SuperAdmin, Developer, Admin
        };

        /// <summary>
        /// System configuration roles (can modify application settings)
        /// </summary>
        public static readonly string[] SystemConfigRoles = 
        {
            SuperAdmin, Developer
        };

        /// <summary>
        /// Functional module manager roles
        /// </summary>
        public static readonly string[] ManagerRoles = 
        {
            IncidentManager, RiskManager, PPEManager, HealthMonitor
        };

        /// <summary>
        /// Read-only roles
        /// </summary>
        public static readonly string[] ReadOnlyRoles = 
        {
            Reporter, Viewer
        };
    }

    /// <summary>
    /// Module name constants for consistent reference throughout the application
    /// </summary>
    public static class Modules
    {
        public const string Dashboard = "Dashboard";
        public const string IncidentManagement = "IncidentManagement";
        public const string RiskManagement = "RiskManagement";
        public const string PPEManagement = "PPEManagement";
        public const string HealthMonitoring = "HealthMonitoring";
        public const string Reporting = "Reporting";
        public const string UserManagement = "UserManagement";
        public const string ApplicationSettings = "ApplicationSettings";

        /// <summary>
        /// All available modules in the system
        /// </summary>
        public static readonly string[] AllModules = 
        {
            Dashboard, IncidentManagement, RiskManagement, PPEManagement,
            HealthMonitoring, Reporting, UserManagement, ApplicationSettings
        };

        /// <summary>
        /// Core functional modules (excludes administrative modules)
        /// </summary>
        public static readonly string[] FunctionalModules = 
        {
            Dashboard, IncidentManagement, RiskManagement, PPEManagement,
            HealthMonitoring, Reporting
        };

        /// <summary>
        /// Administrative modules (restricted access)
        /// </summary>
        public static readonly string[] AdminModules = 
        {
            UserManagement, ApplicationSettings
        };
    }

    /// <summary>
    /// Permission action constants for consistent reference throughout the application
    /// </summary>
    public static class Permissions
    {
        public const string Read = "Read";
        public const string Create = "Create";
        public const string Update = "Update";
        public const string Delete = "Delete";
        public const string Export = "Export";
        public const string Configure = "Configure";
        public const string Approve = "Approve";
        public const string Assign = "Assign";

        /// <summary>
        /// All available permissions in the system
        /// </summary>
        public static readonly string[] AllPermissions = 
        {
            Read, Create, Update, Delete, Export, Configure, Approve, Assign
        };

        /// <summary>
        /// Basic CRUD permissions
        /// </summary>
        public static readonly string[] CrudPermissions = 
        {
            Create, Read, Update, Delete
        };

        /// <summary>
        /// Read-only permissions
        /// </summary>
        public static readonly string[] ReadOnlyPermissions = 
        {
            Read, Export
        };

        /// <summary>
        /// Administrative permissions
        /// </summary>
        public static readonly string[] AdminPermissions = 
        {
            Configure, Approve, Assign
        };
    }

    /// <summary>
    /// Authorization policy names used throughout the application
    /// </summary>
    public static class Policies
    {
        // Module-based policies
        public const string IncidentManagementAccess = "IncidentManagementAccess";
        public const string RiskManagementAccess = "RiskManagementAccess";
        public const string PPEManagementAccess = "PPEManagementAccess";
        public const string HealthMonitoringAccess = "HealthMonitoringAccess";
        public const string ReportingAccess = "ReportingAccess";
        public const string UserManagementAccess = "UserManagementAccess";
        public const string ApplicationSettingsAccess = "ApplicationSettingsAccess";

        // Permission-based policies
        public const string CanCreateRecords = "CanCreateRecords";
        public const string CanUpdateRecords = "CanUpdateRecords";
        public const string CanDeleteRecords = "CanDeleteRecords";
        public const string CanExportData = "CanExportData";
        public const string CanConfigureSystem = "CanConfigureSystem";
        public const string CanApproveActions = "CanApproveActions";
        public const string CanAssignUsers = "CanAssignUsers";

        // Role-based policies
        public const string RequireAdminRole = "RequireAdminRole";
        public const string RequireManagerRole = "RequireManagerRole";
        public const string RequireSystemConfigRole = "RequireSystemConfigRole";
    }

    /// <summary>
    /// Permission format strings for building specific permissions
    /// </summary>
    public static class PermissionFormats
    {
        /// <summary>
        /// Format: Module.Permission (e.g., "IncidentManagement.Create")
        /// </summary>
        public const string ModulePermission = "{0}.{1}";

        /// <summary>
        /// Format: Module.Resource.Permission (e.g., "IncidentManagement.Incident.Create")
        /// </summary>
        public const string ModuleResourcePermission = "{0}.{1}.{2}";
    }

    /// <summary>
    /// Helper methods for building permission strings
    /// </summary>
    public static class PermissionBuilder
    {
        /// <summary>
        /// Builds a module-level permission string
        /// </summary>
        /// <param name="module">The module name</param>
        /// <param name="permission">The permission action</param>
        /// <returns>Formatted permission string</returns>
        public static string BuildModulePermission(string module, string permission)
        {
            return string.Format(PermissionFormats.ModulePermission, module, permission);
        }

        /// <summary>
        /// Builds a resource-level permission string
        /// </summary>
        /// <param name="module">The module name</param>
        /// <param name="resource">The resource name</param>
        /// <param name="permission">The permission action</param>
        /// <returns>Formatted permission string</returns>
        public static string BuildResourcePermission(string module, string resource, string permission)
        {
            return string.Format(PermissionFormats.ModuleResourcePermission, module, resource, permission);
        }
    }
}