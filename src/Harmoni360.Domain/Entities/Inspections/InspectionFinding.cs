using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities.Inspections;

public class InspectionFinding : BaseEntity, IAuditableEntity
{
    private readonly List<FindingAttachment> _attachments = new();

    public int InspectionId { get; private set; }
    public string FindingNumber { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public FindingType Type { get; private set; }
    public FindingSeverity Severity { get; private set; }
    public RiskLevel RiskLevel { get; private set; }
    public string? RootCause { get; private set; }
    public string? ImmediateAction { get; private set; }
    public string? CorrectiveAction { get; private set; }
    public DateTime? DueDate { get; private set; }
    public int? ResponsiblePersonId { get; private set; }
    public FindingStatus Status { get; private set; }
    public string? Location { get; private set; }
    public string? Equipment { get; private set; }
    public string? Regulation { get; private set; }
    public DateTime? ClosedDate { get; private set; }
    public string? ClosureNotes { get; private set; }

    // Navigation Properties
    public virtual Inspection Inspection { get; private set; } = null!;
    public virtual User? ResponsiblePerson { get; private set; }
    public virtual IReadOnlyCollection<FindingAttachment> Attachments => _attachments.AsReadOnly();

    // IAuditableEntity
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    private InspectionFinding() { }

    public static InspectionFinding Create(
        int inspectionId,
        string description,
        FindingType type,
        FindingSeverity severity,
        string? location = null,
        string? equipment = null)
    {
        var finding = new InspectionFinding
        {
            InspectionId = inspectionId,
            Description = description,
            Type = type,
            Severity = severity,
            Status = FindingStatus.Open,
            Location = location,
            Equipment = equipment,
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

    public void SetCorrectiveAction(string correctiveAction, DateTime? dueDate = null, int? responsiblePersonId = null)
    {
        CorrectiveAction = correctiveAction;
        DueDate = dueDate;
        ResponsiblePersonId = responsiblePersonId;
        
        if (Status == FindingStatus.Open)
        {
            Status = FindingStatus.InProgress;
        }
        
        LastModifiedAt = DateTime.UtcNow;
    }

    public void MarkAsResolved()
    {
        if (Status != FindingStatus.InProgress)
            throw new InvalidOperationException("Only in-progress findings can be marked as resolved");

        Status = FindingStatus.Resolved;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void MarkAsVerified()
    {
        if (Status != FindingStatus.Resolved)
            throw new InvalidOperationException("Only resolved findings can be verified");

        Status = FindingStatus.Verified;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Close(string closureNotes)
    {
        if (Status != FindingStatus.Verified)
            throw new InvalidOperationException("Only verified findings can be closed");

        Status = FindingStatus.Closed;
        ClosedDate = DateTime.UtcNow;
        ClosureNotes = closureNotes;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Reopen(string reason)
    {
        if (Status == FindingStatus.Closed)
        {
            Status = FindingStatus.Open;
            ClosedDate = null;
            ClosureNotes = null;
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

    public void SetRegulation(string regulation)
    {
        Regulation = regulation;
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
        var guid = Guid.NewGuid().ToString("N")[..6].ToUpper(); // Use first 6 chars of GUID for uniqueness
        return $"FND-{timestamp}-{guid}";
    }

    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && Status != FindingStatus.Closed;
    public bool CanEdit => Status != FindingStatus.Closed;
    public bool CanClose => Status == FindingStatus.Verified;
    public bool HasCorrectiveAction => !string.IsNullOrEmpty(CorrectiveAction);
}