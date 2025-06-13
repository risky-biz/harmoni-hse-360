using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities.Security;

/// <summary>
/// Represents a threat assessment for a security incident
/// </summary>
public class ThreatAssessment : BaseEntity, IAuditableEntity
{
    public int SecurityIncidentId { get; private set; }
    public SecurityIncident SecurityIncident { get; private set; } = null!;
    
    public ThreatLevel CurrentThreatLevel { get; private set; }
    public ThreatLevel PreviousThreatLevel { get; private set; }
    public string AssessmentRationale { get; private set; } = string.Empty;
    public DateTime AssessmentDateTime { get; private set; }
    
    // Threat Intelligence
    public bool ExternalThreatIntelUsed { get; private set; }
    public string? ThreatIntelSource { get; private set; }
    public string? ThreatIntelDetails { get; private set; }
    
    // Risk Scoring (1-5 scale)
    public int ThreatCapability { get; private set; }
    public int ThreatIntent { get; private set; }
    public int TargetVulnerability { get; private set; }
    public int ImpactPotential { get; private set; }
    
    // Calculated Risk Score
    public int RiskScore => CalculateRiskScore();
    
    // Navigation Properties
    public int AssessedById { get; private set; }
    public User AssessedBy { get; private set; } = null!;
    
    // Audit Properties
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }
    
    // Constructor for EF Core
    protected ThreatAssessment() { }
    
    // Factory method
    public static ThreatAssessment Create(
        int securityIncidentId,
        ThreatLevel currentLevel,
        ThreatLevel previousLevel,
        string rationale,
        int assessedById,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(rationale))
            throw new ArgumentException("Assessment rationale is required", nameof(rationale));
        
        return new ThreatAssessment
        {
            SecurityIncidentId = securityIncidentId,
            CurrentThreatLevel = currentLevel,
            PreviousThreatLevel = previousLevel,
            AssessmentRationale = rationale,
            AssessmentDateTime = DateTime.UtcNow,
            AssessedById = assessedById,
            ExternalThreatIntelUsed = false,
            ThreatCapability = 1,
            ThreatIntent = 1,
            TargetVulnerability = 1,
            ImpactPotential = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }
    
    // Business Methods
    public void UpdateRiskFactors(
        int capability,
        int intent,
        int vulnerability,
        int impact,
        string modifiedBy)
    {
        ValidateRiskFactor(capability, nameof(capability));
        ValidateRiskFactor(intent, nameof(intent));
        ValidateRiskFactor(vulnerability, nameof(vulnerability));
        ValidateRiskFactor(impact, nameof(impact));
        
        ThreatCapability = capability;
        ThreatIntent = intent;
        TargetVulnerability = vulnerability;
        ImpactPotential = impact;
        
        // Update threat level based on new risk score
        var newLevel = DetermineThreatLevel();
        if (newLevel != CurrentThreatLevel)
        {
            PreviousThreatLevel = CurrentThreatLevel;
            CurrentThreatLevel = newLevel;
        }
        
        UpdateAudit(modifiedBy);
    }
    
    public void AddThreatIntelligence(
        string source,
        string details,
        string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Threat intelligence source is required", nameof(source));
        
        if (string.IsNullOrWhiteSpace(details))
            throw new ArgumentException("Threat intelligence details are required", nameof(details));
        
        ExternalThreatIntelUsed = true;
        ThreatIntelSource = source;
        ThreatIntelDetails = details;
        UpdateAudit(modifiedBy);
    }
    
    // Private Methods
    private void ValidateRiskFactor(int value, string paramName)
    {
        if (value < 1 || value > 5)
            throw new ArgumentOutOfRangeException(paramName, "Risk factor must be between 1 and 5");
    }
    
    private int CalculateRiskScore()
    {
        // Risk Score = (Capability × Intent) + (Vulnerability × Impact)
        // This gives a score between 2 and 50
        return (ThreatCapability * ThreatIntent) + (TargetVulnerability * ImpactPotential);
    }
    
    private ThreatLevel DetermineThreatLevel()
    {
        var score = CalculateRiskScore();
        
        return score switch
        {
            <= 10 => ThreatLevel.Minimal,
            <= 20 => ThreatLevel.Low,
            <= 30 => ThreatLevel.Medium,
            <= 40 => ThreatLevel.High,
            _ => ThreatLevel.Severe
        };
    }
    
    private void UpdateAudit(string modifiedBy)
    {
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }
}