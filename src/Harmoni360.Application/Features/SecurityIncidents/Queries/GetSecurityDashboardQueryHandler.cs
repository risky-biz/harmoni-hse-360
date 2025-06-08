using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.SecurityIncidents.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.SecurityIncidents.Queries;

public class GetSecurityDashboardQueryHandler : IRequestHandler<GetSecurityDashboardQuery, SecurityDashboardDto>
{
    private readonly IApplicationDbContext _context;

    public GetSecurityDashboardQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SecurityDashboardDto> Handle(GetSecurityDashboardQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-30);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        var dashboardDto = new SecurityDashboardDto();

        if (request.IncludeMetrics)
        {
            dashboardDto.Metrics = await GetSecurityMetrics(startDate, endDate, cancellationToken);
        }

        if (request.IncludeTrends)
        {
            dashboardDto.Trends = await GetSecurityTrends(startDate, endDate, cancellationToken);
        }

        // Get recent incidents
        dashboardDto.RecentIncidents = await GetRecentIncidents(10, cancellationToken);

        // Get critical incidents
        dashboardDto.CriticalIncidents = await GetCriticalIncidents(cancellationToken);

        // Get overdue incidents
        dashboardDto.OverdueIncidents = await GetOverdueIncidents(cancellationToken);

        if (request.IncludeThreatIntel)
        {
            dashboardDto.RecentThreatIndicators = await GetRecentThreatIndicators(10, cancellationToken);
        }

        dashboardDto.ComplianceStatus = await GetComplianceStatus(cancellationToken);

        return dashboardDto;
    }

    private async Task<SecurityMetricsDto> GetSecurityMetrics(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var incidents = await _context.SecurityIncidents
            .Where(i => i.IncidentDateTime >= startDate && i.IncidentDateTime <= endDate)
            .ToListAsync(cancellationToken);

        var previousPeriodStart = startDate.AddDays(-(endDate - startDate).Days);
        var previousPeriodIncidents = await _context.SecurityIncidents
            .Where(i => i.IncidentDateTime >= previousPeriodStart && i.IncidentDateTime < startDate)
            .CountAsync(cancellationToken);

        var resolvedIncidents = incidents.Where(i => i.Status == SecurityIncidentStatus.Closed).ToList();
        var averageResolutionTime = resolvedIncidents.Any() 
            ? resolvedIncidents.Average(i => i.ResolutionDateTime.HasValue 
                ? (i.ResolutionDateTime.Value - i.CreatedAt).TotalHours 
                : 0) 
            : 0;

        var incidentTrend = previousPeriodIncidents > 0 
            ? ((double)(incidents.Count - previousPeriodIncidents) / previousPeriodIncidents) * 100 
            : 0;

        var locationGroups = incidents.GroupBy(i => i.Location).OrderByDescending(g => g.Count()).FirstOrDefault();

        return new SecurityMetricsDto
        {
            TotalIncidents = incidents.Count,
            OpenIncidents = incidents.Count(i => i.Status != SecurityIncidentStatus.Closed),
            ClosedIncidents = incidents.Count(i => i.Status == SecurityIncidentStatus.Closed),
            CriticalIncidents = incidents.Count(i => i.Severity == SecuritySeverity.Critical),
            HighSeverityIncidents = incidents.Count(i => i.Severity == SecuritySeverity.High),
            DataBreachIncidents = incidents.Count(i => i.DataBreachOccurred),
            InternalThreatIncidents = incidents.Count(i => i.IsInternalThreat),
            PhysicalSecurityIncidents = incidents.Count(i => i.IncidentType == SecurityIncidentType.PhysicalSecurity),
            CybersecurityIncidents = incidents.Count(i => i.IncidentType == SecurityIncidentType.Cybersecurity),
            PersonnelSecurityIncidents = incidents.Count(i => i.IncidentType == SecurityIncidentType.PersonnelSecurity),
            InformationSecurityIncidents = incidents.Count(i => i.IncidentType == SecurityIncidentType.InformationSecurity),
            OverdueIncidents = incidents.Count(i => IsOverdue(i)),
            AverageResolutionTimeHours = averageResolutionTime,
            IncidentTrend = incidentTrend,
            MostCommonSeverity = incidents.GroupBy(i => i.Severity).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? SecuritySeverity.Low,
            MostCommonType = incidents.GroupBy(i => i.IncidentType).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? SecurityIncidentType.PhysicalSecurity,
            MostCommonLocation = locationGroups?.Key ?? string.Empty
        };
    }

    private async Task<SecurityTrendsDto> GetSecurityTrends(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var incidents = await _context.SecurityIncidents
            .Where(i => i.IncidentDateTime >= startDate && i.IncidentDateTime <= endDate)
            .ToListAsync(cancellationToken);

        var trends = new SecurityTrendsDto();

        // Daily incident trends
        trends.IncidentTrends = incidents
            .GroupBy(i => i.IncidentDateTime.Date)
            .Select(g => new SecurityTrendDataPoint
            {
                Date = g.Key,
                Label = g.Key.ToString("yyyy-MM-dd"),
                Count = g.Count(),
                Percentage = incidents.Count > 0 ? (double)g.Count() / incidents.Count * 100 : 0
            })
            .OrderBy(t => t.Date)
            .ToList();

        // Severity trends
        trends.SeverityTrends = incidents
            .GroupBy(i => i.Severity)
            .Select(g => new SecurityTrendDataPoint
            {
                Date = DateTime.UtcNow,
                Label = g.Key.ToString(),
                Count = g.Count(),
                Percentage = incidents.Count > 0 ? (double)g.Count() / incidents.Count * 100 : 0
            })
            .ToList();

        // Type trends
        trends.TypeTrends = incidents
            .GroupBy(i => i.IncidentType)
            .Select(g => new SecurityTrendDataPoint
            {
                Date = DateTime.UtcNow,
                Label = g.Key.ToString(),
                Count = g.Count(),
                Percentage = incidents.Count > 0 ? (double)g.Count() / incidents.Count * 100 : 0
            })
            .ToList();

        // Location trends
        trends.LocationTrends = incidents
            .GroupBy(i => i.Location)
            .Select(g => new SecurityLocationTrendDto
            {
                Location = g.Key,
                IncidentCount = g.Count(),
                HighestSeverity = g.Max(i => i.Severity),
                MostCommonType = g.GroupBy(i => i.IncidentType).OrderByDescending(tg => tg.Count()).First().Key,
                Latitude = g.FirstOrDefault()?.GeoLocation?.Latitude ?? 0,
                Longitude = g.FirstOrDefault()?.GeoLocation?.Longitude ?? 0
            })
            .OrderByDescending(l => l.IncidentCount)
            .Take(10)
            .ToList();

        return trends;
    }

    private async Task<List<SecurityIncidentListDto>> GetRecentIncidents(int count, CancellationToken cancellationToken)
    {
        return await _context.SecurityIncidents
            .Include(i => i.Reporter)
            .Include(i => i.AssignedTo)
            .OrderByDescending(i => i.CreatedAt)
            .Take(count)
            .Select(i => new SecurityIncidentListDto
            {
                Id = i.Id,
                IncidentNumber = i.IncidentNumber,
                Title = i.Title,
                IncidentType = i.IncidentType,
                Category = i.Category,
                Severity = i.Severity,
                Status = i.Status,
                ThreatLevel = i.ThreatLevel,
                IncidentDateTime = i.IncidentDateTime,
                Location = i.Location,
                ReporterName = i.Reporter.Name,
                AssignedToName = i.AssignedTo != null ? i.AssignedTo.Name : null,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<SecurityIncidentListDto>> GetCriticalIncidents(CancellationToken cancellationToken)
    {
        return await _context.SecurityIncidents
            .Include(i => i.Reporter)
            .Include(i => i.AssignedTo)
            .Where(i => i.Severity == SecuritySeverity.Critical && i.Status != SecurityIncidentStatus.Closed)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new SecurityIncidentListDto
            {
                Id = i.Id,
                IncidentNumber = i.IncidentNumber,
                Title = i.Title,
                IncidentType = i.IncidentType,
                Category = i.Category,
                Severity = i.Severity,
                Status = i.Status,
                ThreatLevel = i.ThreatLevel,
                IncidentDateTime = i.IncidentDateTime,
                Location = i.Location,
                ReporterName = i.Reporter.Name,
                AssignedToName = i.AssignedTo != null ? i.AssignedTo.Name : null,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<SecurityIncidentListDto>> GetOverdueIncidents(CancellationToken cancellationToken)
    {
        var cutoffDates = new Dictionary<SecuritySeverity, DateTime>
        {
            { SecuritySeverity.Critical, DateTime.UtcNow.AddDays(-1) },
            { SecuritySeverity.High, DateTime.UtcNow.AddDays(-3) },
            { SecuritySeverity.Medium, DateTime.UtcNow.AddDays(-7) },
            { SecuritySeverity.Low, DateTime.UtcNow.AddDays(-14) }
        };

        return await _context.SecurityIncidents
            .Include(i => i.Reporter)
            .Include(i => i.AssignedTo)
            .Where(i => 
                i.Status != SecurityIncidentStatus.Closed &&
                ((i.Severity == SecuritySeverity.Critical && i.CreatedAt < cutoffDates[SecuritySeverity.Critical]) ||
                 (i.Severity == SecuritySeverity.High && i.CreatedAt < cutoffDates[SecuritySeverity.High]) ||
                 (i.Severity == SecuritySeverity.Medium && i.CreatedAt < cutoffDates[SecuritySeverity.Medium]) ||
                 (i.Severity == SecuritySeverity.Low && i.CreatedAt < cutoffDates[SecuritySeverity.Low])))
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new SecurityIncidentListDto
            {
                Id = i.Id,
                IncidentNumber = i.IncidentNumber,
                Title = i.Title,
                IncidentType = i.IncidentType,
                Category = i.Category,
                Severity = i.Severity,
                Status = i.Status,
                ThreatLevel = i.ThreatLevel,
                IncidentDateTime = i.IncidentDateTime,
                Location = i.Location,
                ReporterName = i.Reporter.Name,
                AssignedToName = i.AssignedTo != null ? i.AssignedTo.Name : null,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<ThreatIndicatorDto>> GetRecentThreatIndicators(int count, CancellationToken cancellationToken)
    {
        return await _context.ThreatIndicators
            .Where(ti => ti.IsActive)
            .OrderByDescending(ti => ti.LastSeen)
            .Take(count)
            .Select(ti => new ThreatIndicatorDto
            {
                Id = ti.Id,
                IndicatorType = ti.IndicatorType,
                IndicatorValue = ti.IndicatorValue,
                ThreatType = ti.ThreatType,
                Confidence = ti.Confidence,
                Source = ti.Source,
                FirstSeen = ti.FirstSeen,
                LastSeen = ti.LastSeen,
                IsActive = ti.IsActive,
                Description = ti.Description,
                Tags = ti.Tags
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<SecurityComplianceStatusDto> GetComplianceStatus(CancellationToken cancellationToken)
    {
        // This would typically integrate with compliance assessment logic
        // For now, returning a basic implementation
        return new SecurityComplianceStatusDto
        {
            ISO27001Compliant = true,
            ITELawCompliant = true,
            SMK3Compliant = true,
            ComplianceScore = 85,
            Issues = new List<SecurityComplianceIssueDto>(),
            LastAssessmentDate = DateTime.UtcNow.AddDays(-30),
            NextAssessmentDue = DateTime.UtcNow.AddDays(60)
        };
    }

    private static bool IsOverdue(Domain.Entities.Security.SecurityIncident incident)
    {
        if (incident.Status == SecurityIncidentStatus.Closed)
            return false;

        var slaThreshold = incident.Severity switch
        {
            SecuritySeverity.Critical => 1,
            SecuritySeverity.High => 3,
            SecuritySeverity.Medium => 7,
            SecuritySeverity.Low => 14,
            _ => 14
        };

        return (DateTime.UtcNow - incident.CreatedAt).Days > slaThreshold;
    }
}