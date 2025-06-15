namespace Harmoni360.Domain.Enums;

/// <summary>
/// Defines the functional modules in the Harmoni360 system.
/// Each module represents a distinct feature area with specific permissions.
/// </summary>
public enum ModuleType
{
    /// <summary>
    /// Dashboard Module - Main dashboard with summary information and key metrics
    /// </summary>
    Dashboard = 1,

    /// <summary>
    /// Incident Management Module - Incident CRUD operations, incident reporting, incident analytics, corrective actions
    /// </summary>
    IncidentManagement = 2,

    /// <summary>
    /// Risk Management Module - Risk assessment CRUD operations, risk reporting, risk analytics, hazard identification
    /// </summary>
    RiskManagement = 3,

    /// <summary>
    /// PPE Management Module - PPE tracking, inventory management, maintenance schedules, compliance monitoring
    /// </summary>
    PPEManagement = 4,

    /// <summary>
    /// Health Monitoring Module - Health data tracking, medical surveillance, health reporting, vaccination compliance
    /// </summary>
    HealthMonitoring = 5,

    /// <summary>
    /// Physical Security Module - Access control systems, visitor management, asset security, surveillance integration
    /// </summary>
    PhysicalSecurity = 6,

    /// <summary>
    /// Information Security Module - Security policies, vulnerability management, data protection, ISMS compliance
    /// </summary>
    InformationSecurity = 7,

    /// <summary>
    /// Personnel Security Module - Background verification, security training, insider threat management
    /// </summary>
    PersonnelSecurity = 8,

    /// <summary>
    /// Security Incident Management Module - Security-specific incident handling, threat response, security investigations
    /// </summary>
    SecurityIncidentManagement = 9,

    /// <summary>
    /// Compliance Management Module - Regulatory compliance, audit management, compliance monitoring across HSSE domains
    /// </summary>
    ComplianceManagement = 10,

    /// <summary>
    /// Reporting Module - Cross-module reporting, analytics dashboards, data export functionality
    /// </summary>
    Reporting = 11,

    /// <summary>
    /// User Management Module - User CRUD operations, role assignments, access control management
    /// (SuperAdmin/Developer/Admin only)
    /// </summary>
    UserManagement = 12,

    /// <summary>
    /// Work Permit Management Module - Work permit creation, approval workflow, compliance tracking, safety oversight
    /// </summary>
    WorkPermitManagement = 14,

    /// <summary>
    /// Inspection Management Module - Safety, environmental, equipment, and compliance inspections with comprehensive workflow management
    /// </summary>
    InspectionManagement = 15,

    /// <summary>
    /// Audit Management Module - Comprehensive HSSE audit management with checklist-based assessments, findings tracking, and compliance monitoring
    /// </summary>
    AuditManagement = 16,

    /// <summary>
    /// Training Management Module - Comprehensive HSSE training management with participant tracking, certification management, and compliance monitoring
    /// </summary>
    TrainingManagement = 17,

    /// <summary>
    /// License Management Module - Comprehensive license, permit, and certification management with renewal tracking, compliance monitoring, and regulatory oversight
    /// </summary>
    LicenseManagement = 18,

    /// <summary>
    /// Application Settings Module - System configuration, module settings, security settings
    /// (SuperAdmin/Developer only)
    /// </summary>
    ApplicationSettings = 19
    /// Application Settings Module - System configuration, module settings, security settings
    /// (SuperAdmin/Developer only)
    /// </summary>
    ApplicationSettings = 16,

    /// <summary>
    /// Waste Management Module - Waste reporting, classification, disposal tracking
    /// </summary>
    WasteManagement = 17
}
