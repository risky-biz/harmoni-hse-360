namespace Harmoni360.Domain.Enums;

/// <summary>
/// Types of work permits
/// </summary>
public enum WorkPermitType
{
    General,
    HotWork,        // Welding, cutting, grinding
    ColdWork,       // Maintenance, construction
    ConfinedSpace,  // Confined space entry
    ElectricalWork, // Electrical work
    Special         // Radioactive, heights, excavation
}

/// <summary>
/// Status of work permits
/// </summary>
public enum WorkPermitStatus
{
    Draft,
    PendingApproval,
    Approved,
    Rejected,
    InProgress,
    Completed,
    Cancelled,
    Expired
}

/// <summary>
/// Priority levels for work permits
/// </summary>
public enum WorkPermitPriority
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Risk levels for hazards and work permits
/// </summary>
public enum RiskLevel
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Categories of hazards in work permits
/// </summary>
public enum HazardCategory
{
    Physical,
    Chemical,
    Biological,
    Ergonomic,
    Fire,
    Electrical,
    Mechanical,
    Environmental,
    Radiological,
    Behavioral
}

/// <summary>
/// Categories of safety precautions
/// </summary>
public enum PrecautionCategory
{
    PersonalProtectiveEquipment,
    Isolation,
    FireSafety,
    GasMonitoring,
    VentilationControl,
    AccessControl,
    EmergencyProcedures,
    EnvironmentalProtection,
    TrafficControl,
    WeatherPrecautions,
    EquipmentSafety,
    MaterialHandling,
    WasteManagement,
    CommunicationProtocol,
    K3_Compliance, // Indonesian K3 specific requirements
    BPJS_Compliance, // BPJS Ketenagakerjaan requirements
    Environmental_Permit, // AMDAL/UKL-UPL requirements
    Other
}

/// <summary>
/// Types of work permit attachments
/// </summary>
public enum WorkPermitAttachmentType
{
    WorkPlan,
    SafetyProcedure,
    RiskAssessment,
    MethodStatement,
    CertificateOfIsolation,
    PermitToWork,
    PhotoEvidence,
    ComplianceDocument,
    K3License,
    EnvironmentalPermit,
    CompanyPermit,
    Other
}