using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities;

public class WorkPermitHazard : BaseEntity, IAuditableEntity
{
    public int WorkPermitId { get; set; }
    public string HazardDescription { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public HazardCategory? Category { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public int Likelihood { get; set; } // 1-5 scale
    public int Severity { get; set; } // 1-5 scale
    public string ControlMeasures { get; set; } = string.Empty;
    public RiskLevel ResidualRiskLevel { get; set; }
    public string ResponsiblePerson { get; set; } = string.Empty;
    public bool IsControlImplemented { get; set; }
    public DateTime? ControlImplementedDate { get; set; }
    public string ImplementationNotes { get; set; } = string.Empty;

    // Navigation Properties
    public WorkPermit? WorkPermit { get; set; }

    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public static WorkPermitHazard Create(
        int workPermitId,
        string hazardDescription,
        int? categoryId,
        RiskLevel riskLevel,
        int likelihood,
        int severity,
        string controlMeasures,
        RiskLevel residualRiskLevel,
        string responsiblePerson = "")
    {
        return new WorkPermitHazard
        {
            WorkPermitId = workPermitId,
            HazardDescription = hazardDescription,
            CategoryId = categoryId,
            RiskLevel = riskLevel,
            Likelihood = likelihood,
            Severity = severity,
            ControlMeasures = controlMeasures,
            ResidualRiskLevel = residualRiskLevel,
            ResponsiblePerson = responsiblePerson,
            IsControlImplemented = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void ImplementControl(string implementedBy, string notes = "")
    {
        IsControlImplemented = true;
        ControlImplementedDate = DateTime.UtcNow;
        ImplementationNotes = notes;
        LastModifiedBy = implementedBy;
        LastModifiedAt = DateTime.UtcNow;
        
        // Typically residual risk is lower after control implementation
        ResidualRiskLevel = CalculateResidualRisk();
    }

    public void UpdateRiskAssessment(int likelihood, int severity, string controlMeasures)
    {
        Likelihood = likelihood;
        Severity = severity;
        ControlMeasures = controlMeasures;
        RiskLevel = CalculateRiskLevel(likelihood, severity);
        ResidualRiskLevel = CalculateResidualRisk();
    }

    private static RiskLevel CalculateRiskLevel(int likelihood, int severity)
    {
        var riskScore = likelihood * severity;
        
        return riskScore switch
        {
            >= 20 => RiskLevel.Critical,
            >= 15 => RiskLevel.High,
            >= 10 => RiskLevel.Medium,
            >= 5 => RiskLevel.Low,
            _ => RiskLevel.Low
        };
    }

    private RiskLevel CalculateResidualRisk()
    {
        if (!IsControlImplemented)
            return RiskLevel;
            
        // Assume control measures reduce risk by one level (if not already Low)
        return RiskLevel switch
        {
            RiskLevel.Critical => RiskLevel.High,
            RiskLevel.High => RiskLevel.Medium,
            RiskLevel.Medium => RiskLevel.Low,
            _ => RiskLevel.Low
        };
    }
}

