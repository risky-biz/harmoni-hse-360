namespace Harmoni360.Domain.Enums;

/// <summary>
/// Types of training programs
/// </summary>
public enum TrainingType
{
    SafetyOrientation,
    HSETraining,
    TechnicalSkills,
    LeadershipDevelopment,
    Compliance,
    EmergencyResponse,
    EquipmentOperation,
    QualityManagement,
    EnvironmentalAwareness,
    K3Training,           // Indonesian Keselamatan dan Kesehatan Kerja
    BPJSCompliance,       // BPJS training requirements
    PermitToWork,
    ConfinedSpaceEntry,
    HotWorkSafety,
    ElectricalSafety,
    FireSafety,
    ChemicalHandling,
    PersonalProtectiveEquipment,
    RiskAssessment,
    IncidentInvestigation,
    Other
}

/// <summary>
/// Status of training programs
/// </summary>
public enum TrainingStatus
{
    Draft,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
    Postponed,
    UnderReview,
    Approved,
    Rejected
}

/// <summary>
/// Priority levels for training
/// </summary>
public enum TrainingPriority
{
    Low,
    Medium,
    High,
    Critical,
    Mandatory
}

/// <summary>
/// Training delivery methods
/// </summary>
public enum TrainingDeliveryMethod
{
    InPerson,
    Online,
    Hybrid,
    SelfPaced,
    OnTheJob,
    Simulator,
    Workshop,
    Seminar,
    Conference,
    Webinar
}

/// <summary>
/// Training categories for organization
/// </summary>
public enum TrainingCategory
{
    MandatoryCompliance,
    ProfessionalDevelopment,
    SafetyTraining,
    TechnicalTraining,
    SoftSkills,
    LeadershipTraining,
    InductionTraining,
    RefresherTraining,
    SpecializedTraining,
    ContinuousEducation
}

/// <summary>
/// Training participant enrollment status
/// </summary>
public enum EnrollmentStatus
{
    Enrolled,
    Pending,
    WaitingList,
    Confirmed,
    Withdrawn,
    Cancelled,
    Rejected
}

/// <summary>
/// Training participant attendance status
/// </summary>
public enum AttendanceStatus
{
    NotMarked,
    Present,
    Absent,
    PartialAttendance,
    Late,
    ExcusedAbsence,
    MedicalLeave,
    Emergency
}

/// <summary>
/// Training participant status
/// </summary>
public enum ParticipantStatus
{
    Enrolled,
    Attending,
    Completed,
    Failed,
    NoShow,
    Withdrawn,
    Pending,
    WaitingList
}

/// <summary>
/// Training requirement status
/// </summary>
public enum RequirementStatus
{
    Pending,
    InProgress,
    Completed,
    Overdue,
    Waived,
    NotApplicable
}

/// <summary>
/// Training assessment methods
/// </summary>
public enum AssessmentMethod
{
    Written,
    Practical,
    Oral,
    Online,
    Simulation,
    Observation,
    Portfolio,
    ProjectBased,
    PeerReview,
    SelfAssessment
}

/// <summary>
/// Training certification types
/// </summary>
public enum CertificationType
{
    Completion,
    Competency,
    Professional,
    Regulatory,
    Internal,
    External,
    K3Certificate,      // Indonesian K3 Certification
    AK3Certificate,     // Ahli K3 Umum Certificate
    POP_Certificate,    // Petugas Operasi Produksi Certificate
    ISO_Certificate,    // ISO related certifications
    OHSAS_Certificate,  // OHSAS 18001 related
    Temporary,
    Permanent
}

/// <summary>
/// Training attachment types
/// </summary>
public enum TrainingAttachmentType
{
    CourseOutline,
    Presentation,
    Handbook,
    Video,
    Assessment,
    Certificate,
    HandoutMaterial,
    ReferenceDocument,
    SafetyDataSheet,
    OperatingProcedure,
    K3Regulation,
    ComplianceDocument,
    PhotoEvidence,
    AttendanceSheet,
    EvaluationForm,
    Other
}

/// <summary>
/// Training comment types for better organization
/// </summary>
public enum TrainingCommentType
{
    General,
    Feedback,
    Issue,
    Improvement,
    Clarification,
    AdminNote,
    InstructorNote,
    ParticipantFeedback,
    AssessmentComment,
    ComplianceNote
}

/// <summary>
/// Training validity periods
/// </summary>
public enum ValidityPeriod
{
    OneMonth,
    ThreeMonths,
    SixMonths,
    OneYear,
    TwoYears,
    ThreeYears,
    FiveYears,
    Indefinite,
    CustomPeriod
}

/// <summary>
/// Training approval levels
/// </summary>
public enum TrainingApprovalLevel
{
    DirectSupervisor,
    DepartmentHead,
    HRManager,
    TrainingManager,
    HSEManager,
    PlantManager,
    K3Officer,          // Indonesian K3 Officer
    AhliK3,            // K3 Expert
    ExternalValidator,
    RegulatoryBody
}