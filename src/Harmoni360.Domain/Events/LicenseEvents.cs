using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Events;

/// <summary>
/// Domain event raised when a new license is created
/// </summary>
public class LicenseCreatedEvent : IDomainEvent
{
    public int LicenseId { get; }
    public string LicenseNumber { get; }
    public LicenseType LicenseType { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public LicenseCreatedEvent(int licenseId, string licenseNumber, LicenseType licenseType)
    {
        LicenseId = licenseId;
        LicenseNumber = licenseNumber;
        LicenseType = licenseType;
    }
}

/// <summary>
/// Domain event raised when a license is updated
/// </summary>
public class LicenseUpdatedEvent : IDomainEvent
{
    public int LicenseId { get; }
    public string LicenseNumber { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public LicenseUpdatedEvent(int licenseId, string licenseNumber)
    {
        LicenseId = licenseId;
        LicenseNumber = licenseNumber;
    }
}

/// <summary>
/// Domain event raised when a license is submitted for approval
/// </summary>
public class LicenseSubmittedEvent : IDomainEvent
{
    public int LicenseId { get; }
    public string LicenseNumber { get; }
    public string SubmittedBy { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public LicenseSubmittedEvent(int licenseId, string licenseNumber, string submittedBy)
    {
        LicenseId = licenseId;
        LicenseNumber = licenseNumber;
        SubmittedBy = submittedBy;
    }
}

/// <summary>
/// Domain event raised when a license is approved
/// </summary>
public class LicenseApprovedEvent : IDomainEvent
{
    public int LicenseId { get; }
    public string LicenseNumber { get; }
    public string ApprovedBy { get; }
    public string? ApprovalNotes { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public LicenseApprovedEvent(int licenseId, string licenseNumber, string approvedBy, string? approvalNotes = null)
    {
        LicenseId = licenseId;
        LicenseNumber = licenseNumber;
        ApprovedBy = approvedBy;
        ApprovalNotes = approvalNotes;
    }
}

/// <summary>
/// Domain event raised when a license is rejected
/// </summary>
public class LicenseRejectedEvent : IDomainEvent
{
    public int LicenseId { get; }
    public string LicenseNumber { get; }
    public string RejectedBy { get; }
    public string RejectionReason { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public LicenseRejectedEvent(int licenseId, string licenseNumber, string rejectedBy, string rejectionReason)
    {
        LicenseId = licenseId;
        LicenseNumber = licenseNumber;
        RejectedBy = rejectedBy;
        RejectionReason = rejectionReason;
    }
}

/// <summary>
/// Domain event raised when a license is activated
/// </summary>
public class LicenseActivatedEvent : IDomainEvent
{
    public int LicenseId { get; }
    public string LicenseNumber { get; }
    public string ActivatedBy { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public LicenseActivatedEvent(int licenseId, string licenseNumber, string activatedBy)
    {
        LicenseId = licenseId;
        LicenseNumber = licenseNumber;
        ActivatedBy = activatedBy;
    }
}

/// <summary>
/// Domain event raised when a license is suspended
/// </summary>
public class LicenseSuspendedEvent : IDomainEvent
{
    public int LicenseId { get; }
    public string LicenseNumber { get; }
    public string SuspendedBy { get; }
    public string SuspensionReason { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public LicenseSuspendedEvent(int licenseId, string licenseNumber, string suspendedBy, string suspensionReason)
    {
        LicenseId = licenseId;
        LicenseNumber = licenseNumber;
        SuspendedBy = suspendedBy;
        SuspensionReason = suspensionReason;
    }
}

/// <summary>
/// Domain event raised when a license is revoked
/// </summary>
public class LicenseRevokedEvent : IDomainEvent
{
    public int LicenseId { get; }
    public string LicenseNumber { get; }
    public string RevokedBy { get; }
    public string RevocationReason { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public LicenseRevokedEvent(int licenseId, string licenseNumber, string revokedBy, string revocationReason)
    {
        LicenseId = licenseId;
        LicenseNumber = licenseNumber;
        RevokedBy = revokedBy;
        RevocationReason = revocationReason;
    }
}

/// <summary>
/// Domain event raised when a license renewal is initiated
/// </summary>
public class LicenseRenewalInitiatedEvent : IDomainEvent
{
    public int LicenseId { get; }
    public string LicenseNumber { get; }
    public int RenewalId { get; }
    public string RenewalNumber { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public LicenseRenewalInitiatedEvent(int licenseId, string licenseNumber, int renewalId, string renewalNumber)
    {
        LicenseId = licenseId;
        LicenseNumber = licenseNumber;
        RenewalId = renewalId;
        RenewalNumber = renewalNumber;
    }
}

/// <summary>
/// Domain event raised when an attachment is added to a license
/// </summary>
public class LicenseAttachmentAddedEvent : IDomainEvent
{
    public int LicenseId { get; }
    public int AttachmentId { get; }
    public string FileName { get; }
    public LicenseAttachmentType AttachmentType { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public LicenseAttachmentAddedEvent(int licenseId, int attachmentId, string fileName, LicenseAttachmentType attachmentType)
    {
        LicenseId = licenseId;
        AttachmentId = attachmentId;
        FileName = fileName;
        AttachmentType = attachmentType;
    }
}

/// <summary>
/// Domain event raised when an attachment is removed from a license
/// </summary>
public class LicenseAttachmentRemovedEvent : IDomainEvent
{
    public int LicenseId { get; }
    public int AttachmentId { get; }
    public string FileName { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public LicenseAttachmentRemovedEvent(int licenseId, int attachmentId, string fileName)
    {
        LicenseId = licenseId;
        AttachmentId = attachmentId;
        FileName = fileName;
    }
}

/// <summary>
/// Domain event raised when a license is approaching expiry
/// </summary>
public class LicenseExpiringEvent : IDomainEvent
{
    public int LicenseId { get; }
    public string LicenseNumber { get; }
    public DateTime ExpiryDate { get; }
    public int DaysUntilExpiry { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public LicenseExpiringEvent(int licenseId, string licenseNumber, DateTime expiryDate, int daysUntilExpiry)
    {
        LicenseId = licenseId;
        LicenseNumber = licenseNumber;
        ExpiryDate = expiryDate;
        DaysUntilExpiry = daysUntilExpiry;
    }
}

/// <summary>
/// Domain event raised when a license has expired
/// </summary>
public class LicenseExpiredEvent : IDomainEvent
{
    public int LicenseId { get; }
    public string LicenseNumber { get; }
    public DateTime ExpiryDate { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public LicenseExpiredEvent(int licenseId, string licenseNumber, DateTime expiryDate)
    {
        LicenseId = licenseId;
        LicenseNumber = licenseNumber;
        ExpiryDate = expiryDate;
    }
}