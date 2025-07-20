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
    /// Inspection Manager - RESTRICTED access ONLY to Inspection Management module and its related features 
    /// (inspection creation, scheduling, completion, findings management, compliance tracking)
    /// </summary>
    InspectionManager = 8,

    /// <summary>
    /// Security Manager - COMPREHENSIVE access to ALL Security modules (Physical Security, Information Security, Personnel Security)
    /// and their related features (access control, security incidents, security risk assessment, threat monitoring)
    /// </summary>
    SecurityManager = 9,

    /// <summary>
    /// Security Officer - OPERATIONAL access to day-to-day Security operations including Security incident management, 
    /// physical security monitoring, and security data entry (excluding strategic security configuration)
    /// </summary>
    SecurityOfficer = 10,

    /// <summary>
    /// Compliance Officer - ENHANCED access to HSSE compliance management across ALL domains 
    /// (Health, Safety, Security, Environment) including audit management, regulatory reporting, and compliance monitoring
    /// </summary>
    ComplianceOfficer = 11,

    /// <summary>
    /// Safety Officer - SPECIALIZED access to safety operations and work permit approvals
    /// (safety inspections, work permit reviews, safety compliance)
    /// </summary>
    SafetyOfficer = 12,

    /// <summary>
    /// Department Head - DEPARTMENTAL management access with approval authority
    /// (department-level approvals, team management, departmental oversight)
    /// </summary>
    DepartmentHead = 13,

    /// <summary>
    /// Hot Work Specialist - SPECIALIZED access to hot work permit approvals and oversight
    /// (hot work safety, fire prevention, welding/cutting safety)
    /// </summary>
    HotWorkSpecialist = 14,

    /// <summary>
    /// Confined Space Specialist - SPECIALIZED access to confined space permit approvals
    /// (confined space entry safety, atmospheric monitoring, rescue procedures)
    /// </summary>
    ConfinedSpaceSpecialist = 15,

    /// <summary>
    /// Electrical Supervisor - SPECIALIZED access to electrical work permit approvals
    /// (electrical safety, lockout/tagout, electrical hazard assessment)
    /// </summary>
    ElectricalSupervisor = 16,

    /// <summary>
    /// Special Work Specialist - SPECIALIZED access to special work permit approvals
    /// (unique/high-risk work activities, specialized safety procedures)
    /// </summary>
    SpecialWorkSpecialist = 17,

    /// <summary>
    /// HSE Manager - COMPREHENSIVE access to Health, Safety, and Environment management
    /// (HSE strategy, high-level approvals, HSE compliance oversight)
    /// </summary>
    HSEManager = 18,

    /// <summary>
    /// Reporter - READ-ONLY access to reporting functionality across modules they have permission for
    /// </summary>
    Reporter = 19,

    /// <summary>
    /// Viewer - READ-ONLY access to basic dashboard and summary information (no create/update/delete permissions)
    /// </summary>
    Viewer = 20,

    /// <summary>
    /// Workflow Manager - RESTRICTED access ONLY to Workflow Management module and its related features
    /// (workflow design, workflow instances, process automation, workflow reporting)
    /// </summary>
    WorkflowManager = 21
}