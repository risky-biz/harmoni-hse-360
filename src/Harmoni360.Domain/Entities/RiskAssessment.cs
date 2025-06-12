using Harmoni360.Domain.Common;
using Harmoni360.Domain.Events;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities;

public class RiskAssessment : BaseEntity, IAuditableEntity
{
    public int HazardId { get; private set; }
    public Hazard Hazard { get; private set; } = null!;
    public RiskAssessmentType Type { get; private set; }
    public int AssessorId { get; private set; }
    public User Assessor { get; private set; } = null!;
    public DateTime AssessmentDate { get; private set; }
    
    // Risk scoring
    public int ProbabilityScore { get; private set; } // 1-5
    public int SeverityScore { get; private set; } // 1-5
    public int RiskScore { get; private set; } // Calculated: Probability * Severity
    public RiskAssessmentLevel RiskLevel { get; private set; } // Low, Medium, High, Critical
    
    // Assessment details
    public string PotentialConsequences { get; private set; } = string.Empty;
    public string ExistingControls { get; private set; } = string.Empty;
    public string RecommendedActions { get; private set; } = string.Empty;
    public string AdditionalNotes { get; private set; } = string.Empty;
    
    // Review cycle
    public DateTime NextReviewDate { get; private set; }
    public bool IsActive { get; private set; }
    
    // Approval workflow
    public bool IsApproved { get; private set; }
    public int? ApprovedById { get; private set; }
    public User? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? ApprovalNotes { get; private set; }

    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    protected RiskAssessment() { } // For EF Core

    public static RiskAssessment Create(
        int hazardId,
        RiskAssessmentType type,
        int assessorId,
        int probabilityScore,
        int severityScore,
        string potentialConsequences,
        string existingControls,
        string recommendedActions,
        string additionalNotes = "")
    {
        ValidateScores(probabilityScore, severityScore);

        var riskScore = probabilityScore * severityScore;
        var riskLevel = CalculateRiskLevel(riskScore);

        var assessment = new RiskAssessment
        {
            HazardId = hazardId,
            Type = type,
            AssessorId = assessorId,
            AssessmentDate = DateTime.UtcNow,
            ProbabilityScore = probabilityScore,
            SeverityScore = severityScore,
            RiskScore = riskScore,
            RiskLevel = riskLevel,
            PotentialConsequences = potentialConsequences,
            ExistingControls = existingControls,
            RecommendedActions = recommendedActions,
            AdditionalNotes = additionalNotes,
            NextReviewDate = CalculateNextReviewDate(riskLevel),
            IsActive = true,
            IsApproved = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = $"User_{assessorId}"
        };

        assessment.AddDomainEvent(new RiskAssessmentCreatedEvent(assessment));

        if (assessment.RiskLevel >= RiskAssessmentLevel.High)
        {
            assessment.AddDomainEvent(new HighRiskAssessmentCreatedEvent(assessment));
        }

        return assessment;
    }

    public void UpdateAssessment(
        int probabilityScore,
        int severityScore,
        string potentialConsequences,
        string existingControls,
        string recommendedActions,
        string additionalNotes,
        string modifiedBy)
    {
        ValidateScores(probabilityScore, severityScore);

        var previousRiskLevel = RiskLevel;
        
        ProbabilityScore = probabilityScore;
        SeverityScore = severityScore;
        RiskScore = probabilityScore * severityScore;
        RiskLevel = CalculateRiskLevel(RiskScore);
        PotentialConsequences = potentialConsequences;
        ExistingControls = existingControls;
        RecommendedActions = recommendedActions;
        AdditionalNotes = additionalNotes;
        NextReviewDate = CalculateNextReviewDate(RiskLevel);
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        // Reset approval if assessment changed significantly
        if (IsApproved && (RiskLevel != previousRiskLevel || RiskScore != (probabilityScore * severityScore)))
        {
            IsApproved = false;
            ApprovedById = null;
            ApprovedBy = null;
            ApprovedAt = null;
            ApprovalNotes = null;
        }

        AddDomainEvent(new RiskAssessmentUpdatedEvent(this, previousRiskLevel, RiskLevel));
    }

    public void Approve(int approvedById, string approvalNotes = "")
    {
        if (IsApproved)
        {
            throw new InvalidOperationException("Risk assessment is already approved");
        }

        IsApproved = true;
        ApprovedById = approvedById;
        ApprovedAt = DateTime.UtcNow;
        ApprovalNotes = approvalNotes;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = $"User_{approvedById}";

        AddDomainEvent(new RiskAssessmentApprovedEvent(this));
    }

    public void Deactivate(string reason)
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
        
        AddDomainEvent(new RiskAssessmentDeactivatedEvent(this, reason));
    }

    public void ScheduleReview(DateTime nextReviewDate)
    {
        NextReviewDate = nextReviewDate;
        LastModifiedAt = DateTime.UtcNow;
    }

    public bool IsOverdueForReview()
    {
        return IsActive && DateTime.UtcNow > NextReviewDate;
    }

    private static void ValidateScores(int probabilityScore, int severityScore)
    {
        if (probabilityScore < 1 || probabilityScore > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(probabilityScore), "Probability score must be between 1 and 5");
        }

        if (severityScore < 1 || severityScore > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(severityScore), "Severity score must be between 1 and 5");
        }
    }

    private static RiskAssessmentLevel CalculateRiskLevel(int riskScore)
    {
        return riskScore switch
        {
            >= 1 and <= 4 => RiskAssessmentLevel.VeryLow,
            >= 5 and <= 9 => RiskAssessmentLevel.Low,
            >= 10 and <= 14 => RiskAssessmentLevel.Medium,
            >= 15 and <= 19 => RiskAssessmentLevel.High,
            >= 20 and <= 25 => RiskAssessmentLevel.Critical,
            _ => RiskAssessmentLevel.Low
        };
    }

    private static DateTime CalculateNextReviewDate(RiskAssessmentLevel riskLevel)
    {
        var months = riskLevel switch
        {
            RiskAssessmentLevel.Critical => 1,    // Monthly for critical risks
            RiskAssessmentLevel.High => 3,        // Quarterly for high risks
            RiskAssessmentLevel.Medium => 6,      // Semi-annually for medium risks
            RiskAssessmentLevel.Low => 12,        // Annually for low risks
            RiskAssessmentLevel.VeryLow => 24,    // Every 2 years for very low risks
            _ => 12
        };

        return DateTime.UtcNow.AddMonths(months);
    }
}

public enum RiskAssessmentLevel
{
    VeryLow = 1,    // Score 1-4
    Low = 2,        // Score 5-9
    Medium = 3,     // Score 10-14
    High = 4,       // Score 15-19
    Critical = 5    // Score 20-25
}

public enum RiskAssessmentType
{
    General = 1,
    JSA = 2,        // Job Safety Analysis
    HIRA = 3,       // Hazard Identification and Risk Assessment
    Environmental = 4,
    Fire = 5
}