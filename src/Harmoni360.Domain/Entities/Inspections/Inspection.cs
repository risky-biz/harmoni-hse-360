using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;
using Harmoni360.Domain.Events;

namespace Harmoni360.Domain.Entities.Inspections;

public class Inspection : BaseEntity, IAuditableEntity
{
    private readonly List<InspectionItem> _items = new();
    private readonly List<InspectionAttachment> _attachments = new();
    private readonly List<InspectionFinding> _findings = new();
    private readonly List<InspectionComment> _comments = new();

    public string InspectionNumber { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public InspectionType Type { get; private set; }
    public InspectionCategory Category { get; private set; }
    public InspectionStatus Status { get; private set; }
    public InspectionPriority Priority { get; private set; }
    public DateTime ScheduledDate { get; private set; }
    public DateTime? StartedDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public int InspectorId { get; private set; }
    public int? LocationId { get; private set; }
    public int? DepartmentId { get; private set; }
    public int? FacilityId { get; private set; }
    public RiskLevel RiskLevel { get; private set; }
    public string? Summary { get; private set; }
    public string? Recommendations { get; private set; }
    public int? EstimatedDurationMinutes { get; private set; }
    public int? ActualDurationMinutes { get; private set; }

    // Navigation Properties
    public virtual User Inspector { get; private set; } = null!;
    public virtual Department? Department { get; private set; }
    public virtual IReadOnlyCollection<InspectionItem> Items => _items.AsReadOnly();
    public virtual IReadOnlyCollection<InspectionAttachment> Attachments => _attachments.AsReadOnly();
    public virtual IReadOnlyCollection<InspectionFinding> Findings => _findings.AsReadOnly();
    public virtual IReadOnlyCollection<InspectionComment> Comments => _comments.AsReadOnly();

    // IAuditableEntity
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    private Inspection() { }

    public static Inspection Create(
        string title,
        string description,
        InspectionType type,
        InspectionCategory category,
        InspectionPriority priority,
        DateTime scheduledDate,
        int inspectorId,
        int? locationId = null,
        int? departmentId = null,
        int? facilityId = null)
    {
        var inspection = new Inspection
        {
            Title = title,
            Description = description,
            Type = type,
            Category = category,
            Priority = priority,
            Status = InspectionStatus.Draft,
            ScheduledDate = scheduledDate,
            InspectorId = inspectorId,
            LocationId = locationId,
            DepartmentId = departmentId,
            FacilityId = facilityId,
            RiskLevel = RiskLevel.Low,
            CreatedAt = DateTime.UtcNow
        };

        inspection.InspectionNumber = GenerateInspectionNumber();
        
        inspection.AddDomainEvent(new InspectionCreatedEvent(inspection.Id, inspection.Title, inspection.Type));
        
        return inspection;
    }

    public void UpdateBasicInfo(
        string title,
        string description,
        InspectionPriority priority,
        DateTime scheduledDate,
        int? locationId = null,
        int? departmentId = null,
        int? facilityId = null)
    {
        if (Status == InspectionStatus.Completed || Status == InspectionStatus.Archived)
            throw new InvalidOperationException("Cannot update completed or archived inspection");

        Title = title;
        Description = description;
        Priority = priority;
        ScheduledDate = scheduledDate;
        LocationId = locationId;
        DepartmentId = departmentId;
        FacilityId = facilityId;
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new InspectionUpdatedEvent(Id, Title));
    }

    public void Schedule(DateTime scheduledDate)
    {
        if (Status != InspectionStatus.Draft)
            throw new InvalidOperationException("Only draft inspections can be scheduled");

        Status = InspectionStatus.Scheduled;
        ScheduledDate = scheduledDate;
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new InspectionScheduledEvent(Id, Title, ScheduledDate));
    }

    public void StartInspection()
    {
        if (Status != InspectionStatus.Scheduled)
            throw new InvalidOperationException("Only scheduled inspections can be started");

        Status = InspectionStatus.InProgress;
        StartedDate = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new InspectionStartedEvent(Id, Title));
    }

    public void CompleteInspection(string? summary = null, string? recommendations = null)
    {
        if (Status != InspectionStatus.InProgress)
            throw new InvalidOperationException("Only in-progress inspections can be completed");

        Status = InspectionStatus.Completed;
        CompletedDate = DateTime.UtcNow;
        Summary = summary;
        Recommendations = recommendations;
        LastModifiedAt = DateTime.UtcNow;

        if (StartedDate.HasValue)
        {
            ActualDurationMinutes = (int)(CompletedDate.Value - StartedDate.Value).TotalMinutes;
        }

        AddDomainEvent(new InspectionCompletedEvent(Id, Title, CompletedDate.Value));
    }

    public void Cancel(string reason)
    {
        if (Status == InspectionStatus.Completed || Status == InspectionStatus.Archived)
            throw new InvalidOperationException("Cannot cancel completed or archived inspection");

        Status = InspectionStatus.Cancelled;
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new InspectionCancelledEvent(Id, Title, reason));
    }

    public void Archive()
    {
        if (Status != InspectionStatus.Completed && Status != InspectionStatus.Cancelled)
            throw new InvalidOperationException("Only completed or cancelled inspections can be archived");

        Status = InspectionStatus.Archived;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddItem(InspectionItem item)
    {
        if (Status == InspectionStatus.Completed || Status == InspectionStatus.Archived)
            throw new InvalidOperationException("Cannot add items to completed or archived inspection");

        _items.Add(item);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void RemoveItem(InspectionItem item)
    {
        if (Status == InspectionStatus.Completed || Status == InspectionStatus.Archived)
            throw new InvalidOperationException("Cannot remove items from completed or archived inspection");

        _items.Remove(item);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddFinding(InspectionFinding finding)
    {
        _findings.Add(finding);
        
        // Update risk level based on findings
        UpdateRiskLevel();
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new InspectionFindingAddedEvent(Id, finding.Id, finding.Type, finding.Severity));
    }

    public void AddAttachment(InspectionAttachment attachment)
    {
        _attachments.Add(attachment);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void RemoveAttachment(InspectionAttachment attachment)
    {
        _attachments.Remove(attachment);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddComment(InspectionComment comment)
    {
        _comments.Add(comment);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void MarkOverdue()
    {
        if (Status == InspectionStatus.Scheduled && ScheduledDate < DateTime.UtcNow)
        {
            Status = InspectionStatus.Overdue;
            LastModifiedAt = DateTime.UtcNow;

            AddDomainEvent(new InspectionOverdueEvent(Id, Title, ScheduledDate));
        }
    }

    private void UpdateRiskLevel()
    {
        if (!_findings.Any())
        {
            RiskLevel = RiskLevel.Low;
            return;
        }

        var maxSeverity = _findings.Max(f => f.Severity);
        RiskLevel = maxSeverity switch
        {
            FindingSeverity.Critical => RiskLevel.Critical,
            FindingSeverity.Major => RiskLevel.High,
            FindingSeverity.Moderate => RiskLevel.Medium,
            _ => RiskLevel.Low
        };
    }

    private static string GenerateInspectionNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var guid = Guid.NewGuid().ToString("N")[..6].ToUpper(); // Use first 6 chars of GUID for uniqueness
        return $"INS-{timestamp}-{guid}";
    }

    public bool IsOverdue => Status == InspectionStatus.Scheduled && ScheduledDate < DateTime.UtcNow;
    public bool CanEdit => Status == InspectionStatus.Draft || Status == InspectionStatus.Scheduled;
    public bool CanStart => Status == InspectionStatus.Scheduled;
    public bool CanComplete => Status == InspectionStatus.InProgress;
    public bool CanCancel => Status != InspectionStatus.Completed && Status != InspectionStatus.Archived;
}