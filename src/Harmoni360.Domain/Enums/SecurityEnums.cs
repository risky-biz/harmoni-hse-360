namespace Harmoni360.Domain.Enums;

/// <summary>
/// Types of security incidents
/// </summary>
public enum SecurityIncidentType
{
    /// <summary>
    /// Physical security incidents (unauthorized access, theft, vandalism)
    /// </summary>
    PhysicalSecurity = 1,
    
    /// <summary>
    /// Cybersecurity incidents (data breaches, malware, intrusions)
    /// </summary>
    Cybersecurity = 2,
    
    /// <summary>
    /// Personnel security incidents (insider threats, policy violations)
    /// </summary>
    PersonnelSecurity = 3,
    
    /// <summary>
    /// Information security incidents (data leaks, improper handling)
    /// </summary>
    InformationSecurity = 4
}

/// <summary>
/// Categories of security incidents for detailed classification
/// </summary>
public enum SecurityIncidentCategory
{
    // Physical Security Categories (100-199)
    UnauthorizedAccess = 101,
    Theft = 102,
    Vandalism = 103,
    PerimeterBreach = 104,
    SuspiciousActivity = 105,
    PhysicalThreat = 106,
    
    // Cybersecurity Categories (200-299)
    DataBreach = 201,
    MalwareInfection = 202,
    PhishingAttempt = 203,
    SystemIntrusion = 204,
    ServiceDisruption = 205,
    UnauthorizedChange = 206,
    
    // Personnel Security Categories (300-399)
    BackgroundCheckFailure = 301,
    PolicyViolation = 302,
    InsiderThreat = 303,
    CredentialMisuse = 304,
    SecurityTrainingFailure = 305,
    
    // Information Security Categories (400-499)
    DataLeak = 401,
    ImproperDataHandling = 402,
    UnauthorizedDisclosure = 403,
    DataClassificationViolation = 404
}

/// <summary>
/// Severity levels for security incidents
/// </summary>
public enum SecuritySeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// Status of security incidents
/// </summary>
public enum SecurityIncidentStatus
{
    Open = 1,
    Assigned = 2,
    Investigating = 3,
    Contained = 4,
    Eradicating = 5,
    Recovering = 6,
    Resolved = 7,
    Closed = 8
}

/// <summary>
/// Threat levels for security assessment
/// </summary>
public enum ThreatLevel
{
    Minimal = 1,
    Low = 2,
    Medium = 3,
    High = 4,
    Severe = 5
}

/// <summary>
/// Types of threat actors
/// </summary>
public enum ThreatActorType
{
    External = 1,
    Internal = 2,
    Partner = 3,
    Unknown = 4
}

/// <summary>
/// Impact levels for security incidents
/// </summary>
public enum SecurityImpact
{
    None = 0,
    Minor = 1,
    Moderate = 2,
    Major = 3,
    Severe = 4
}

/// <summary>
/// Types of security controls
/// </summary>
public enum SecurityControlType
{
    /// <summary>
    /// Controls that prevent incidents
    /// </summary>
    Preventive = 1,
    
    /// <summary>
    /// Controls that detect incidents
    /// </summary>
    Detective = 2,
    
    /// <summary>
    /// Controls that correct after incidents
    /// </summary>
    Corrective = 3,
    
    /// <summary>
    /// Controls that deter incidents
    /// </summary>
    Deterrent = 4,
    
    /// <summary>
    /// Controls that compensate for other controls
    /// </summary>
    Compensating = 5
}

/// <summary>
/// Categories of security controls
/// </summary>
public enum SecurityControlCategory
{
    /// <summary>
    /// Technical/technological controls
    /// </summary>
    Technical = 1,
    
    /// <summary>
    /// Administrative/procedural controls
    /// </summary>
    Administrative = 2,
    
    /// <summary>
    /// Physical/environmental controls
    /// </summary>
    Physical = 3
}

/// <summary>
/// Implementation status of security controls
/// </summary>
public enum ControlImplementationStatus
{
    Planned = 1,
    Implementing = 2,
    Active = 3,
    UnderReview = 4,
    Retired = 5
}

/// <summary>
/// Types of security incident responses
/// </summary>
public enum SecurityResponseType
{
    /// <summary>
    /// Initial response to incident
    /// </summary>
    Initial = 1,
    
    /// <summary>
    /// Actions to contain the incident
    /// </summary>
    Containment = 2,
    
    /// <summary>
    /// Actions to remove the threat
    /// </summary>
    Eradication = 3,
    
    /// <summary>
    /// Actions to restore normal operations
    /// </summary>
    Recovery = 4,
    
    /// <summary>
    /// Lessons learned and improvements
    /// </summary>
    LessonsLearned = 5
}

/// <summary>
/// Types of security incident attachments
/// </summary>
public enum SecurityAttachmentType
{
    Evidence = 1,
    Screenshot = 2,
    Log = 3,
    Report = 4,
    Other = 5
}

/// <summary>
/// Security audit actions for logging
/// </summary>
public enum SecurityAuditAction
{
    Create = 1,
    Read = 2,
    Update = 3,
    Delete = 4,
    Escalate = 5,
    Assign = 6,
    Close = 7,
    Export = 8,
    Login = 9,
    Logout = 10,
    AccessDenied = 11,
    ConfigurationChange = 12
}

/// <summary>
/// Security audit categories for organization
/// </summary>
public enum SecurityAuditCategory
{
    IncidentManagement = 1,
    ThreatAssessment = 2,
    UserAuthentication = 3,
    DataAccess = 4,
    SystemConfiguration = 5,
    ComplianceReporting = 6,
    ThreatIntelligence = 7
}