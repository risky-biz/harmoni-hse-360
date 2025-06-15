using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Inspections.DTOs;

public class InspectionDashboardDto
{
    // Primary KPI metrics
    public int TotalInspections { get; set; }
    public int ScheduledInspections { get; set; }
    public int InProgressInspections { get; set; }
    public int CompletedInspections { get; set; }
    public int OverdueInspections { get; set; }
    public int CriticalFindings { get; set; }
    public double AverageCompletionTime { get; set; }
    public double ComplianceRate { get; set; }

    // Lists for dashboard sections
    public List<InspectionDto> RecentInspections { get; set; } = new();
    public List<InspectionFindingDto> CriticalFindingsList { get; set; } = new();
    public List<InspectionDto> UpcomingInspections { get; set; } = new();
    public List<InspectionDto> OverdueList { get; set; } = new();

    // Statistics for charts
    public List<InspectionStatusStatistic> InspectionsByStatus { get; set; } = new();
    public List<InspectionTypeStatistic> InspectionsByType { get; set; } = new();
    public List<MonthlyInspectionTrend> MonthlyTrends { get; set; } = new();

    // Legacy properties for compatibility
    public InspectionStatsDto Stats { get; set; } = new();
    public List<InspectionTrendDto> Trends { get; set; } = new();
    public List<DepartmentStatsDto> DepartmentStats { get; set; } = new();
    public List<InspectionTypeStatsDto> TypeStats { get; set; } = new();
}

// Additional DTOs for dashboard statistics
public class InspectionStatusStatistic
{
    public InspectionStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class InspectionTypeStatistic
{
    public InspectionType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class MonthlyInspectionTrend
{
    public string Month { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Scheduled { get; set; }
    public int Completed { get; set; }
    public int Overdue { get; set; }
    public int CriticalFindings { get; set; }
}

public class InspectionStatsDto
{
    public int TotalInspections { get; set; }
    public int ScheduledInspections { get; set; }
    public int InProgressInspections { get; set; }
    public int CompletedInspections { get; set; }
    public int OverdueInspections { get; set; }
    public int CancelledInspections { get; set; }
    public double CompletionRate { get; set; }
    public double OverdueRate { get; set; }
    public int TotalFindings { get; set; }
    public int CriticalFindings { get; set; }
    public int PendingFindings { get; set; }
    public double FindingsPerInspection { get; set; }
    public int TotalTrend { get; set; }
    public int CompletedTrend { get; set; }
    public int InProgressTrend { get; set; }
    public int OverdueTrend { get; set; }
}

public class InspectionTrendDto
{
    public DateTime Date { get; set; }
    public int TotalInspections { get; set; }
    public int CompletedInspections { get; set; }
    public int OverdueInspections { get; set; }
    public int FindingsCount { get; set; }
}

public class DepartmentStatsDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int TotalInspections { get; set; }
    public int CompletedInspections { get; set; }
    public int OverdueInspections { get; set; }
    public double CompletionRate { get; set; }
    public int FindingsCount { get; set; }
}

public class InspectionTypeStatsDto
{
    public InspectionType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int Count { get; set; }
    public int CompletedCount { get; set; }
    public int OverdueCount { get; set; }
    public double CompletionRate { get; set; }
}