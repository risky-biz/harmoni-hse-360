using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Health.DTOs;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.Health.Queries;

public class GetHealthIncidentTrendsQueryHandler : IRequestHandler<GetHealthIncidentTrendsQuery, HealthIncidentTrendsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetHealthIncidentTrendsQueryHandler> _logger;
    private readonly ICacheService _cache;

    public GetHealthIncidentTrendsQueryHandler(
        IApplicationDbContext context,
        ILogger<GetHealthIncidentTrendsQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<HealthIncidentTrendsDto> Handle(GetHealthIncidentTrendsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting health incident trends for date range: {FromDate} to {ToDate}, Department: {Department}, PersonType: {PersonType}, Period: {Period}",
            request.FromDate?.ToString("yyyy-MM-dd") ?? "All",
            request.ToDate?.ToString("yyyy-MM-dd") ?? "All",
            request.Department ?? "All",
            request.PersonType?.ToString() ?? "All",
            request.Period);

        // Set default date range based on period
        var fromDate = request.FromDate ?? request.Period switch
        {
            TrendPeriod.Daily => DateTime.UtcNow.AddDays(-30),
            TrendPeriod.Weekly => DateTime.UtcNow.AddDays(-84), // 12 weeks
            TrendPeriod.Monthly => DateTime.UtcNow.AddMonths(-12),
            TrendPeriod.Quarterly => DateTime.UtcNow.AddYears(-2),
            _ => DateTime.UtcNow.AddMonths(-12)
        };
        var toDate = request.ToDate ?? DateTime.UtcNow;

        // Try cache first
        var cacheKey = $"health-incident-trends-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}-{request.Department ?? "all"}-{request.PersonType?.ToString() ?? "all"}-{request.IncidentType?.ToString() ?? "all"}-{request.Severity?.ToString() ?? "all"}-{request.Period}-{request.IncludeResolved}";
        var cachedResult = await _cache.GetAsync<HealthIncidentTrendsDto>(cacheKey);
        if (cachedResult != null)
        {
            _logger.LogInformation("Health incident trends data found in cache");
            return cachedResult;
        }

        // Build base query for health incidents
        var healthIncidentsQuery = _context.HealthIncidents
            .Include(hi => hi.HealthRecord)
                .ThenInclude(hr => hr.Person)
            .Where(hi => hi.IncidentDateTime >= fromDate && hi.IncidentDateTime <= toDate)
            .AsQueryable();

        if (!request.IncludeResolved)
        {
            healthIncidentsQuery = healthIncidentsQuery.Where(hi => !hi.IsResolved);
        }

        if (request.PersonType.HasValue)
        {
            healthIncidentsQuery = healthIncidentsQuery.Where(hi => hi.HealthRecord.PersonType == request.PersonType.Value);
        }

        if (!string.IsNullOrEmpty(request.Department))
        {
            healthIncidentsQuery = healthIncidentsQuery.Where(hi =>
                hi.HealthRecord.Person.Department != null &&
                hi.HealthRecord.Person.Department.ToLower().Contains(request.Department.ToLower()));
        }

        if (request.IncidentType.HasValue)
        {
            healthIncidentsQuery = healthIncidentsQuery.Where(hi => hi.Type == request.IncidentType.Value);
        }

        if (request.Severity.HasValue)
        {
            healthIncidentsQuery = healthIncidentsQuery.Where(hi => hi.Severity == request.Severity.Value);
        }

        var healthIncidents = await healthIncidentsQuery.ToListAsync(cancellationToken);

        // Calculate overall metrics
        var totalIncidents = healthIncidents.Count;
        var criticalIncidents = healthIncidents.Count(hi => hi.IsCritical());
        var unresolvedIncidents = healthIncidents.Count(hi => !hi.IsResolved);
        var hospitalizations = healthIncidents.Count(hi => hi.RequiredHospitalization);
        var avgResolutionTime = healthIncidents
            .Where(hi => hi.IsResolved && hi.LastModifiedAt.HasValue)
            .Select(hi => (hi.LastModifiedAt!.Value - hi.CreatedAt).TotalHours)
            .DefaultIfEmpty(0)
            .Average();

        // Generate trend data based on period
        var trendData = GenerateTrendData(healthIncidents, request.Period, fromDate, toDate);

        // Incident breakdown by type
        var incidentsByType = healthIncidents
            .GroupBy(hi => hi.Type)
            .Select(g => new IncidentTypeBreakdown
            {
                Type = g.Key.ToString(),
                Count = g.Count(),
                CriticalCount = g.Count(hi => hi.IsCritical()),
                HospitalizationCount = g.Count(hi => hi.RequiredHospitalization),
                UnresolvedCount = g.Count(hi => !hi.IsResolved),
                Percentage = totalIncidents > 0 ? (decimal)g.Count() / totalIncidents * 100 : 0,
                AvgResolutionHours = g.Where(hi => hi.IsResolved && hi.LastModifiedAt.HasValue)
                    .Select(hi => (hi.LastModifiedAt!.Value - hi.CreatedAt).TotalHours)
                    .DefaultIfEmpty(0)
                    .Average()
            })
            .OrderByDescending(i => i.Count)
            .ToList();

        // Incident breakdown by severity
        var incidentsBySeverity = healthIncidents
            .GroupBy(hi => hi.Severity)
            .Select(g => new IncidentSeverityBreakdown
            {
                Severity = g.Key.ToString(),
                Count = g.Count(),
                HospitalizationCount = g.Count(hi => hi.RequiredHospitalization),
                UnresolvedCount = g.Count(hi => !hi.IsResolved),
                Percentage = totalIncidents > 0 ? (decimal)g.Count() / totalIncidents * 100 : 0,
                AvgResolutionHours = g.Where(hi => hi.IsResolved && hi.LastModifiedAt.HasValue)
                    .Select(hi => (hi.LastModifiedAt!.Value - hi.CreatedAt).TotalHours)
                    .DefaultIfEmpty(0)
                    .Average()
            })
            .OrderByDescending(i => i.Count)
            .ToList();

        // Incident breakdown by department
        var incidentsByDepartment = healthIncidents
            .Where(hi => !string.IsNullOrEmpty(hi.HealthRecord.Person.Department))
            .GroupBy(hi => hi.HealthRecord.Person.Department)
            .Select(g => new DepartmentIncidentBreakdown
            {
                Department = g.Key!,
                Count = g.Count(),
                CriticalCount = g.Count(hi => hi.IsCritical()),
                HospitalizationCount = g.Count(hi => hi.RequiredHospitalization),
                UnresolvedCount = g.Count(hi => !hi.IsResolved),
                Percentage = totalIncidents > 0 ? (decimal)g.Count() / totalIncidents * 100 : 0,
                StudentIncidents = g.Count(hi => hi.HealthRecord.PersonType == PersonType.Student),
                StaffIncidents = g.Count(hi => hi.HealthRecord.PersonType == PersonType.Staff)
            })
            .OrderByDescending(d => d.Count)
            .ToList();

        // Time-based analysis
        var peakHours = healthIncidents
            .GroupBy(hi => hi.IncidentDateTime.Hour)
            .Select(g => new HourlyIncidentPattern
            {
                Hour = g.Key,
                Count = g.Count(),
                CriticalCount = g.Count(hi => hi.IsCritical()),
                Percentage = totalIncidents > 0 ? (decimal)g.Count() / totalIncidents * 100 : 0
            })
            .OrderByDescending(h => h.Count)
            .ToList();

        var dayOfWeekPattern = healthIncidents
            .GroupBy(hi => hi.IncidentDateTime.DayOfWeek)
            .Select(g => new DayOfWeekIncidentPattern
            {
                DayOfWeek = g.Key.ToString(),
                Count = g.Count(),
                CriticalCount = g.Count(hi => hi.IsCritical()),
                Percentage = totalIncidents > 0 ? (decimal)g.Count() / totalIncidents * 100 : 0
            })
            .OrderByDescending(d => d.Count)
            .ToList();

        // Recent high-priority incidents requiring attention
        var recentCriticalIncidents = healthIncidents
            .Where(hi => hi.IsCritical() || !hi.IsResolved)
            .OrderByDescending(hi => hi.IncidentDateTime)
            .Take(10)
            .Select(hi => new CriticalIncidentSummary
            {
                Id = hi.Id,
                IncidentId = hi.IncidentId,
                PersonName = hi.HealthRecord.Person.Name,
                PersonType = hi.HealthRecord.PersonType.ToString(),
                Department = hi.HealthRecord.Person.Department,
                Type = hi.Type.ToString(),
                Severity = hi.Severity.ToString(),
                IncidentDateTime = hi.IncidentDateTime,
                IsCritical = hi.IsCritical(),
                IsResolved = hi.IsResolved,
                RequiredHospitalization = hi.RequiredHospitalization,
                DaysOpen = (DateTime.UtcNow - hi.IncidentDateTime).Days,
                IsOverdue = hi.IsOverdue()
            })
            .ToList();

        // Calculate risk indicators
        var riskIndicators = new HealthIncidentRiskIndicators
        {
            IncidentRateIncrease = CalculateIncidentRateChange(healthIncidents, request.Period),
            CriticalIncidentRate = totalIncidents > 0 ? (decimal)criticalIncidents / totalIncidents * 100 : 0,
            HospitalizationRate = totalIncidents > 0 ? (decimal)hospitalizations / totalIncidents * 100 : 0,
            UnresolvedIncidentRate = totalIncidents > 0 ? (decimal)unresolvedIncidents / totalIncidents * 100 : 0,
            AvgResolutionTimeHours = avgResolutionTime,
            HighRiskDepartments = incidentsByDepartment
                .Where(d => d.Count > totalIncidents * 0.15m) // Departments with >15% of incidents
                .Select(d => d.Department)
                .ToList(),
            PeakIncidentHours = peakHours
                .Where(h => h.Count > totalIncidents * 0.1m) // Hours with >10% of incidents
                .Select(h => h.Hour)
                .ToList()
        };

        var result = new HealthIncidentTrendsDto
        {
            TotalIncidents = totalIncidents,
            CriticalIncidents = criticalIncidents,
            UnresolvedIncidents = unresolvedIncidents,
            HospitalizationCount = hospitalizations,
            AvgResolutionTimeHours = avgResolutionTime,
            
            TrendData = trendData,
            IncidentsByType = incidentsByType,
            IncidentsBySeverity = incidentsBySeverity,
            IncidentsByDepartment = incidentsByDepartment,
            
            PeakHours = peakHours,
            DayOfWeekPattern = dayOfWeekPattern,
            RecentCriticalIncidents = recentCriticalIncidents,
            RiskIndicators = riskIndicators,
            
            Period = request.Period.ToString(),
            FromDate = fromDate,
            ToDate = toDate,
            GeneratedAt = DateTime.UtcNow
        };

        // Cache for 30 minutes
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30), new[] { "health", "health-incident-trends" });

        _logger.LogInformation("Health incident trends analysis completed. Total incidents: {TotalIncidents}, Critical: {CriticalIncidents}, Unresolved: {UnresolvedIncidents}",
            totalIncidents, criticalIncidents, unresolvedIncidents);

        return result;
    }

    private List<HealthIncidentTrendDataPoint> GenerateTrendData(List<HealthIncident> incidents, TrendPeriod period, DateTime fromDate, DateTime toDate)
    {
        return period switch
        {
            TrendPeriod.Daily => GenerateDailyTrends(incidents, fromDate, toDate),
            TrendPeriod.Weekly => GenerateWeeklyTrends(incidents, fromDate, toDate),
            TrendPeriod.Monthly => GenerateMonthlyTrends(incidents, fromDate, toDate),
            TrendPeriod.Quarterly => GenerateQuarterlyTrends(incidents, fromDate, toDate),
            _ => GenerateMonthlyTrends(incidents, fromDate, toDate)
        };
    }

    private List<HealthIncidentTrendDataPoint> GenerateDailyTrends(List<HealthIncident> incidents, DateTime fromDate, DateTime toDate)
    {
        var totalDays = (toDate - fromDate).Days + 1;
        return Enumerable.Range(0, totalDays)
            .Select(i =>
            {
                var date = fromDate.AddDays(i).Date;
                var dayIncidents = incidents.Where(hi => hi.IncidentDateTime.Date == date).ToList();

                return new HealthIncidentTrendDataPoint
                {
                    Date = date,
                    Count = dayIncidents.Count,
                    CriticalCount = dayIncidents.Count(hi => hi.IsCritical()),
                    HospitalizationCount = dayIncidents.Count(hi => hi.RequiredHospitalization),
                    UnresolvedCount = dayIncidents.Count(hi => !hi.IsResolved)
                };
            })
            .OrderBy(t => t.Date)
            .ToList();
    }

    private List<HealthIncidentTrendDataPoint> GenerateWeeklyTrends(List<HealthIncident> incidents, DateTime fromDate, DateTime toDate)
    {
        var totalWeeks = (int)Math.Ceiling((toDate - fromDate).TotalDays / 7);
        return Enumerable.Range(0, totalWeeks)
            .Select(i =>
            {
                var weekStart = fromDate.AddDays(i * 7).Date;
                var weekEnd = weekStart.AddDays(6);
                var weekIncidents = incidents.Where(hi => hi.IncidentDateTime.Date >= weekStart && hi.IncidentDateTime.Date <= weekEnd).ToList();

                return new HealthIncidentTrendDataPoint
                {
                    Date = weekStart,
                    Count = weekIncidents.Count,
                    CriticalCount = weekIncidents.Count(hi => hi.IsCritical()),
                    HospitalizationCount = weekIncidents.Count(hi => hi.RequiredHospitalization),
                    UnresolvedCount = weekIncidents.Count(hi => !hi.IsResolved)
                };
            })
            .OrderBy(t => t.Date)
            .ToList();
    }

    private List<HealthIncidentTrendDataPoint> GenerateMonthlyTrends(List<HealthIncident> incidents, DateTime fromDate, DateTime toDate)
    {
        var totalMonths = ((toDate.Year - fromDate.Year) * 12) + toDate.Month - fromDate.Month + 1;
        return Enumerable.Range(0, totalMonths)
            .Select(i =>
            {
                var monthStart = new DateTime(fromDate.Year, fromDate.Month, 1).AddMonths(i);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var monthIncidents = incidents.Where(hi => hi.IncidentDateTime >= monthStart && hi.IncidentDateTime <= monthEnd).ToList();

                return new HealthIncidentTrendDataPoint
                {
                    Date = monthStart,
                    Count = monthIncidents.Count,
                    CriticalCount = monthIncidents.Count(hi => hi.IsCritical()),
                    HospitalizationCount = monthIncidents.Count(hi => hi.RequiredHospitalization),
                    UnresolvedCount = monthIncidents.Count(hi => !hi.IsResolved)
                };
            })
            .OrderBy(t => t.Date)
            .ToList();
    }

    private List<HealthIncidentTrendDataPoint> GenerateQuarterlyTrends(List<HealthIncident> incidents, DateTime fromDate, DateTime toDate)
    {
        var totalQuarters = ((toDate.Year - fromDate.Year) * 4) + ((toDate.Month - 1) / 3) - ((fromDate.Month - 1) / 3) + 1;
        return Enumerable.Range(0, totalQuarters)
            .Select(i =>
            {
                var quarterStart = new DateTime(fromDate.Year, ((fromDate.Month - 1) / 3) * 3 + 1, 1).AddMonths(i * 3);
                var quarterEnd = quarterStart.AddMonths(3).AddDays(-1);
                var quarterIncidents = incidents.Where(hi => hi.IncidentDateTime >= quarterStart && hi.IncidentDateTime <= quarterEnd).ToList();

                return new HealthIncidentTrendDataPoint
                {
                    Date = quarterStart,
                    Count = quarterIncidents.Count,
                    CriticalCount = quarterIncidents.Count(hi => hi.IsCritical()),
                    HospitalizationCount = quarterIncidents.Count(hi => hi.RequiredHospitalization),
                    UnresolvedCount = quarterIncidents.Count(hi => !hi.IsResolved)
                };
            })
            .OrderBy(t => t.Date)
            .ToList();
    }

    private decimal CalculateIncidentRateChange(List<HealthIncident> incidents, TrendPeriod period)
    {
        var cutoffDate = period switch
        {
            TrendPeriod.Daily => DateTime.UtcNow.AddDays(-7),
            TrendPeriod.Weekly => DateTime.UtcNow.AddDays(-14),
            TrendPeriod.Monthly => DateTime.UtcNow.AddMonths(-1),
            TrendPeriod.Quarterly => DateTime.UtcNow.AddMonths(-3),
            _ => DateTime.UtcNow.AddMonths(-1)
        };

        var recentIncidents = incidents.Count(hi => hi.IncidentDateTime >= cutoffDate);
        var previousIncidents = incidents.Count(hi => hi.IncidentDateTime < cutoffDate);

        if (previousIncidents == 0) return recentIncidents > 0 ? 100 : 0;
        return ((decimal)recentIncidents - previousIncidents) / previousIncidents * 100;
    }
}