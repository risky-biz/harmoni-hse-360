using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Inspections.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.Inspections.Queries;

public class GetInspectionStatisticsQueryHandler : IRequestHandler<GetInspectionStatisticsQuery, InspectionStatisticsDto>
{
    private readonly IApplicationDbContext _context;

    public GetInspectionStatisticsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InspectionStatisticsDto> Handle(GetInspectionStatisticsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Inspections
            .Include(i => i.Inspector)
            .Include(i => i.Department)
            .Include(i => i.Items)
            .Include(i => i.Findings)
            .AsQueryable();

        // Apply filters
        if (request.StartDate.HasValue)
            query = query.Where(i => i.CreatedAt >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(i => i.CreatedAt <= request.EndDate.Value);

        if (request.DepartmentId.HasValue)
            query = query.Where(i => i.DepartmentId == request.DepartmentId.Value);

        if (request.InspectorId.HasValue)
            query = query.Where(i => i.InspectorId == request.InspectorId.Value);

        if (request.Type.HasValue)
            query = query.Where(i => i.Type == request.Type.Value);

        if (request.Category.HasValue)
            query = query.Where(i => i.Category == request.Category.Value);

        var inspections = await query.ToListAsync(cancellationToken);

        // Calculate basic metrics
        var totalInspections = inspections.Count;
        var scheduledInspections = inspections.Count(i => i.Status == InspectionStatus.Scheduled);
        var inProgressInspections = inspections.Count(i => i.Status == InspectionStatus.InProgress);
        var completedInspections = inspections.Count(i => i.Status == InspectionStatus.Completed);
        var overdueInspections = inspections.Count(i => i.IsOverdue);
        var cancelledInspections = inspections.Count(i => i.Status == InspectionStatus.Cancelled);

        // Calculate rates
        var completionRate = totalInspections > 0 ? Math.Round((double)completedInspections / totalInspections * 100, 1) : 0;
        var overdueRate = totalInspections > 0 ? Math.Round((double)overdueInspections / totalInspections * 100, 1) : 0;

        // Calculate average completion time
        var completedWithDuration = inspections
            .Where(i => i.Status == InspectionStatus.Completed && i.ActualDurationMinutes.HasValue);
        var averageCompletionTime = completedWithDuration.Any() 
            ? Math.Round(completedWithDuration.Average(i => i.ActualDurationMinutes!.Value) / 60.0, 1)
            : 0;

        // Calculate findings metrics
        var allFindings = inspections.SelectMany(i => i.Findings).ToList();
        var totalFindings = allFindings.Count;
        var criticalFindings = allFindings.Count(f => f.Severity == FindingSeverity.Critical);
        var majorFindings = allFindings.Count(f => f.Severity == FindingSeverity.Major);
        var moderateFindings = allFindings.Count(f => f.Severity == FindingSeverity.Moderate);
        var minorFindings = allFindings.Count(f => f.Severity == FindingSeverity.Minor);
        var findingsPerInspection = totalInspections > 0 ? Math.Round((double)totalFindings / totalInspections, 1) : 0;

        // Statistics by status
        var byStatus = Enum.GetValues(typeof(InspectionStatus)).Cast<InspectionStatus>()
            .Select(status => new InspectionStatusStatistic
            {
                Status = status,
                StatusName = status.ToString(),
                Count = inspections.Count(i => i.Status == status),
                Percentage = totalInspections > 0 ? Math.Round((double)inspections.Count(i => i.Status == status) / totalInspections * 100, 1) : 0
            })
            .Where(s => s.Count > 0)
            .ToList();

        // Statistics by type
        var byType = Enum.GetValues(typeof(InspectionType)).Cast<InspectionType>()
            .Select(type => new InspectionTypeStatistic
            {
                Type = type,
                TypeName = type.ToString(),
                Count = inspections.Count(i => i.Type == type),
                Percentage = totalInspections > 0 ? Math.Round((double)inspections.Count(i => i.Type == type) / totalInspections * 100, 1) : 0
            })
            .Where(s => s.Count > 0)
            .ToList();

        // Statistics by department
        var byDepartment = inspections
            .Where(i => i.Department != null)
            .GroupBy(i => new { DepartmentId = i.DepartmentId ?? 0, i.Department!.Name })
            .Select(g => new DepartmentStatsDto
            {
                DepartmentId = g.Key.DepartmentId,
                DepartmentName = g.Key.Name,
                TotalInspections = g.Count(),
                CompletedInspections = g.Count(i => i.Status == InspectionStatus.Completed),
                OverdueInspections = g.Count(i => i.IsOverdue),
                CompletionRate = g.Count() > 0 ? Math.Round((double)g.Count(i => i.Status == InspectionStatus.Completed) / g.Count() * 100, 1) : 0,
                FindingsCount = g.SelectMany(i => i.Findings).Count()
            })
            .OrderByDescending(d => d.TotalInspections)
            .ToList();

        // Monthly trends (last 6 months)
        var monthlyTrends = new List<MonthlyInspectionTrend>();
        for (int i = 5; i >= 0; i--)
        {
            var monthStart = DateTime.UtcNow.AddMonths(-i).Date.AddDays(1 - DateTime.UtcNow.AddMonths(-i).Day);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            
            var monthInspections = inspections.Where(insp => 
                insp.CreatedAt >= monthStart && insp.CreatedAt <= monthEnd).ToList();

            monthlyTrends.Add(new MonthlyInspectionTrend
            {
                Month = monthStart.ToString("MMM"),
                Year = monthStart.Year,
                Scheduled = monthInspections.Count(i => i.Status == InspectionStatus.Scheduled),
                Completed = monthInspections.Count(i => i.Status == InspectionStatus.Completed),
                Overdue = monthInspections.Count(i => i.IsOverdue),
                CriticalFindings = monthInspections.SelectMany(i => i.Findings).Count(f => f.Severity == FindingSeverity.Critical)
            });
        }

        return new InspectionStatisticsDto
        {
            TotalInspections = totalInspections,
            ScheduledInspections = scheduledInspections,
            InProgressInspections = inProgressInspections,
            CompletedInspections = completedInspections,
            OverdueInspections = overdueInspections,
            CancelledInspections = cancelledInspections,
            CompletionRate = completionRate,
            OverdueRate = overdueRate,
            AverageCompletionTime = averageCompletionTime,
            TotalFindings = totalFindings,
            CriticalFindings = criticalFindings,
            MajorFindings = majorFindings,
            ModerateFindings = moderateFindings,
            MinorFindings = minorFindings,
            FindingsPerInspection = findingsPerInspection,
            ByStatus = byStatus,
            ByType = byType,
            ByDepartment = byDepartment,
            MonthlyTrends = monthlyTrends
        };
    }
}