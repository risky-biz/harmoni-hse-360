using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;
using Harmoni360.Domain.Events;
using Harmoni360.Domain.ValueObjects;

namespace Harmoni360.Domain.Entities.Audits;

public class Audit : BaseEntity, IAuditableEntity
{
    private readonly List<AuditItem> _items = new();
    private readonly List<AuditAttachment> _attachments = new();
    private readonly List<AuditFinding> _findings = new();
    private readonly List<AuditComment> _comments = new();

    public string AuditNumber { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public AuditType Type { get; private set; }
    public AuditCategory Category { get; private set; }
    public AuditStatus Status { get; private set; }
    public AuditPriority Priority { get; private set; }
    
    // Schedule & Execution
    public DateTime ScheduledDate { get; private set; }
    public DateTime? StartedDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public int AuditorId { get; private set; }
    public int? LocationId { get; private set; }
    public int? DepartmentId { get; private set; }
    public int? FacilityId { get; private set; }
    
    // Assessment Results
    public RiskLevel RiskLevel { get; private set; }
    public string? Summary { get; private set; }
    public string? Recommendations { get; private set; }
    public AuditScore? OverallScore { get; private set; }
    public int? EstimatedDurationMinutes { get; private set; }
    public int? ActualDurationMinutes { get; private set; }
    
    // Compliance & Standards
    public string? StandardsApplied { get; private set; }
    public bool IsRegulatory { get; private set; }
    public string? RegulatoryReference { get; private set; }
    
    // Scoring and Performance
    public decimal? ScorePercentage { get; private set; }
    public int? TotalPossiblePoints { get; private set; }
    public int? AchievedPoints { get; private set; }
    
    // Navigation Properties
    public virtual User Auditor { get; private set; } = null!;
    public virtual Department? Department { get; private set; }
    public virtual IReadOnlyCollection<AuditItem> Items => _items.AsReadOnly();
    public virtual IReadOnlyCollection<AuditAttachment> Attachments => _attachments.AsReadOnly();
    public virtual IReadOnlyCollection<AuditFinding> Findings => _findings.AsReadOnly();
    public virtual IReadOnlyCollection<AuditComment> Comments => _comments.AsReadOnly();
    
    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    private Audit() { }

    public static Audit Create(
        string title,
        string description,
        AuditType type,
        AuditCategory category,
        AuditPriority priority,
        DateTime scheduledDate,
        int auditorId,
        int? locationId = null,
        int? departmentId = null,
        int? facilityId = null,
        int? estimatedDurationMinutes = null)
    {
        var audit = new Audit
        {
            Title = title,
            Description = description,
            Type = type,
            Category = category,
            Priority = priority,
            Status = AuditStatus.Draft,
            ScheduledDate = scheduledDate,
            AuditorId = auditorId,
            LocationId = locationId,
            DepartmentId = departmentId,
            FacilityId = facilityId,
            RiskLevel = RiskLevel.Low,
            EstimatedDurationMinutes = estimatedDurationMinutes,
            CreatedAt = DateTime.UtcNow
        };

        audit.AuditNumber = GenerateAuditNumber(type);
        
        audit.AddDomainEvent(new AuditCreatedEvent(audit.Id, audit.Title, audit.Type));
        
        return audit;
    }

    public void UpdateBasicInfo(
        string title,
        string description,
        AuditPriority priority,
        DateTime scheduledDate,
        int? locationId = null,
        int? departmentId = null,
        int? facilityId = null,
        int? estimatedDurationMinutes = null)
    {
        if (Status == AuditStatus.Completed || Status == AuditStatus.Archived)
            throw new InvalidOperationException("Cannot update completed or archived audit");

        Title = title;
        Description = description;
        Priority = priority;
        ScheduledDate = scheduledDate;
        LocationId = locationId;
        DepartmentId = departmentId;
        FacilityId = facilityId;
        EstimatedDurationMinutes = estimatedDurationMinutes;
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new AuditUpdatedEvent(Id, Title));
    }

    public void SetComplianceInfo(
        string? standardsApplied,
        bool isRegulatory = false,
        string? regulatoryReference = null)
    {
        StandardsApplied = standardsApplied;
        IsRegulatory = isRegulatory;
        RegulatoryReference = regulatoryReference;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Schedule(DateTime scheduledDate)
    {
        if (Status != AuditStatus.Draft)
            throw new InvalidOperationException("Only draft audits can be scheduled");

        Status = AuditStatus.Scheduled;
        ScheduledDate = scheduledDate;
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new AuditScheduledEvent(Id, Title, ScheduledDate));
    }

    public void StartAudit()
    {
        if (Status != AuditStatus.Scheduled)
            throw new InvalidOperationException("Only scheduled audits can be started");

        Status = AuditStatus.InProgress;
        StartedDate = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new AuditStartedEvent(Id, Title));
    }

    public void CompleteAudit(string? summary = null, string? recommendations = null)
    {
        if (Status != AuditStatus.InProgress)
            throw new InvalidOperationException("Only in-progress audits can be completed");

        Status = AuditStatus.Completed;
        CompletedDate = DateTime.UtcNow;
        Summary = summary;
        Recommendations = recommendations;
        LastModifiedAt = DateTime.UtcNow;

        if (StartedDate.HasValue)
        {
            ActualDurationMinutes = (int)(CompletedDate.Value - StartedDate.Value).TotalMinutes;
        }

        // Calculate final score
        CalculateOverallScore();

        AddDomainEvent(new AuditCompletedEvent(Id, Title, CompletedDate.Value, OverallScore));
    }

    public void Cancel(string reason)
    {
        if (Status == AuditStatus.Completed || Status == AuditStatus.Archived)
            throw new InvalidOperationException("Cannot cancel completed or archived audit");

        Status = AuditStatus.Cancelled;
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new AuditCancelledEvent(Id, Title, reason));
    }

    public void Archive()
    {
        if (Status != AuditStatus.Completed && Status != AuditStatus.Cancelled)
            throw new InvalidOperationException("Only completed or cancelled audits can be archived");

        Status = AuditStatus.Archived;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SubmitForReview()
    {
        if (Status != AuditStatus.InProgress)
            throw new InvalidOperationException("Only in-progress audits can be submitted for review");

        Status = AuditStatus.UnderReview;
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new AuditSubmittedForReviewEvent(Id, Title));
    }

    public void AddItem(AuditItem item)
    {
        if (Status == AuditStatus.Completed || Status == AuditStatus.Archived)
            throw new InvalidOperationException("Cannot add items to completed or archived audit");

        _items.Add(item);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void RemoveItem(AuditItem item)
    {
        if (Status == AuditStatus.Completed || Status == AuditStatus.Archived)
            throw new InvalidOperationException("Cannot remove items from completed or archived audit");

        _items.Remove(item);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddFinding(AuditFinding finding)
    {
        _findings.Add(finding);
        
        // Update risk level based on findings
        UpdateRiskLevel();
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new AuditFindingAddedEvent(Id, finding.Id, finding.Type, finding.Severity));
    }

    public void AddAttachment(AuditAttachment attachment)
    {
        _attachments.Add(attachment);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void RemoveAttachment(AuditAttachment attachment)
    {
        _attachments.Remove(attachment);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddComment(AuditComment comment)
    {
        _comments.Add(comment);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void MarkOverdue()
    {
        if (Status == AuditStatus.Scheduled && ScheduledDate < DateTime.UtcNow)
        {
            Status = AuditStatus.Overdue;
            LastModifiedAt = DateTime.UtcNow;

            AddDomainEvent(new AuditOverdueEvent(Id, Title, ScheduledDate));
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
        var criticalCount = _findings.Count(f => f.Severity == FindingSeverity.Critical);
        var majorCount = _findings.Count(f => f.Severity == FindingSeverity.Major);

        RiskLevel = (criticalCount, majorCount, maxSeverity) switch
        {
            (> 0, _, _) => RiskLevel.Critical,
            (0, >= 3, _) => RiskLevel.Critical,
            (0, >= 1, FindingSeverity.Major) => RiskLevel.High,
            (0, 0, FindingSeverity.Moderate) => RiskLevel.Medium,
            _ => RiskLevel.Low
        };
    }

    private void CalculateOverallScore()
    {
        var completedItems = _items.Where(i => i.Status == AuditItemStatus.Completed).ToList();
        if (!completedItems.Any())
        {
            OverallScore = null;
            ScorePercentage = null;
            return;
        }

        TotalPossiblePoints = completedItems.Sum(i => i.MaxPoints ?? 1);
        AchievedPoints = completedItems.Sum(i => i.ActualPoints ?? 0);

        if (TotalPossiblePoints > 0)
        {
            ScorePercentage = Math.Round((decimal)AchievedPoints.Value / TotalPossiblePoints.Value * 100, 2);
            
            OverallScore = ScorePercentage switch
            {
                >= 90 => AuditScore.Excellent,
                >= 80 => AuditScore.Good,
                >= 70 => AuditScore.Satisfactory,
                >= 60 => AuditScore.NeedsImprovement,
                _ => AuditScore.Unsatisfactory
            };
        }
    }

    private static string GenerateAuditNumber(AuditType type)
    {
        var prefix = type switch
        {
            AuditType.Safety => "SA",
            AuditType.Environmental => "EA",
            AuditType.Equipment => "EQ",
            AuditType.Compliance => "CA",
            AuditType.Fire => "FA",
            AuditType.Chemical => "CH",
            AuditType.Ergonomic => "ER",
            AuditType.Emergency => "EM",
            AuditType.Management => "MA",
            AuditType.Process => "PA",
            _ => "AU"
        };

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var guid = Guid.NewGuid().ToString("N")[..6].ToUpper();
        return $"{prefix}-{timestamp}-{guid}";
    }

    // Computed Properties
    public bool IsOverdue => Status == AuditStatus.Scheduled && ScheduledDate < DateTime.UtcNow;
    public bool CanEdit => Status == AuditStatus.Draft || Status == AuditStatus.Scheduled;
    public bool CanStart => Status == AuditStatus.Scheduled;
    public bool CanComplete => Status == AuditStatus.InProgress;
    public bool CanCancel => Status != AuditStatus.Completed && Status != AuditStatus.Archived;
    public bool CanArchive => Status == AuditStatus.Completed || Status == AuditStatus.Cancelled;
    public bool HasFindings => _findings.Any();
    public bool HasCriticalFindings => _findings.Any(f => f.Severity == FindingSeverity.Critical);
    public int CompletionPercentage => CalculateCompletionPercentage();
    
    private int CalculateCompletionPercentage()
    {
        if (!_items.Any()) return 0;
        var completedItems = _items.Count(i => i.Status == AuditItemStatus.Completed || i.Status == AuditItemStatus.NotApplicable);
        return (int)Math.Round((double)completedItems / _items.Count * 100);
    }
}