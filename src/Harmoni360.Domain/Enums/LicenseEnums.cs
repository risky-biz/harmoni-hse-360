namespace Harmoni360.Domain.Enums;

/// <summary>
/// Types of licenses managed in the system
/// </summary>
public enum LicenseType
{
    Environmental = 1,
    Safety = 2,
    Health = 3,
    Construction = 4,
    Operating = 5,
    Transport = 6,
    Waste = 7,
    Chemical = 8,
    Radiation = 9,
    Fire = 10,
    Electrical = 11,
    Mechanical = 12,
    Professional = 13,
    Business = 14,
    Import = 15,
    Export = 16,
    Other = 17
}

/// <summary>
/// Current status of a license in its lifecycle
/// </summary>
public enum LicenseStatus
{
    Draft = 1,
    PendingSubmission = 2,
    Submitted = 3,
    UnderReview = 4,
    Approved = 5,
    Active = 6,
    Rejected = 7,
    Expired = 8,
    Suspended = 9,
    Revoked = 10,
    PendingRenewal = 11
}

/// <summary>
/// Priority levels for licenses based on risk and business impact
/// </summary>
public enum LicensePriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// Types of documents that can be attached to a license
/// </summary>
public enum LicenseAttachmentType
{
    Application = 1,
    SupportingDocument = 2,
    Certificate = 3,
    Compliance = 4,
    Insurance = 5,
    TechnicalSpec = 6,
    LegalDocument = 7,
    RenewalDocument = 8,
    InspectionReport = 9,
    Other = 10
}

/// <summary>
/// Status of license renewal applications
/// </summary>
public enum LicenseRenewalStatus
{
    Draft = 1,
    Submitted = 2,
    UnderReview = 3,
    Approved = 4,
    Rejected = 5,
    Expired = 6
}

/// <summary>
/// Status of license conditions and compliance requirements
/// </summary>
public enum LicenseConditionStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Overdue = 4,
    Waived = 5
}

/// <summary>
/// Types of audit actions performed on licenses
/// </summary>
public enum LicenseAuditAction
{
    Created = 1,
    Updated = 2,
    Submitted = 3,
    Approved = 4,
    Rejected = 5,
    Activated = 6,
    Suspended = 7,
    Revoked = 8,
    Renewed = 9,
    AttachmentAdded = 10,
    AttachmentRemoved = 11,
    ConditionAdded = 12,
    ConditionUpdated = 13,
    StatusChanged = 14
}