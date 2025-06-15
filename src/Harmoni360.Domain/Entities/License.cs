using Harmoni360.Domain.Common;
using Harmoni360.Domain.ValueObjects;
using Harmoni360.Domain.Events;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities;

public class License : BaseEntity, IAuditableEntity
{
    private readonly List<LicenseAttachment> _attachments = new();
    private readonly List<LicenseRenewal> _renewals = new();
    private readonly List<LicenseCondition> _licenseConditions = new();
    private readonly List<LicenseAuditLog> _auditLogs = new();

    // Basic Information
    public string LicenseNumber { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public LicenseType Type { get; private set; }
    public LicenseStatus Status { get; private set; }
    public LicensePriority Priority { get; private set; }
    
    // Issuing Information
    public string IssuingAuthority { get; private set; } = string.Empty;
    public string IssuingAuthorityContact { get; private set; } = string.Empty;
    public DateTime IssuedDate { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public string IssuedLocation { get; private set; } = string.Empty;
    
    // Renewal Information
    public bool RenewalRequired { get; private set; }
    public int RenewalPeriodDays { get; private set; }
    public DateTime? NextRenewalDate { get; private set; }
    public bool AutoRenewal { get; private set; }
    public string RenewalProcedure { get; private set; } = string.Empty;
    
    // Regulatory Information
    public string RegulatoryFramework { get; private set; } = string.Empty;
    public string ApplicableRegulations { get; private set; } = string.Empty;
    public string ComplianceStandards { get; private set; } = string.Empty; // JSON array
    
    // Scope and Coverage
    public string Scope { get; private set; } = string.Empty;
    public string CoverageAreas { get; private set; } = string.Empty; // JSON array
    public string Restrictions { get; private set; } = string.Empty;
    public string Conditions { get; private set; } = string.Empty;
    
    // Business Information
    public int HolderId { get; private set; } // User who holds the license
    public string HolderName { get; private set; } = string.Empty;
    public string Department { get; private set; } = string.Empty;
    public decimal? LicenseFee { get; private set; }
    public string Currency { get; private set; } = "USD";
    
    // Risk and Compliance
    public RiskLevel RiskLevel { get; private set; }
    public bool IsCriticalLicense { get; private set; }
    public bool RequiresInsurance { get; private set; }
    public decimal? RequiredInsuranceAmount { get; private set; }
    
    // Status Tracking
    public DateTime? SubmittedDate { get; private set; }
    public DateTime? ApprovedDate { get; private set; }
    public DateTime? ActivatedDate { get; private set; }
    public DateTime? SuspendedDate { get; private set; }
    public DateTime? RevokedDate { get; private set; }
    public string StatusNotes { get; private set; } = string.Empty;
    
    // Navigation Properties
    public IReadOnlyCollection<LicenseAttachment> Attachments => _attachments.AsReadOnly();
    public IReadOnlyCollection<LicenseRenewal> Renewals => _renewals.AsReadOnly();
    public IReadOnlyCollection<LicenseCondition> LicenseConditions => _licenseConditions.AsReadOnly();
    public IReadOnlyCollection<LicenseAuditLog> AuditLogs => _auditLogs.AsReadOnly();
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    private License() { } // For EF Core

    public static License Create(
        string title,
        string description,
        LicenseType type,
        string licenseNumber,
        string issuingAuthority,
        DateTime issuedDate,
        DateTime expiryDate,
        int holderId,
        string holderName,
        string department = "",
        LicensePriority priority = LicensePriority.Medium)
    {
        var license = new License
        {
            Title = title,
            Description = description,
            Type = type,
            LicenseNumber = licenseNumber,
            IssuingAuthority = issuingAuthority,
            IssuedDate = issuedDate,
            ExpiryDate = expiryDate,
            HolderId = holderId,
            HolderName = holderName,
            Department = department,
            Priority = priority,
            Status = LicenseStatus.Draft,
            RiskLevel = RiskLevel.Medium,
            RenewalRequired = true,
            RenewalPeriodDays = 90,
            CreatedAt = DateTime.UtcNow
        };

        // Calculate next renewal date
        license.CalculateNextRenewalDate();

        // Raise domain event
        license.AddDomainEvent(new LicenseCreatedEvent(license.Id, license.LicenseNumber, license.Type));
        
        return license;
    }

    public void UpdateDetails(
        string title,
        string description,
        string scope,
        string restrictions,
        string conditions,
        LicensePriority priority)
    {
        if (Status != LicenseStatus.Draft && Status != LicenseStatus.Rejected)
            throw new InvalidOperationException("Only draft or rejected licenses can be edited.");

        Title = title;
        Description = description;
        Scope = scope;
        Restrictions = restrictions;
        Conditions = conditions;
        Priority = priority;
        
        AddDomainEvent(new LicenseUpdatedEvent(Id, LicenseNumber));
    }

    public void Submit(string submittedBy)
    {
        if (Status != LicenseStatus.Draft)
            throw new InvalidOperationException("Only draft licenses can be submitted.");
            
        Status = LicenseStatus.Submitted;
        SubmittedDate = DateTime.UtcNow;
        
        AddDomainEvent(new LicenseSubmittedEvent(Id, LicenseNumber, submittedBy));
    }

    public void Approve(string approvedBy, string approvalNotes = "")
    {
        if (Status != LicenseStatus.Submitted && Status != LicenseStatus.UnderReview)
            throw new InvalidOperationException("Only submitted or under review licenses can be approved.");

        Status = LicenseStatus.Approved;
        ApprovedDate = DateTime.UtcNow;
        StatusNotes = approvalNotes;
        
        AddDomainEvent(new LicenseApprovedEvent(Id, LicenseNumber, approvedBy));
    }

    public void Reject(string rejectedBy, string rejectionReason)
    {
        if (Status != LicenseStatus.Submitted && Status != LicenseStatus.UnderReview)
            throw new InvalidOperationException("Only submitted or under review licenses can be rejected.");

        Status = LicenseStatus.Rejected;
        StatusNotes = rejectionReason;
        
        AddDomainEvent(new LicenseRejectedEvent(Id, LicenseNumber, rejectedBy, rejectionReason));
    }

    public void Activate(string activatedBy)
    {
        if (Status != LicenseStatus.Approved)
            throw new InvalidOperationException("Only approved licenses can be activated.");
            
        Status = LicenseStatus.Active;
        ActivatedDate = DateTime.UtcNow;
        
        AddDomainEvent(new LicenseActivatedEvent(Id, LicenseNumber, activatedBy));
    }

    public void Suspend(string suspendedBy, string suspensionReason)
    {
        if (Status != LicenseStatus.Active)
            throw new InvalidOperationException("Only active licenses can be suspended.");
            
        Status = LicenseStatus.Suspended;
        SuspendedDate = DateTime.UtcNow;
        StatusNotes = suspensionReason;
        
        AddDomainEvent(new LicenseSuspendedEvent(Id, LicenseNumber, suspendedBy, suspensionReason));
    }

    public void Revoke(string revokedBy, string revocationReason)
    {
        if (Status == LicenseStatus.Revoked)
            throw new InvalidOperationException("License is already revoked.");
            
        Status = LicenseStatus.Revoked;
        RevokedDate = DateTime.UtcNow;
        StatusNotes = revocationReason;
        
        AddDomainEvent(new LicenseRevokedEvent(Id, LicenseNumber, revokedBy, revocationReason));
    }

    public void InitiateRenewal(string initiatedBy)
    {
        if (Status != LicenseStatus.Active && Status != LicenseStatus.Expired)
            throw new InvalidOperationException("Only active or expired licenses can be renewed.");
            
        Status = LicenseStatus.PendingRenewal;
        
        var renewal = LicenseRenewal.Create(Id, CalculateNewExpiryDate(), initiatedBy);
        _renewals.Add(renewal);
        
        AddDomainEvent(new LicenseRenewalInitiatedEvent(Id, LicenseNumber, renewal.Id, renewal.RenewalNumber));
    }

    public void SetRenewalInformation(bool renewalRequired, int renewalPeriodDays, bool autoRenewal = false, string renewalProcedure = "")
    {
        RenewalRequired = renewalRequired;
        RenewalPeriodDays = renewalPeriodDays;
        AutoRenewal = autoRenewal;
        RenewalProcedure = renewalProcedure;
        
        if (renewalRequired)
        {
            CalculateNextRenewalDate();
        }
    }

    public void SetRegulatoryInformation(string regulatoryFramework, string applicableRegulations, string complianceStandards)
    {
        RegulatoryFramework = regulatoryFramework;
        ApplicableRegulations = applicableRegulations;
        ComplianceStandards = complianceStandards;
    }

    public void SetRiskAndCompliance(RiskLevel riskLevel, bool isCritical, bool requiresInsurance = false, decimal? insuranceAmount = null)
    {
        RiskLevel = riskLevel;
        IsCriticalLicense = isCritical;
        RequiresInsurance = requiresInsurance;
        RequiredInsuranceAmount = insuranceAmount;
    }

    public void AddAttachment(LicenseAttachment attachment)
    {
        _attachments.Add(attachment);
        AddDomainEvent(new LicenseAttachmentAddedEvent(Id, attachment.Id, attachment.FileName, attachment.AttachmentType));
    }

    public void RemoveAttachment(int attachmentId)
    {
        var attachment = _attachments.FirstOrDefault(a => a.Id == attachmentId);
        if (attachment != null)
        {
            _attachments.Remove(attachment);
            AddDomainEvent(new LicenseAttachmentRemovedEvent(Id, attachment.Id, attachment.FileName));
        }
    }

    public void AddCondition(LicenseCondition condition)
    {
        _licenseConditions.Add(condition);
    }

    public void LogAuditAction(LicenseAuditAction action, string actionDescription, string performedBy, string oldValues = "", string newValues = "")
    {
        var auditLog = LicenseAuditLog.Create(Id, action, actionDescription, performedBy, oldValues, newValues);
        _auditLogs.Add(auditLog);
    }

    // Computed Properties
    public bool IsExpired => DateTime.UtcNow > ExpiryDate;
    public bool IsExpiringSoon => (ExpiryDate - DateTime.UtcNow).TotalDays <= 30;
    public int DaysUntilExpiry => (int)(ExpiryDate - DateTime.UtcNow).TotalDays;
    public bool RequiresRenewal => RenewalRequired && IsExpiringSoon;
    public double ComplianceScore => CalculateComplianceScore();

    private void CalculateNextRenewalDate()
    {
        if (RenewalRequired && RenewalPeriodDays > 0)
        {
            NextRenewalDate = ExpiryDate.AddDays(-RenewalPeriodDays);
        }
    }

    private DateTime CalculateNewExpiryDate()
    {
        // Default renewal extends license for same period as original
        var originalPeriod = (ExpiryDate - IssuedDate).Days;
        return ExpiryDate.AddDays(originalPeriod);
    }

    private double CalculateComplianceScore()
    {
        double score = 100.0;
        
        // Deduct points for expired license
        if (IsExpired) score -= 50.0;
        
        // Deduct points for missing renewal
        if (RequiresRenewal && Status != LicenseStatus.PendingRenewal) score -= 20.0;
        
        // Deduct points for incomplete conditions
        var totalConditions = _licenseConditions.Count;
        if (totalConditions > 0)
        {
            var completedConditions = _licenseConditions.Count(c => c.Status == LicenseConditionStatus.Completed);
            var conditionScore = (double)completedConditions / totalConditions * 30.0;
            score = score - 30.0 + conditionScore;
        }
        
        return Math.Max(0, score);
    }
}