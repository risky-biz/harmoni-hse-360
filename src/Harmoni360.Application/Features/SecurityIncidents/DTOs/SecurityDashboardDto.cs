using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.SecurityIncidents.DTOs;

public class SecurityDashboardDto
{
    public SecurityMetricsDto Metrics { get; set; } = new();
    public SecurityTrendsDto Trends { get; set; } = new();
    public List<SecurityIncidentListDto> RecentIncidents { get; set; } = new();
    public List<SecurityIncidentListDto> CriticalIncidents { get; set; } = new();
    public List<SecurityIncidentListDto> OverdueIncidents { get; set; } = new();
    public List<ThreatIndicatorDto> RecentThreatIndicators { get; set; } = new();
    public SecurityComplianceStatusDto ComplianceStatus { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class SecurityMetricsDto
{
    public int TotalIncidents { get; set; }
    public int OpenIncidents { get; set; }
    public int ClosedIncidents { get; set; }
    public int CriticalIncidents { get; set; }
    public int HighSeverityIncidents { get; set; }
    public int DataBreachIncidents { get; set; }
    public int InternalThreatIncidents { get; set; }
    public int PhysicalSecurityIncidents { get; set; }
    public int CybersecurityIncidents { get; set; }
    public int PersonnelSecurityIncidents { get; set; }
    public int InformationSecurityIncidents { get; set; }
    public int OverdueIncidents { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public double IncidentTrend { get; set; } // Percentage change from previous period
    public SecuritySeverity MostCommonSeverity { get; set; }
    public SecurityIncidentType MostCommonType { get; set; }
    public string MostCommonLocation { get; set; } = string.Empty;
}

public class SecurityTrendsDto
{
    public List<SecurityTrendDataPoint> IncidentTrends { get; set; } = new();
    public List<SecurityTrendDataPoint> SeverityTrends { get; set; } = new();
    public List<SecurityTrendDataPoint> TypeTrends { get; set; } = new();
    public List<SecurityTrendDataPoint> ThreatLevelTrends { get; set; } = new();
    public List<SecurityLocationTrendDto> LocationTrends { get; set; } = new();
    public List<SecurityThreatActorTrendDto> ThreatActorTrends { get; set; } = new();
}

public class SecurityTrendDataPoint
{
    public DateTime Date { get; set; }
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class SecurityLocationTrendDto
{
    public string Location { get; set; } = string.Empty;
    public int IncidentCount { get; set; }
    public SecuritySeverity HighestSeverity { get; set; }
    public SecurityIncidentType MostCommonType { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class SecurityThreatActorTrendDto
{
    public ThreatActorType ActorType { get; set; }
    public int IncidentCount { get; set; }
    public SecuritySeverity AverageSeverity { get; set; }
    public List<SecurityIncidentType> TargetedTypes { get; set; } = new();
}

public class SecurityComplianceStatusDto
{
    public bool ISO27001Compliant { get; set; }
    public bool ITELawCompliant { get; set; }
    public bool SMK3Compliant { get; set; }
    public int ComplianceScore { get; set; } // 0-100
    public List<SecurityComplianceIssueDto> Issues { get; set; } = new();
    public DateTime LastAssessmentDate { get; set; }
    public DateTime NextAssessmentDue { get; set; }
}

public class SecurityComplianceIssueDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SecuritySeverity Severity { get; set; }
    public DateTime DueDate { get; set; }
    public string ResponsiblePerson { get; set; } = string.Empty;
    public bool IsOverdue => DateTime.UtcNow > DueDate;
}

public class SecurityAnalyticsDto
{
    public SecurityRiskMatrixDto RiskMatrix { get; set; } = new();
    public List<SecurityPatternDto> IdentifiedPatterns { get; set; } = new();
    public List<SecurityPredictionDto> Predictions { get; set; } = new();
    public SecurityBenchmarkDto Benchmarks { get; set; } = new();
}

public class SecurityRiskMatrixDto
{
    public List<SecurityRiskCellDto> RiskCells { get; set; } = new();
    public int TotalIncidents { get; set; }
    public double AverageRiskScore { get; set; }
}

public class SecurityRiskCellDto
{
    public int Likelihood { get; set; } // 1-5
    public int Impact { get; set; } // 1-5
    public int IncidentCount { get; set; }
    public List<SecurityIncidentType> IncidentTypes { get; set; } = new();
}

public class SecurityPatternDto
{
    public string PatternName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; } // 0-100
    public List<SecurityIncidentListDto> RelatedIncidents { get; set; } = new();
    public string RecommendedAction { get; set; } = string.Empty;
}

public class SecurityPredictionDto
{
    public string PredictionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Probability { get; set; } // 0-100
    public DateTime PredictedTimeframe { get; set; }
    public List<string> RecommendedMitigations { get; set; } = new();
}

public class SecurityBenchmarkDto
{
    public double IndustryAverageIncidents { get; set; }
    public double IndustryAverageResolutionTime { get; set; }
    public double OrganizationPerformance { get; set; } // Percentage compared to industry
    public string PerformanceRating { get; set; } = string.Empty; // Excellent, Good, Average, Below Average, Poor
    public List<string> ImprovementRecommendations { get; set; } = new();
}