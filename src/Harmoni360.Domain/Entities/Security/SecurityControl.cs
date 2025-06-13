using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities.Security;

/// <summary>
/// Represents a security control implemented to mitigate risks
/// </summary>
public class SecurityControl : BaseEntity, IAuditableEntity
{
    public string ControlName { get; private set; } = string.Empty;
    public string ControlDescription { get; private set; } = string.Empty;
    public SecurityControlType ControlType { get; private set; }
    public SecurityControlCategory Category { get; private set; }
    public ControlImplementationStatus Status { get; private set; }
    
    // Implementation Details
    public DateTime ImplementationDate { get; private set; }
    public DateTime? ReviewDate { get; private set; }
    public DateTime? NextReviewDate { get; private set; }
    
    // Effectiveness Metrics
    public int? EffectivenessScore { get; private set; } // 1-10 scale
    public string? EffectivenessNotes { get; private set; }
    
    // Cost Information
    public decimal? ImplementationCost { get; private set; }
    public decimal? AnnualMaintenanceCost { get; private set; }
    
    // Relationships
    public int? RelatedIncidentId { get; private set; }
    public SecurityIncident? RelatedIncident { get; private set; }
    
    public int ImplementedById { get; private set; }
    public User ImplementedBy { get; private set; } = null!;
    
    public int? ReviewedById { get; private set; }
    public User? ReviewedBy { get; private set; }
    
    // Collections
    private readonly List<SecurityIncident> _mitigatedIncidents = new();
    public IReadOnlyCollection<SecurityIncident> MitigatedIncidents => _mitigatedIncidents.AsReadOnly();
    
    // Audit Properties
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }
    
    // Constructor for EF Core
    protected SecurityControl() { }
    
    // Factory method
    public static SecurityControl Create(
        string name,
        string description,
        SecurityControlType controlType,
        SecurityControlCategory category,
        int implementedById,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Control name is required", nameof(name));
        
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Control description is required", nameof(description));
        
        return new SecurityControl
        {
            ControlName = name,
            ControlDescription = description,
            ControlType = controlType,
            Category = category,
            Status = ControlImplementationStatus.Planned,
            ImplementationDate = DateTime.UtcNow,
            ImplementedById = implementedById,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }
    
    // Factory method for incident-related control
    public static SecurityControl CreateForIncident(
        string name,
        string description,
        SecurityControlType controlType,
        SecurityControlCategory category,
        int relatedIncidentId,
        int implementedById,
        string createdBy)
    {
        var control = Create(name, description, controlType, category, implementedById, createdBy);
        control.RelatedIncidentId = relatedIncidentId;
        return control;
    }
    
    // Business Methods
    public void StartImplementation(string modifiedBy)
    {
        if (Status != ControlImplementationStatus.Planned)
            throw new InvalidOperationException("Can only start implementation of planned controls");
        
        Status = ControlImplementationStatus.Implementing;
        UpdateAudit(modifiedBy);
    }
    
    public void CompleteImplementation(string modifiedBy)
    {
        if (Status != ControlImplementationStatus.Implementing)
            throw new InvalidOperationException("Can only complete implementation of controls that are being implemented");
        
        Status = ControlImplementationStatus.Active;
        ImplementationDate = DateTime.UtcNow;
        ScheduleNextReview();
        UpdateAudit(modifiedBy);
    }
    
    public void ScheduleReview(DateTime reviewDate, string modifiedBy)
    {
        if (reviewDate <= DateTime.UtcNow)
            throw new ArgumentException("Review date must be in the future", nameof(reviewDate));
        
        NextReviewDate = reviewDate;
        UpdateAudit(modifiedBy);
    }
    
    public void ConductReview(
        int reviewedById,
        int effectivenessScore,
        string effectivenessNotes,
        string modifiedBy)
    {
        if (effectivenessScore < 1 || effectivenessScore > 10)
            throw new ArgumentOutOfRangeException(nameof(effectivenessScore), "Effectiveness score must be between 1 and 10");
        
        ReviewedById = reviewedById;
        ReviewDate = DateTime.UtcNow;
        EffectivenessScore = effectivenessScore;
        EffectivenessNotes = effectivenessNotes;
        Status = ControlImplementationStatus.UnderReview;
        
        // Schedule next review based on effectiveness
        ScheduleNextReviewBasedOnEffectiveness(effectivenessScore);
        UpdateAudit(modifiedBy);
    }
    
    public void UpdateCostInformation(
        decimal? implementationCost,
        decimal? annualMaintenanceCost,
        string modifiedBy)
    {
        ImplementationCost = implementationCost;
        AnnualMaintenanceCost = annualMaintenanceCost;
        UpdateAudit(modifiedBy);
    }
    
    public void RetireControl(string reason, string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason for retirement is required", nameof(reason));
        
        Status = ControlImplementationStatus.Retired;
        EffectivenessNotes = $"Retired: {reason}";
        UpdateAudit(modifiedBy);
    }
    
    public void LinkToIncident(SecurityIncident incident)
    {
        if (incident == null)
            throw new ArgumentNullException(nameof(incident));
        
        _mitigatedIncidents.Add(incident);
    }
    
    // Calculated Properties
    public bool IsOverdue => NextReviewDate.HasValue && NextReviewDate.Value < DateTime.UtcNow;
    
    public int DaysUntilReview => NextReviewDate.HasValue 
        ? Math.Max(0, (NextReviewDate.Value.Date - DateTime.UtcNow.Date).Days)
        : 0;
    
    public bool IsEffective => EffectivenessScore.HasValue && EffectivenessScore.Value >= 7;
    
    // Private Methods
    private void ScheduleNextReview()
    {
        // Default review cycle based on control type
        var reviewInterval = ControlType switch
        {
            SecurityControlType.Preventive => TimeSpan.FromDays(365), // Annual
            SecurityControlType.Detective => TimeSpan.FromDays(180),  // Semi-annual
            SecurityControlType.Corrective => TimeSpan.FromDays(90),  // Quarterly
            SecurityControlType.Deterrent => TimeSpan.FromDays(365),  // Annual
            SecurityControlType.Compensating => TimeSpan.FromDays(90), // Quarterly
            _ => TimeSpan.FromDays(365)
        };
        
        NextReviewDate = DateTime.UtcNow.Add(reviewInterval);
    }
    
    private void ScheduleNextReviewBasedOnEffectiveness(int effectivenessScore)
    {
        // More effective controls can have longer review intervals
        var baseInterval = ControlType switch
        {
            SecurityControlType.Detective => TimeSpan.FromDays(180),
            SecurityControlType.Corrective => TimeSpan.FromDays(90),
            SecurityControlType.Compensating => TimeSpan.FromDays(90),
            _ => TimeSpan.FromDays(365)
        };
        
        // Adjust based on effectiveness
        var multiplier = effectivenessScore switch
        {
            >= 9 => 1.5,  // Extend by 50% for highly effective controls
            >= 7 => 1.0,  // Standard interval for effective controls
            >= 5 => 0.75, // Reduce by 25% for moderately effective controls
            _ => 0.5      // Reduce by 50% for less effective controls
        };
        
        var adjustedInterval = TimeSpan.FromDays(baseInterval.TotalDays * multiplier);
        NextReviewDate = DateTime.UtcNow.Add(adjustedInterval);
    }
    
    private void UpdateAudit(string modifiedBy)
    {
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }
}