using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Application.Features.Incidents.DTOs;
using HarmoniHSE360.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarmoniHSE360.Application.Features.Incidents.Queries;

public class GetIncidentDashboardQueryHandler : IRequestHandler<GetIncidentDashboardQuery, IncidentDashboardDto>
{
    private readonly IApplicationDbContext _context;

    public GetIncidentDashboardQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IncidentDashboardDto> Handle(GetIncidentDashboardQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Incidents
            .Include(i => i.Reporter)
            .Include(i => i.Investigator)
            .Include(i => i.CorrectiveActions)
            .AsQueryable();

        // Apply filters
        if (request.FromDate.HasValue)
            query = query.Where(i => i.CreatedAt >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(i => i.CreatedAt <= request.ToDate.Value);

        if (!string.IsNullOrEmpty(request.Department))
            query = query.Where(i => i.ReporterDepartment == request.Department);

        if (!request.IncludeResolved)
            query = query.Where(i => i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed);

        var incidents = await query.ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;

        // Calculate overall statistics
        var overallStats = CalculateOverallStats(incidents, now);

        // Calculate status distribution
        var statusStats = CalculateStatusStats(incidents);

        // Calculate severity distribution
        var severityStats = CalculateSeverityStats(incidents, now);

        // Calculate response time analytics
        var responseTimeStats = CalculateResponseTimeStats(incidents);

        // Calculate resolution time analytics
        var resolutionTimeStats = CalculateResolutionTimeStats(incidents);

        // Calculate trend data (last 12 months)
        var trendData = await CalculateTrendData(cancellationToken);

        // Calculate category breakdown
        var categoryStats = CalculateCategoryStats(incidents);

        // Get recent incidents
        var recentIncidents = GetRecentIncidents(incidents, now);

        // Calculate performance metrics
        var performanceMetrics = await CalculatePerformanceMetrics(incidents, cancellationToken);

        // Calculate department statistics
        var departmentStats = CalculateDepartmentStats(incidents);

        return new IncidentDashboardDto
        {
            OverallStats = overallStats,
            StatusStats = statusStats,
            SeverityStats = severityStats,
            ResponseTimeStats = responseTimeStats,
            ResolutionTimeStats = resolutionTimeStats,
            TrendData = trendData,
            CategoryStats = categoryStats,
            RecentIncidents = recentIncidents,
            PerformanceMetrics = performanceMetrics,
            DepartmentStats = departmentStats
        };
    }

    private static IncidentOverallStatsDto CalculateOverallStats(List<Incident> incidents, DateTime now)
    {
        var thisMonth = now.Month;
        var thisYear = now.Year;
        var lastMonth = thisMonth == 1 ? 12 : thisMonth - 1;
        var lastMonthYear = thisMonth == 1 ? thisYear - 1 : thisYear;

        var incidentsThisMonth = incidents.Count(i => i.CreatedAt.Month == thisMonth && i.CreatedAt.Year == thisYear);
        var incidentsLastMonth = incidents.Count(i => i.CreatedAt.Month == lastMonth && i.CreatedAt.Year == lastMonthYear);

        var monthOverMonthChange = incidentsLastMonth == 0 ? 0 : 
            ((decimal)(incidentsThisMonth - incidentsLastMonth) / incidentsLastMonth) * 100;

        return new IncidentOverallStatsDto
        {
            TotalIncidents = incidents.Count,
            OpenIncidents = incidents.Count(i => i.Status == IncidentStatus.Reported || 
                                                i.Status == IncidentStatus.UnderInvestigation),
            ClosedIncidents = incidents.Count(i => i.Status == IncidentStatus.Resolved || 
                                                 i.Status == IncidentStatus.Closed),
            CriticalIncidents = incidents.Count(i => i.Severity == IncidentSeverity.Critical),
            IncidentsThisMonth = incidentsThisMonth,
            IncidentsLastMonth = incidentsLastMonth,
            MonthOverMonthChange = Math.Round(monthOverMonthChange, 1),
            OverdueIncidents = incidents.Count(i => i.Status != IncidentStatus.Resolved && 
                                                   i.Status != IncidentStatus.Closed &&
                                                   i.CreatedAt < now.AddDays(-7)),
            IncidentsAwaitingAction = incidents.Count(i => i.Status == IncidentStatus.AwaitingAction)
        };
    }

    private static List<IncidentStatusStatsDto> CalculateStatusStats(List<Incident> incidents)
    {
        var totalCount = incidents.Count;
        if (totalCount == 0) return new List<IncidentStatusStatsDto>();

        var statusColors = new Dictionary<IncidentStatus, string>
        {
            { IncidentStatus.Reported, "primary" },
            { IncidentStatus.UnderInvestigation, "warning" },
            { IncidentStatus.AwaitingAction, "danger" },
            { IncidentStatus.Resolved, "success" },
            { IncidentStatus.Closed, "secondary" }
        };

        return incidents
            .GroupBy(i => i.Status)
            .Select(g => new IncidentStatusStatsDto
            {
                Status = g.Key.ToString(),
                Count = g.Count(),
                Percentage = Math.Round((decimal)g.Count() / totalCount * 100, 1),
                Color = statusColors.GetValueOrDefault(g.Key, "secondary")
            })
            .OrderByDescending(s => s.Count)
            .ToList();
    }

    private static List<IncidentSeverityStatsDto> CalculateSeverityStats(List<Incident> incidents, DateTime now)
    {
        var totalCount = incidents.Count;
        if (totalCount == 0) return new List<IncidentSeverityStatsDto>();

        var thisMonth = now.Month;
        var thisYear = now.Year;
        var lastMonth = thisMonth == 1 ? 12 : thisMonth - 1;
        var lastMonthYear = thisMonth == 1 ? thisYear - 1 : thisYear;

        var severityColors = new Dictionary<IncidentSeverity, string>
        {
            { IncidentSeverity.Minor, "success" },
            { IncidentSeverity.Moderate, "warning" },
            { IncidentSeverity.Serious, "danger" },
            { IncidentSeverity.Critical, "dark" }
        };

        return incidents
            .GroupBy(i => i.Severity)
            .Select(g => new IncidentSeverityStatsDto
            {
                Severity = g.Key.ToString(),
                Count = g.Count(),
                Percentage = Math.Round((decimal)g.Count() / totalCount * 100, 1),
                Color = severityColors.GetValueOrDefault(g.Key, "secondary"),
                ThisMonth = g.Count(i => i.CreatedAt.Month == thisMonth && i.CreatedAt.Year == thisYear),
                LastMonth = g.Count(i => i.CreatedAt.Month == lastMonth && i.CreatedAt.Year == lastMonthYear)
            })
            .OrderByDescending(s => (int)Enum.Parse<IncidentSeverity>(s.Severity))
            .ToList();
    }

    private static IncidentResponseTimeStatsDto CalculateResponseTimeStats(List<Incident> incidents)
    {
        var incidentsWithResponse = incidents
            .Where(i => i.LastResponseAt.HasValue)
            .ToList();

        if (!incidentsWithResponse.Any())
        {
            return new IncidentResponseTimeStatsDto();
        }

        var responseTimes = incidentsWithResponse
            .Select(i => (i.LastResponseAt!.Value - i.CreatedAt).TotalHours)
            .ToList();

        var slaHours = 24; // 24 hours SLA
        var withinSLA = responseTimes.Count(rt => rt <= slaHours);

        var criticalIncidentsWithResponse = incidentsWithResponse
            .Where(i => i.Severity == IncidentSeverity.Critical)
            .ToList();

        var criticalResponseTimes = criticalIncidentsWithResponse.Any() 
            ? criticalIncidentsWithResponse.Select(i => (i.LastResponseAt!.Value - i.CreatedAt).TotalHours).Average()
            : 0;

        return new IncidentResponseTimeStatsDto
        {
            AverageResponseTimeHours = Math.Round(responseTimes.Average(), 2),
            MedianResponseTimeHours = Math.Round(responseTimes.OrderBy(x => x).Skip(responseTimes.Count / 2).First(), 2),
            IncidentsWithinSLA = withinSLA,
            TotalIncidentsWithResponse = incidentsWithResponse.Count,
            SLACompliancePercentage = Math.Round((decimal)withinSLA / incidentsWithResponse.Count * 100, 1),
            CriticalIncidentsAvgResponseHours = Math.Round(criticalResponseTimes, 2)
        };
    }

    private static IncidentResolutionTimeStatsDto CalculateResolutionTimeStats(List<Incident> incidents)
    {
        var resolvedIncidents = incidents
            .Where(i => i.Status == IncidentStatus.Resolved || i.Status == IncidentStatus.Closed)
            .Where(i => i.LastModifiedAt.HasValue)
            .ToList();

        if (!resolvedIncidents.Any())
        {
            return new IncidentResolutionTimeStatsDto
            {
                PendingResolution = incidents.Count(i => i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed)
            };
        }

        var resolutionTimes = resolvedIncidents
            .Select(i => (i.LastModifiedAt!.Value - i.CreatedAt).TotalDays)
            .ToList();

        var pendingResolution = incidents.Count(i => i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed);
        var totalIncidents = incidents.Count;

        return new IncidentResolutionTimeStatsDto
        {
            AverageResolutionTimeDays = Math.Round(resolutionTimes.Average(), 2),
            MedianResolutionTimeDays = Math.Round(resolutionTimes.OrderBy(x => x).Skip(resolutionTimes.Count / 2).First(), 2),
            ResolvedIncidents = resolvedIncidents.Count,
            PendingResolution = pendingResolution,
            ResolutionRate = totalIncidents > 0 ? Math.Round((decimal)resolvedIncidents.Count / totalIncidents * 100, 1) : 0
        };
    }

    private async Task<List<IncidentTrendDataDto>> CalculateTrendData(CancellationToken cancellationToken)
    {
        var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-12);
        
        var trendIncidents = await _context.Incidents
            .Where(i => i.CreatedAt >= twelveMonthsAgo)
            .ToListAsync(cancellationToken);

        var monthlyData = trendIncidents
            .GroupBy(i => new { i.CreatedAt.Year, i.CreatedAt.Month })
            .Select(g => new IncidentTrendDataDto
            {
                Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                PeriodLabel = $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM yyyy}",
                TotalIncidents = g.Count(),
                CriticalIncidents = g.Count(i => i.Severity == IncidentSeverity.Critical),
                ResolvedIncidents = g.Count(i => i.Status == IncidentStatus.Resolved || i.Status == IncidentStatus.Closed),
                AverageResolutionDays = g.Where(i => i.Status == IncidentStatus.Resolved || i.Status == IncidentStatus.Closed)
                                        .Where(i => i.LastModifiedAt.HasValue)
                                        .Select(i => (i.LastModifiedAt!.Value - i.CreatedAt).TotalDays)
                                        .DefaultIfEmpty(0)
                                        .Average()
            })
            .OrderBy(t => t.Period)
            .ToList();

        return monthlyData;
    }

    private static List<IncidentCategoryStatsDto> CalculateCategoryStats(List<Incident> incidents)
    {
        var totalCount = incidents.Count;
        if (totalCount == 0) return new List<IncidentCategoryStatsDto>();

        // Extract category from title or use a default categorization
        var categoryMapping = new Dictionary<string, string>
        {
            { "injury", "Workplace Injury" },
            { "accident", "Accident" },
            { "fire", "Fire Safety" },
            { "spill", "Environmental" },
            { "equipment", "Equipment Failure" },
            { "safety", "Safety Violation" },
            { "health", "Health Incident" },
            { "security", "Security Incident" }
        };

        var categorizedIncidents = incidents.Select(i => new
        {
            Incident = i,
            Category = GetIncidentCategory(i.Title.ToLowerInvariant(), categoryMapping)
        }).ToList();

        return categorizedIncidents
            .GroupBy(ci => ci.Category)
            .Select(g => new IncidentCategoryStatsDto
            {
                Category = g.Key,
                Count = g.Count(),
                Percentage = Math.Round((decimal)g.Count() / totalCount * 100, 1),
                CriticalCount = g.Count(ci => ci.Incident.Severity == IncidentSeverity.Critical),
                AverageResolutionDays = g.Where(ci => ci.Incident.Status == IncidentStatus.Resolved || ci.Incident.Status == IncidentStatus.Closed)
                                        .Where(ci => ci.Incident.LastModifiedAt.HasValue)
                                        .Select(ci => (ci.Incident.LastModifiedAt!.Value - ci.Incident.CreatedAt).TotalDays)
                                        .DefaultIfEmpty(0)
                                        .Average()
            })
            .OrderByDescending(c => c.Count)
            .ToList();
    }

    private static string GetIncidentCategory(string title, Dictionary<string, string> categoryMapping)
    {
        foreach (var mapping in categoryMapping)
        {
            if (title.Contains(mapping.Key))
                return mapping.Value;
        }
        return "Other";
    }

    private static List<RecentIncidentDto> GetRecentIncidents(List<Incident> incidents, DateTime now)
    {
        return incidents
            .OrderByDescending(i => i.CreatedAt)
            .Take(10)
            .Select(i => new RecentIncidentDto
            {
                Id = i.Id,
                Title = i.Title,
                Severity = i.Severity.ToString(),
                Status = i.Status.ToString(),
                IncidentDate = i.IncidentDate,
                CreatedAt = i.CreatedAt,
                ReporterName = i.ReporterName,
                Location = i.Location,
                DaysOpen = (int)(now - i.CreatedAt).TotalDays,
                IsOverdue = i.Status != IncidentStatus.Resolved && 
                           i.Status != IncidentStatus.Closed && 
                           i.CreatedAt < now.AddDays(-7)
            })
            .ToList();
    }

    private async Task<IncidentPerformanceMetricsDto> CalculatePerformanceMetrics(List<Incident> incidents, CancellationToken cancellationToken)
    {
        var thisMonth = DateTime.UtcNow.Month;
        var thisYear = DateTime.UtcNow.Year;

        var resolvedIncidents = incidents.Where(i => i.Status == IncidentStatus.Resolved || i.Status == IncidentStatus.Closed).ToList();
        var totalCorrectiveActions = resolvedIncidents.Sum(i => i.CorrectiveActions?.Count ?? 0);
        var completedCorrectiveActions = resolvedIncidents
            .SelectMany(i => i.CorrectiveActions ?? new List<CorrectiveAction>())
            .Count(ca => ca.Status == ActionStatus.Completed);

        var uniqueReportersThisMonth = await _context.Incidents
            .Where(i => i.CreatedAt.Month == thisMonth && i.CreatedAt.Year == thisYear)
            .Select(i => i.ReporterId)
            .Distinct()
            .CountAsync(cancellationToken);

        var totalEmployees = await _context.Users.CountAsync(cancellationToken);

        return new IncidentPerformanceMetricsDto
        {
            IncidentPreventionRate = 85.0m, // This would be calculated based on near-miss reports vs actual incidents
            RepeatIncidentRate = 12.5m, // This would require additional logic to detect similar incidents
            CorrectiveActionCompletionRate = totalCorrectiveActions > 0 
                ? Math.Round((decimal)completedCorrectiveActions / totalCorrectiveActions * 100, 1) 
                : 0,
            AverageCorrectiveActionsPerIncident = resolvedIncidents.Count > 0 
                ? totalCorrectiveActions / resolvedIncidents.Count 
                : 0,
            EmployeeReportingEngagement = totalEmployees > 0 
                ? Math.Round((decimal)uniqueReportersThisMonth / totalEmployees * 100, 1) 
                : 0,
            UniqueReportersThisMonth = uniqueReportersThisMonth
        };
    }

    private static List<IncidentDepartmentStatsDto> CalculateDepartmentStats(List<Incident> incidents)
    {
        return incidents
            .Where(i => !string.IsNullOrEmpty(i.ReporterDepartment))
            .GroupBy(i => i.ReporterDepartment!)
            .Select(g => new IncidentDepartmentStatsDto
            {
                Department = g.Key,
                IncidentCount = g.Count(),
                CriticalCount = g.Count(i => i.Severity == IncidentSeverity.Critical),
                AverageResolutionDays = (decimal)g.Where(i => i.Status == IncidentStatus.Resolved || i.Status == IncidentStatus.Closed)
                                        .Where(i => i.LastModifiedAt.HasValue)
                                        .Select(i => (i.LastModifiedAt!.Value - i.CreatedAt).TotalDays)
                                        .DefaultIfEmpty(0)
                                        .Average(),
                ComplianceScore = CalculateDepartmentComplianceScore(g.ToList())
            })
            .OrderByDescending(d => d.IncidentCount)
            .ToList();
    }

    private static decimal CalculateDepartmentComplianceScore(List<Incident> departmentIncidents)
    {
        if (!departmentIncidents.Any()) return 100;

        var resolvedIncidents = departmentIncidents.Count(i => i.Status == IncidentStatus.Resolved || i.Status == IncidentStatus.Closed);
        var criticalIncidents = departmentIncidents.Count(i => i.Severity == IncidentSeverity.Critical);
        var overdueIncidents = departmentIncidents.Count(i => 
            i.Status != IncidentStatus.Resolved && 
            i.Status != IncidentStatus.Closed && 
            i.CreatedAt < DateTime.UtcNow.AddDays(-7));

        var resolutionRate = (decimal)resolvedIncidents / departmentIncidents.Count;
        var criticalPenalty = (decimal)criticalIncidents / departmentIncidents.Count * 0.3m;
        var overduePenalty = (decimal)overdueIncidents / departmentIncidents.Count * 0.4m;

        var score = (resolutionRate * 100) - (criticalPenalty * 100) - (overduePenalty * 100);
        return Math.Max(0, Math.Min(100, Math.Round(score, 1)));
    }
}