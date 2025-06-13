namespace Harmoni360.Application.Features.Incidents.DTOs;

public class IncidentDashboardDto
{
    // Overall statistics
    public IncidentOverallStatsDto OverallStats { get; set; } = new();
    
    // Status distribution
    public List<IncidentStatusStatsDto> StatusStats { get; set; } = new();
    
    // Severity distribution
    public List<IncidentSeverityStatsDto> SeverityStats { get; set; } = new();
    
    // Response time analytics
    public IncidentResponseTimeStatsDto ResponseTimeStats { get; set; } = new();
    
    // Resolution time analytics
    public IncidentResolutionTimeStatsDto ResolutionTimeStats { get; set; } = new();
    
    // Trend data (last 12 months)
    public List<IncidentTrendDataDto> TrendData { get; set; } = new();
    
    // Category breakdown
    public List<IncidentCategoryStatsDto> CategoryStats { get; set; } = new();
    
    // Recent incidents
    public List<RecentIncidentDto> RecentIncidents { get; set; } = new();
    
    // Performance metrics
    public IncidentPerformanceMetricsDto PerformanceMetrics { get; set; } = new();
    
    // Department statistics
    public List<IncidentDepartmentStatsDto> DepartmentStats { get; set; } = new();
}

public class IncidentOverallStatsDto
{
    public int TotalIncidents { get; set; }
    public int OpenIncidents { get; set; }
    public int ClosedIncidents { get; set; }
    public int CriticalIncidents { get; set; }
    public int IncidentsThisMonth { get; set; }
    public int IncidentsLastMonth { get; set; }
    public decimal MonthOverMonthChange { get; set; }
    public int OverdueIncidents { get; set; }
    public int IncidentsAwaitingAction { get; set; }
}

public class IncidentStatusStatsDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class IncidentSeverityStatsDto
{
    public string Severity { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
    public string Color { get; set; } = string.Empty;
    public int ThisMonth { get; set; }
    public int LastMonth { get; set; }
}

public class IncidentResponseTimeStatsDto
{
    public double AverageResponseTimeHours { get; set; }
    public double MedianResponseTimeHours { get; set; }
    public int IncidentsWithinSLA { get; set; }
    public int TotalIncidentsWithResponse { get; set; }
    public decimal SLACompliancePercentage { get; set; }
    public double CriticalIncidentsAvgResponseHours { get; set; }
    public List<ResponseTimeTrendDto> ResponseTimeTrends { get; set; } = new();
}

public class IncidentResolutionTimeStatsDto
{
    public double AverageResolutionTimeDays { get; set; }
    public double MedianResolutionTimeDays { get; set; }
    public int ResolvedIncidents { get; set; }
    public int PendingResolution { get; set; }
    public decimal ResolutionRate { get; set; }
    public List<ResolutionTimeTrendDto> ResolutionTimeTrends { get; set; } = new();
}

public class IncidentTrendDataDto
{
    public string Period { get; set; } = string.Empty; // "2024-01" format
    public string PeriodLabel { get; set; } = string.Empty; // "Jan 2024" format
    public int TotalIncidents { get; set; }
    public int CriticalIncidents { get; set; }
    public int ResolvedIncidents { get; set; }
    public double AverageResolutionDays { get; set; }
}

public class IncidentCategoryStatsDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
    public int CriticalCount { get; set; }
    public double AverageResolutionDays { get; set; }
}

public class RecentIncidentDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime IncidentDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int DaysOpen { get; set; }
    public bool IsOverdue { get; set; }
}

public class IncidentPerformanceMetricsDto
{
    public decimal IncidentPreventionRate { get; set; }
    public decimal RepeatIncidentRate { get; set; }
    public decimal CorrectiveActionCompletionRate { get; set; }
    public int AverageCorrectiveActionsPerIncident { get; set; }
    public decimal EmployeeReportingEngagement { get; set; }
    public int UniqueReportersThisMonth { get; set; }
}

public class IncidentDepartmentStatsDto
{
    public string Department { get; set; } = string.Empty;
    public int IncidentCount { get; set; }
    public int CriticalCount { get; set; }
    public decimal AverageResolutionDays { get; set; }
    public decimal ComplianceScore { get; set; }
}

public class ResponseTimeTrendDto
{
    public string Period { get; set; } = string.Empty;
    public double AverageHours { get; set; }
    public int IncidentCount { get; set; }
}

public class ResolutionTimeTrendDto
{
    public string Period { get; set; } = string.Empty;
    public double AverageDays { get; set; }
    public int ResolvedCount { get; set; }
}