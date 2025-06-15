using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities.Audits;

public class AuditItem : BaseEntity, IAuditableEntity
{
    public int AuditId { get; private set; }
    public string ItemNumber { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public AuditItemType Type { get; private set; }
    public AuditItemStatus Status { get; private set; }
    public string? Category { get; private set; }
    public bool IsRequired { get; private set; }
    public int SortOrder { get; private set; }
    
    // Assessment Data
    public string? ExpectedResult { get; private set; }
    public string? ActualResult { get; private set; }
    public string? Comments { get; private set; }
    public bool? IsCompliant { get; private set; }
    
    // Scoring
    public int? MaxPoints { get; private set; }
    public int? ActualPoints { get; private set; }
    
    // Assessment Details
    public string? AssessedBy { get; private set; }
    public DateTime? AssessedAt { get; private set; }
    public string? Evidence { get; private set; }
    public string? CorrectiveAction { get; private set; }
    public DateTime? DueDate { get; private set; }
    public int? ResponsiblePersonId { get; private set; }
    
    // Validation Rules
    public string? ValidationCriteria { get; private set; }
    public string? AcceptanceCriteria { get; private set; }
    
    // Navigation Properties
    public virtual Audit Audit { get; private set; } = null!;
    public virtual User? ResponsiblePerson { get; private set; }
    
    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    private AuditItem() { }

    public static AuditItem Create(
        int auditId,
        string description,
        AuditItemType type,
        bool isRequired = true,
        string? category = null,
        int sortOrder = 0,
        string? expectedResult = null,
        int? maxPoints = null)
    {
        var item = new AuditItem
        {
            AuditId = auditId,
            Description = description,
            Type = type,
            Status = AuditItemStatus.NotStarted,
            IsRequired = isRequired,
            Category = category,
            SortOrder = sortOrder,
            ExpectedResult = expectedResult,
            MaxPoints = maxPoints,
            CreatedAt = DateTime.UtcNow
        };

        item.ItemNumber = GenerateItemNumber(auditId, sortOrder);
        
        return item;
    }

    public void UpdateDescription(string description, string? expectedResult = null)
    {
        Description = description;
        ExpectedResult = expectedResult;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetValidationCriteria(string validationCriteria, string? acceptanceCriteria = null)
    {
        ValidationCriteria = validationCriteria;
        AcceptanceCriteria = acceptanceCriteria;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void StartAssessment()
    {
        if (Status != AuditItemStatus.NotStarted)
            throw new InvalidOperationException("Only not started items can be started");

        Status = AuditItemStatus.InProgress;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void CompleteAssessment(
        string actualResult,
        bool isCompliant,
        string assessedBy,
        int? actualPoints = null,
        string? comments = null,
        string? evidence = null)
    {
        if (Status != AuditItemStatus.InProgress && Status != AuditItemStatus.NotStarted)
            throw new InvalidOperationException("Only in-progress or not started items can be completed");

        Status = isCompliant ? AuditItemStatus.Completed : AuditItemStatus.NonCompliant;
        ActualResult = actualResult;
        IsCompliant = isCompliant;
        AssessedBy = assessedBy;
        AssessedAt = DateTime.UtcNow;
        ActualPoints = actualPoints;
        Comments = comments;
        Evidence = evidence;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void MarkAsNotApplicable(string reason, string assessedBy)
    {
        Status = AuditItemStatus.NotApplicable;
        ActualResult = $"Not Applicable: {reason}";
        AssessedBy = assessedBy;
        AssessedAt = DateTime.UtcNow;
        IsCompliant = null;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddCorrectiveAction(
        string correctiveAction,
        DateTime? dueDate = null,
        int? responsiblePersonId = null)
    {
        if (Status != AuditItemStatus.NonCompliant)
            throw new InvalidOperationException("Corrective actions can only be added to non-compliant items");

        CorrectiveAction = correctiveAction;
        DueDate = dueDate;
        ResponsiblePersonId = responsiblePersonId;
        
        if (!string.IsNullOrEmpty(correctiveAction))
        {
            Status = AuditItemStatus.RequiresFollowUp;
        }
        
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateScore(int maxPoints, int? actualPoints = null)
    {
        MaxPoints = maxPoints;
        ActualPoints = actualPoints;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Reset()
    {
        Status = AuditItemStatus.NotStarted;
        ActualResult = null;
        IsCompliant = null;
        AssessedBy = null;
        AssessedAt = null;
        ActualPoints = null;
        Comments = null;
        Evidence = null;
        CorrectiveAction = null;
        DueDate = null;
        ResponsiblePersonId = null;
        LastModifiedAt = DateTime.UtcNow;
    }

    private static string GenerateItemNumber(int auditId, int sortOrder)
    {
        return $"AI-{auditId:D6}-{sortOrder:D3}";
    }

    // Computed Properties
    public bool RequiresFollowUp => Status == AuditItemStatus.RequiresFollowUp || 
                                   (!string.IsNullOrEmpty(CorrectiveAction) && Status == AuditItemStatus.NonCompliant);
    
    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && 
                           Status != AuditItemStatus.Completed;
    
    public double? ScorePercentage => MaxPoints.HasValue && MaxPoints.Value > 0 && ActualPoints.HasValue
        ? Math.Round((double)ActualPoints.Value / MaxPoints.Value * 100, 2)
        : null;
}