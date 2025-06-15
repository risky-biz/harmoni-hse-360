using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities.Audits;

public class AuditFinding : BaseEntity, IAuditableEntity
{
    private readonly List<FindingAttachment> _attachments = new();

    public int AuditId { get; private set; }
    public string FindingNumber { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public FindingType Type { get; private set; }
    public FindingSeverity Severity { get; private set; }
    public RiskLevel RiskLevel { get; private set; }
    public FindingStatus Status { get; private set; }
    
    // Context Information
    public string? Location { get; private set; }
    public string? Equipment { get; private set; }
    public string? Standard { get; private set; }
    public string? Regulation { get; private set; }
    public int? AuditItemId { get; private set; }
    
    // Root Cause and Actions
    public string? RootCause { get; private set; }
    public string? ImmediateAction { get; private set; }
    public string? CorrectiveAction { get; private set; }
    public string? PreventiveAction { get; private set; }
    
    // Responsibility and Timeline
    public DateTime? DueDate { get; private set; }
    public int? ResponsiblePersonId { get; private set; }
    public string? ResponsiblePersonName { get; private set; }
    
    // Closure Information
    public DateTime? ClosedDate { get; private set; }
    public string? ClosureNotes { get; private set; }
    public string? ClosedBy { get; private set; }
    public string? VerificationMethod { get; private set; }
    public bool RequiresVerification { get; private set; }
    public DateTime? VerificationDate { get; private set; }
    public string? VerifiedBy { get; private set; }
    
    // Cost and Impact
    public decimal? EstimatedCost { get; private set; }
    public decimal? ActualCost { get; private set; }
    public string? BusinessImpact { get; private set; }
    
    // Navigation Properties
    public virtual Audit Audit { get; private set; } = null!;
    public virtual AuditItem? AuditItem { get; private set; }
    public virtual User? ResponsiblePerson { get; private set; }
    public virtual IReadOnlyCollection<FindingAttachment> Attachments => _attachments.AsReadOnly();
    
    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    private AuditFinding() { }

    public static AuditFinding Create(
        int auditId,
        string description,
        FindingType type,
        FindingSeverity severity,
        string? location = null,
        string? equipment = null,
        int? auditItemId = null)
    {
        var finding = new AuditFinding
        {
            AuditId = auditId,
            Description = description,
            Type = type,
            Severity = severity,
            Status = FindingStatus.Open,
            Location = location,
            Equipment = equipment,
            AuditItemId = auditItemId,
            RequiresVerification = severity >= FindingSeverity.Major,
            CreatedAt = DateTime.UtcNow
        };

        finding.FindingNumber = GenerateFindingNumber();
        finding.RiskLevel = MapSeverityToRiskLevel(severity);

        return finding;
    }

    public void UpdateDescription(string description)
    {
        if (Status == FindingStatus.Closed)
            throw new InvalidOperationException("Cannot update closed finding");

        Description = description;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateSeverity(FindingSeverity severity)
    {
        if (Status == FindingStatus.Closed)
            throw new InvalidOperationException("Cannot update closed finding");

        Severity = severity;
        RiskLevel = MapSeverityToRiskLevel(severity);
        RequiresVerification = severity >= FindingSeverity.Major;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetContext(
        string? location = null,
        string? equipment = null,
        string? standard = null,
        string? regulation = null)
    {
        Location = location;
        Equipment = equipment;
        Standard = standard;
        Regulation = regulation;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetRootCause(string rootCause)
    {
        RootCause = rootCause;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetImmediateAction(string immediateAction)
    {
        ImmediateAction = immediateAction;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetCorrectiveAction(
        string correctiveAction,
        DateTime? dueDate = null,
        int? responsiblePersonId = null,
        string? responsiblePersonName = null)
    {
        CorrectiveAction = correctiveAction;
        DueDate = dueDate;
        ResponsiblePersonId = responsiblePersonId;
        ResponsiblePersonName = responsiblePersonName;
        
        if (Status == FindingStatus.Open)
        {
            Status = FindingStatus.InProgress;
        }
        
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetPreventiveAction(string preventiveAction)
    {
        PreventiveAction = preventiveAction;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetCostInformation(decimal? estimatedCost, decimal? actualCost = null, string? businessImpact = null)
    {
        EstimatedCost = estimatedCost;
        ActualCost = actualCost;
        BusinessImpact = businessImpact;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void MarkAsResolved()
    {
        if (Status != FindingStatus.InProgress)
            throw new InvalidOperationException("Only in-progress findings can be marked as resolved");

        Status = FindingStatus.Resolved;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void MarkAsVerified(string verifiedBy, string? verificationMethod = null)
    {
        if (Status != FindingStatus.Resolved)
            throw new InvalidOperationException("Only resolved findings can be verified");

        Status = FindingStatus.Verified;
        VerifiedBy = verifiedBy;
        VerificationDate = DateTime.UtcNow;
        VerificationMethod = verificationMethod;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Close(string closureNotes, string closedBy)
    {
        if (RequiresVerification && Status != FindingStatus.Verified)
            throw new InvalidOperationException("Critical and major findings must be verified before closing");
        
        if (Status != FindingStatus.Verified && Status != FindingStatus.Resolved)
            throw new InvalidOperationException("Only verified or resolved findings can be closed");

        Status = FindingStatus.Closed;
        ClosedDate = DateTime.UtcNow;
        ClosureNotes = closureNotes;
        ClosedBy = closedBy;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Reopen(string reason)
    {
        if (Status == FindingStatus.Closed)
        {
            Status = string.IsNullOrEmpty(CorrectiveAction) ? FindingStatus.Open : FindingStatus.InProgress;
            ClosedDate = null;
            ClosureNotes = null;
            ClosedBy = null;
            VerificationDate = null;
            VerifiedBy = null;
            VerificationMethod = null;
            LastModifiedAt = DateTime.UtcNow;
        }
    }

    public void AddAttachment(FindingAttachment attachment)
    {
        _attachments.Add(attachment);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void RemoveAttachment(FindingAttachment attachment)
    {
        _attachments.Remove(attachment);
        LastModifiedAt = DateTime.UtcNow;
    }

    private static RiskLevel MapSeverityToRiskLevel(FindingSeverity severity)
    {
        return severity switch
        {
            FindingSeverity.Critical => RiskLevel.Critical,
            FindingSeverity.Major => RiskLevel.High,
            FindingSeverity.Moderate => RiskLevel.Medium,
            FindingSeverity.Minor => RiskLevel.Low,
            _ => RiskLevel.Low
        };
    }

    private static string GenerateFindingNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var guid = Guid.NewGuid().ToString("N")[..6].ToUpper();
        return $"FND-{timestamp}-{guid}";
    }

    // Computed Properties
    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && Status != FindingStatus.Closed;
    public bool CanEdit => Status != FindingStatus.Closed;
    public bool CanClose => RequiresVerification ? Status == FindingStatus.Verified : Status == FindingStatus.Resolved;
    public bool HasCorrectiveAction => !string.IsNullOrEmpty(CorrectiveAction);
    public bool IsCritical => Severity == FindingSeverity.Critical;
    public bool IsHighPriority => Severity >= FindingSeverity.Major;
    
    public int DaysOverdue => IsOverdue ? (DateTime.UtcNow.Date - DueDate!.Value.Date).Days : 0;
    
    public string StatusDisplay => Status switch
    {
        FindingStatus.Open => "Open",
        FindingStatus.InProgress => "In Progress",
        FindingStatus.Resolved => "Resolved",
        FindingStatus.Verified => "Verified",
        FindingStatus.Closed => "Closed",
        _ => "Unknown"
    };
}