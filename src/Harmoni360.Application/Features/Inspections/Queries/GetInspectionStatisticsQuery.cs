using Harmoni360.Application.Features.Inspections.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;

namespace Harmoni360.Application.Features.Inspections.Queries;

public record GetInspectionStatisticsQuery : IRequest<InspectionStatisticsDto>
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int? DepartmentId { get; init; }
    public int? InspectorId { get; init; }
    public InspectionType? Type { get; init; }
    public InspectionCategory? Category { get; init; }
}

public class InspectionStatisticsDto
{
    // Primary metrics
    public int TotalInspections { get; set; }
    public int ScheduledInspections { get; set; }
    public int InProgressInspections { get; set; }
    public int CompletedInspections { get; set; }
    public int OverdueInspections { get; set; }
    public int CancelledInspections { get; set; }
    
    // Completion metrics
    public double CompletionRate { get; set; }
    public double OverdueRate { get; set; }
    public double AverageCompletionTime { get; set; }
    
    // Findings metrics
    public int TotalFindings { get; set; }
    public int CriticalFindings { get; set; }
    public int MajorFindings { get; set; }
    public int ModerateFindings { get; set; }
    public int MinorFindings { get; set; }
    public double FindingsPerInspection { get; set; }
    
    // Breakdown by status
    public List<InspectionStatusStatistic> ByStatus { get; set; } = new();
    
    // Breakdown by type
    public List<InspectionTypeStatistic> ByType { get; set; } = new();
    
    // Breakdown by department
    public List<DepartmentStatsDto> ByDepartment { get; set; } = new();
    
    // Monthly trends
    public List<MonthlyInspectionTrend> MonthlyTrends { get; set; } = new();
}