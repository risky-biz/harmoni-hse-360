namespace Harmoni360.Domain.Enums;

/// <summary>
/// Defines the hierarchical role types in the Harmoni360 system.
/// Roles are ordered by access level from highest (SuperAdmin) to lowest (Viewer).
/// </summary>
public enum RoleType
{
    /// <summary>
    /// Super Administrator - Complete system access including ALL modules, 
    /// application settings/configuration, and user management
    /// </summary>
    SuperAdmin = 1,

    /// <summary>
    /// Developer - Complete system access including ALL modules, 
    /// application settings/configuration, and user management (functionally equivalent to SuperAdmin for development purposes)
    /// </summary>
    Developer = 2,

    /// <summary>
    /// Administrator - Access to ALL functional modules (Incident Management, Risk Management, PPE Management, Health Monitoring, Reporting) 
    /// BUT EXCLUDED from application settings/configuration (can access User Management for operational purposes)
    /// </summary>
    Admin = 3,

    /// <summary>
    /// Incident Manager - RESTRICTED access ONLY to Incident Management module and its related features 
    /// (create, read, update, delete incidents, view incident reports)
    /// </summary>
    IncidentManager = 4,

    /// <summary>
    /// Risk Manager - RESTRICTED access ONLY to Risk Management module and its related features 
    /// (create, read, update, delete risk assessments, view risk reports)
    /// </summary>
    RiskManager = 5,

    /// <summary>
    /// PPE Manager - RESTRICTED access ONLY to PPE Management module and its related features 
    /// (PPE tracking, maintenance schedules, compliance monitoring)
    /// </summary>
    PPEManager = 6,

    /// <summary>
    /// Health Monitor - RESTRICTED access ONLY to Health Monitoring module and its related features 
    /// (health data tracking, medical surveillance, health reporting)
    /// </summary>
    HealthMonitor = 7,

    /// <summary>
    /// Security Manager - COMPREHENSIVE access to ALL Security modules (Physical Security, Information Security, Personnel Security)
    /// and their related features (access control, security incidents, security risk assessment, threat monitoring)
    /// </summary>
    SecurityManager = 8,

    /// <summary>
    /// Security Officer - OPERATIONAL access to day-to-day Security operations including Security incident management, 
    /// physical security monitoring, and security data entry (excluding strategic security configuration)
    /// </summary>
    SecurityOfficer = 9,

    /// <summary>
    /// Compliance Officer - ENHANCED access to HSSE compliance management across ALL domains 
    /// (Health, Safety, Security, Environment) including audit management, regulatory reporting, and compliance monitoring
    /// </summary>
    ComplianceOfficer = 10,

    /// <summary>
    /// Reporter - READ-ONLY access to reporting functionality across modules they have permission for
    /// </summary>
    Reporter = 11,

    /// <summary>
    /// Viewer - READ-ONLY access to basic dashboard and summary information (no create/update/delete permissions)
    /// </summary>
    Viewer = 12
}