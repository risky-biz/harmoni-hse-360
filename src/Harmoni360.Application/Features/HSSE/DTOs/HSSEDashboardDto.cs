using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.HSSE.DTOs;

public class HSSEDashboardDto
{
    // Existing data
    public HazardStatisticsDto HazardStatistics { get; set; } = new();
    public List<MonthlyHazardDto> MonthlyHazards { get; set; } = new();
    public List<HazardClassificationDto> HazardClassifications { get; set; } = new();
    public List<NonConformanceCriteriaDto> NonConformanceCriteria { get; set; } = new();
    public List<UnsafeConditionDto> TopUnsafeConditions { get; set; } = new();
    public ResponsibleActionSummaryDto ResponsibleActions { get; set; } = new();
    public HazardCaseStatusDto HazardCaseStatus { get; set; } = new();
    public List<IncidentFrequencyRateDto> IncidentFrequencyRates { get; set; } = new();
    public List<SafetyPerformanceDto> SafetyPerformance { get; set; } = new();
    public LostTimeInjuryDto LostTimeInjury { get; set; } = new();
    
    // Extended data from other modules
    public PPEComplianceDto PPECompliance { get; set; } = new();
    public TrainingSafetyDto TrainingSafety { get; set; } = new();
    public InspectionSafetyDto InspectionSafety { get; set; } = new();
    public WorkPermitSafetyDto WorkPermitSafety { get; set; } = new();
    public WasteEnvironmentalDto WasteEnvironmental { get; set; } = new();
    public SecurityIncidentDto SecurityIncidents { get; set; } = new();
    public HealthMonitoringDto HealthMonitoring { get; set; } = new();
    public AuditFindingsDto AuditFindings { get; set; } = new();
}

public class HazardStatisticsDto
{
    public int TotalHazards { get; set; }
    public int NearMiss { get; set; }
    public int Accidents { get; set; }
    public int OpenCases { get; set; }
    public int ClosedCases { get; set; }
    public decimal CompletionRate { get; set; }
}

public class MonthlyHazardDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int HazardCount { get; set; }
    public int NearnessCount { get; set; }
    public int AccidentCount { get; set; }
    public string RiskLevel { get; set; } = string.Empty; // For color coding
}

public class HazardClassificationDto
{
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class NonConformanceCriteriaDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

public class UnsafeConditionDto
{
    public int Rank { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
    public string Severity { get; set; } = string.Empty;
}

public class ResponsibleActionSummaryDto
{
    public int TotalActions { get; set; }
    public int OpenActions { get; set; }
    public int ClosedActions { get; set; }
    public int OverdueActions { get; set; }
    public decimal CompletionRate { get; set; }
    public List<ResponsibleActionItemDto> TopActions { get; set; } = new();
}

public class ResponsibleActionItemDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
}

public class HazardCaseStatusDto
{
    public int TotalCases { get; set; }
    public int OpenCases { get; set; }
    public int ClosedCases { get; set; }
    public decimal OpenPercentage { get; set; }
    public decimal ClosedPercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class IncidentFrequencyRateDto
{
    public int Year { get; set; }
    public decimal TotalRecordableIncidentFrequencyRate { get; set; } // TRIFR
    public decimal TotalRecordableSeverityRate { get; set; } // TRSR
    public decimal StudyRelatedIFR { get; set; }
    public decimal WorkRelatedIFR { get; set; }
    public decimal StudyRelatedSR { get; set; }
    public decimal WorkRelatedSR { get; set; }
}

public class SafetyPerformanceDto
{
    public int Year { get; set; }
    public int NearMiss { get; set; }
    public int Hazards { get; set; }
    public int Accidents { get; set; }
    public decimal IFRStudyRelated { get; set; }
    public decimal IFRWorkRelated { get; set; }
    public decimal TotalIFR { get; set; }
    public string PerformanceLevel { get; set; } = string.Empty; // Good, Average, Poor
    public string ColorCode { get; set; } = string.Empty; // Green, Yellow, Red
}

public class LostTimeInjuryDto
{
    public int Year { get; set; }
    public decimal LTIStudyRelatedRate { get; set; }
    public decimal LTIWorkRelatedRate { get; set; }
    public decimal TotalLTICaseRate { get; set; }
    public int TotalLTICases { get; set; }
    public int StudyRelatedCases { get; set; }
    public int WorkRelatedCases { get; set; }
}

// Extended DTOs for other modules
public class PPEComplianceDto
{
    public int TotalPPEAssignments { get; set; }
    public int ComplianceCount { get; set; }
    public decimal ComplianceRate { get; set; }
    public int OverdueInspections { get; set; }
    public int NonCompliantUsers { get; set; }
    public int DefectiveEquipment { get; set; }
    public List<PPECategoryComplianceDto> CategoryCompliance { get; set; } = new();
}

public class PPECategoryComplianceDto
{
    public string Category { get; set; } = string.Empty;
    public int TotalAssigned { get; set; }
    public int CompliantCount { get; set; }
    public decimal ComplianceRate { get; set; }
}

public class TrainingSafetyDto
{
    public int TotalSafetyTrainings { get; set; }
    public int CompletedTrainings { get; set; }
    public decimal CompletionRate { get; set; }
    public int OverdueTrainings { get; set; }
    public int CertificationExpiries { get; set; }
    public int MandatoryTrainings { get; set; }
    public int MandatoryCompleted { get; set; }
    public decimal MandatoryCompletionRate { get; set; }
}

public class InspectionSafetyDto
{
    public int TotalInspections { get; set; }
    public int SafetyInspections { get; set; }
    public int TotalFindings { get; set; }
    public int CriticalFindings { get; set; }
    public int HighPriorityFindings { get; set; }
    public int CorrectiveActions { get; set; }
    public int CompletedActions { get; set; }
    public decimal CorrectionRate { get; set; }
    public decimal InspectionComplianceRate { get; set; }
}

public class WorkPermitSafetyDto
{
    public int TotalActivePermits { get; set; }
    public int SafetyCompliantPermits { get; set; }
    public decimal SafetyComplianceRate { get; set; }
    public int OverduePermits { get; set; }
    public int SafetyViolations { get; set; }
    public int HotWorkPermits { get; set; }
    public int ConfinedSpacePermits { get; set; }
    public int CompletedSafetyInductions { get; set; }
}

public class WasteEnvironmentalDto
{
    public int TotalWasteReports { get; set; }
    public int EnvironmentalIncidents { get; set; }
    public int ComplianceIssues { get; set; }
    public decimal WasteComplianceRate { get; set; }
    public int HazardousWasteReports { get; set; }
    public int WasteReductionInitiatives { get; set; }
    public decimal EnvironmentalImpactScore { get; set; }
}

public class SecurityIncidentDto
{
    public int TotalSecurityIncidents { get; set; }
    public int PhysicalSecurityIncidents { get; set; }
    public int DataSecurityIncidents { get; set; }
    public int ResolvedIncidents { get; set; }
    public decimal ResolutionRate { get; set; }
    public int CriticalSecurityIncidents { get; set; }
    public int SecurityViolations { get; set; }
    public decimal SecurityComplianceRate { get; set; }
}

public class HealthMonitoringDto
{
    public int TotalHealthIncidents { get; set; }
    public int OccupationalHealthCases { get; set; }
    public int HealthSurveillanceRecords { get; set; }
    public int MedicalEmergencies { get; set; }
    public int HealthComplianceIssues { get; set; }
    public decimal HealthComplianceRate { get; set; }
    public int PreventiveMeasuresImplemented { get; set; }
}

public class AuditFindingsDto
{
    public int TotalAudits { get; set; }
    public int SafetyAudits { get; set; }
    public int TotalFindings { get; set; }
    public int MajorNonConformities { get; set; }
    public int MinorNonConformities { get; set; }
    public int CorrectiveActions { get; set; }
    public int CompletedActions { get; set; }
    public decimal ActionCompletionRate { get; set; }
    public decimal AuditComplianceScore { get; set; }
}