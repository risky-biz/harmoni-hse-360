using MediatR;
using Microsoft.EntityFrameworkCore;
using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Application.Features.Hazards.DTOs;
using HarmoniHSE360.Domain.Entities;

namespace HarmoniHSE360.Application.Features.Hazards.Queries;

public class GetHazardDashboardQueryHandler : IRequestHandler<GetHazardDashboardQuery, HazardDashboardDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public GetHazardDashboardQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<HazardDashboardDto> Handle(GetHazardDashboardQuery request, CancellationToken cancellationToken)
    {
        // Determine date range and ensure UTC
        var dateFrom = (request.DateFrom ?? DateTime.UtcNow.AddMonths(-12));
        var dateTo = (request.DateTo ?? DateTime.UtcNow);
        
        // Ensure dates are in UTC for PostgreSQL compatibility
        if (dateFrom.Kind == DateTimeKind.Unspecified)
            dateFrom = DateTime.SpecifyKind(dateFrom, DateTimeKind.Utc);
        else if (dateFrom.Kind == DateTimeKind.Local)
            dateFrom = dateFrom.ToUniversalTime();
            
        if (dateTo.Kind == DateTimeKind.Unspecified)
            dateTo = DateTime.SpecifyKind(dateTo, DateTimeKind.Utc);
        else if (dateTo.Kind == DateTimeKind.Local)
            dateTo = dateTo.ToUniversalTime();

        // Check cache for dashboard data
        var cacheKey = GenerateCacheKey(request, dateFrom, dateTo);
        var cachedDashboard = await _cacheService.GetAsync<HazardDashboardDto>(cacheKey);
        
        if (cachedDashboard != null)
        {
            return cachedDashboard;
        }

        // Build base query with department filter if specified
        var baseQuery = _context.Hazards.AsQueryable();
        
        if (!string.IsNullOrEmpty(request.Department))
        {
            baseQuery = baseQuery.Where(h => h.ReporterDepartment == request.Department);
        }

        // Apply personalized filter if requested
        if (request.PersonalizedView)
        {
            var currentUserId = _currentUserService.UserId;
            
            if (currentUserId > 0)
            {
                baseQuery = baseQuery.Where(h => 
                    h.ReporterId == currentUserId ||
                    h.MitigationActions.Any(ma => ma.AssignedToId == currentUserId));
            }
        }

        var dashboard = new HazardDashboardDto();

        // Build overview metrics
        dashboard.Overview = await BuildOverviewMetrics(baseQuery, dateFrom, dateTo, cancellationToken);

        // Build risk metrics
        dashboard.RiskAnalysis = await BuildRiskMetrics(baseQuery, cancellationToken);

        // Build performance metrics
        dashboard.Performance = await BuildPerformanceMetrics(baseQuery, cancellationToken);

        // Build trends if requested
        if (request.IncludeTrends)
        {
            dashboard.Trends = await BuildTrendAnalytics(baseQuery, dateFrom, dateTo, cancellationToken);
        }

        // Build location analytics if requested
        if (request.IncludeLocationAnalytics)
        {
            dashboard.LocationData = await BuildLocationAnalytics(baseQuery, cancellationToken);
        }

        // Build compliance metrics if requested
        if (request.IncludeComplianceMetrics)
        {
            dashboard.Compliance = await BuildComplianceMetrics(baseQuery, cancellationToken);
        }

        // Build recent activities
        dashboard.RecentActivities = await BuildRecentActivities(baseQuery, cancellationToken);

        // Build alerts
        dashboard.Alerts = await BuildAlerts(baseQuery, cancellationToken);

        // Cache the dashboard for 10 minutes
        await _cacheService.SetAsync(cacheKey, dashboard, TimeSpan.FromMinutes(10));

        return dashboard;
    }

    private async Task<HazardOverviewMetrics> BuildOverviewMetrics(
        IQueryable<Hazard> baseQuery, 
        DateTime dateFrom, 
        DateTime dateTo, 
        CancellationToken cancellationToken)
    {
        var currentPeriodQuery = baseQuery.Where(h => h.IdentifiedDate >= dateFrom && h.IdentifiedDate <= dateTo);
        var previousPeriodStart = dateFrom.AddDays(-(dateTo - dateFrom).TotalDays);
        var previousPeriodQuery = baseQuery.Where(h => h.IdentifiedDate >= previousPeriodStart && h.IdentifiedDate < dateFrom);

        var metrics = new HazardOverviewMetrics
        {
            TotalHazards = await baseQuery.CountAsync(cancellationToken),
            OpenHazards = await baseQuery.CountAsync(h => 
                h.Status != HazardStatus.Resolved && h.Status != HazardStatus.Closed, cancellationToken),
            ResolvedHazards = await baseQuery.CountAsync(h => 
                h.Status == HazardStatus.Resolved || h.Status == HazardStatus.Closed, cancellationToken),
            HighRiskHazards = await baseQuery.CountAsync(h => 
                h.CurrentRiskAssessment != null && h.CurrentRiskAssessment.RiskLevel == RiskLevel.High, cancellationToken),
            CriticalRiskHazards = await baseQuery.CountAsync(h => 
                h.CurrentRiskAssessment != null && h.CurrentRiskAssessment.RiskLevel == RiskLevel.Critical, cancellationToken),
            UnassessedHazards = await baseQuery.CountAsync(h => h.CurrentRiskAssessment == null, cancellationToken),
            NewHazardsThisMonth = await baseQuery.CountAsync(h => 
                h.IdentifiedDate >= DateTime.UtcNow.AddDays(-30), cancellationToken)
        };

        // Get overdue actions count
        metrics.OverdueActions = await _context.HazardMitigationActions
            .Where(ma => baseQuery.Any(h => h.Id == ma.HazardId) &&
                        ma.TargetDate < DateTime.UtcNow &&
                        ma.Status != MitigationActionStatus.Completed)
            .CountAsync(cancellationToken);

        // Calculate percentage changes
        var currentPeriodTotal = await currentPeriodQuery.CountAsync(cancellationToken);
        var previousPeriodTotal = await previousPeriodQuery.CountAsync(cancellationToken);
        
        if (previousPeriodTotal > 0)
        {
            metrics.TotalHazardsChange = ((double)(currentPeriodTotal - previousPeriodTotal) / previousPeriodTotal) * 100;
        }

        return metrics;
    }

    private async Task<RiskMetrics> BuildRiskMetrics(IQueryable<Hazard> baseQuery, CancellationToken cancellationToken)
    {
        var hazardsWithRisk = baseQuery.Where(h => h.CurrentRiskAssessment != null);

        var riskMetrics = new RiskMetrics
        {
            RiskAssessmentsCompleted = await hazardsWithRisk.CountAsync(cancellationToken),
            RiskAssessmentsPending = await baseQuery.CountAsync(h => h.CurrentRiskAssessment == null, cancellationToken)
        };

        var totalHazards = await baseQuery.CountAsync(cancellationToken);
        if (totalHazards > 0)
        {
            riskMetrics.RiskAssessmentCompletionRate = (double)riskMetrics.RiskAssessmentsCompleted / totalHazards * 100;
        }

        // Risk level distribution
        riskMetrics.RiskLevelDistribution = await hazardsWithRisk
            .GroupBy(h => h.CurrentRiskAssessment!.RiskLevel)
            .Select(g => new { Level = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Level, x => x.Count, cancellationToken);

        // Category distribution
        riskMetrics.CategoryDistribution = await baseQuery
            .GroupBy(h => h.Category)
            .Select(g => new { Category = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Category, x => x.Count, cancellationToken);

        // Severity distribution
        riskMetrics.SeverityDistribution = await baseQuery
            .GroupBy(h => h.Severity)
            .Select(g => new { Severity = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Severity, x => x.Count, cancellationToken);

        // Average risk score
        var riskScores = await hazardsWithRisk
            .Select(h => h.CurrentRiskAssessment!.RiskScore)
            .ToListAsync(cancellationToken);
        
        if (riskScores.Any())
        {
            riskMetrics.AverageRiskScore = riskScores.Average();
        }

        return riskMetrics;
    }

    private async Task<PerformanceMetrics> BuildPerformanceMetrics(IQueryable<Hazard> baseQuery, CancellationToken cancellationToken)
    {
        var mitigationActions = _context.HazardMitigationActions
            .Where(ma => baseQuery.Any(h => h.Id == ma.HazardId));

        var performanceMetrics = new PerformanceMetrics
        {
            TotalMitigationActions = await mitigationActions.CountAsync(cancellationToken),
            CompletedMitigationActions = await mitigationActions
                .CountAsync(ma => ma.Status == MitigationActionStatus.Completed, cancellationToken),
            OverdueMitigationActions = await mitigationActions
                .CountAsync(ma => ma.TargetDate < DateTime.UtcNow && 
                                 ma.Status != MitigationActionStatus.Completed, cancellationToken)
        };

        if (performanceMetrics.TotalMitigationActions > 0)
        {
            performanceMetrics.MitigationActionCompletionRate = 
                (double)performanceMetrics.CompletedMitigationActions / performanceMetrics.TotalMitigationActions * 100;
        }

        // Average resolution time for resolved hazards
        var resolvedHazards = await baseQuery
            .Where(h => h.Status == HazardStatus.Resolved || h.Status == HazardStatus.Closed)
            .Select(h => new { h.IdentifiedDate, h.LastModifiedAt })
            .ToListAsync(cancellationToken);

        if (resolvedHazards.Any())
        {
            var resolutionTimes = resolvedHazards
                .Where(h => h.LastModifiedAt.HasValue)
                .Select(h => (h.LastModifiedAt!.Value - h.IdentifiedDate).TotalDays);
            
            if (resolutionTimes.Any())
            {
                performanceMetrics.AverageResolutionTime = resolutionTimes.Average();
            }
        }

        // Action type distribution
        performanceMetrics.ActionTypeDistribution = await mitigationActions
            .GroupBy(ma => ma.Type)
            .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count, cancellationToken);

        // Average effectiveness rating
        var effectivenessRatings = await mitigationActions
            .Where(ma => ma.EffectivenessRating.HasValue)
            .Select(ma => ma.EffectivenessRating!.Value)
            .ToListAsync(cancellationToken);

        if (effectivenessRatings.Any())
        {
            performanceMetrics.AverageActionEffectiveness = effectivenessRatings.Average();
        }

        return performanceMetrics;
    }

    private async Task<TrendAnalytics> BuildTrendAnalytics(
        IQueryable<Hazard> baseQuery, 
        DateTime dateFrom, 
        DateTime dateTo, 
        CancellationToken cancellationToken)
    {
        var trendAnalytics = new TrendAnalytics();

        // Monthly hazard reporting trend
        var monthlyData = await baseQuery
            .Where(h => h.IdentifiedDate >= dateFrom && h.IdentifiedDate <= dateTo)
            .GroupBy(h => new { Year = h.IdentifiedDate.Year, Month = h.IdentifiedDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync(cancellationToken);

        trendAnalytics.HazardReportingTrend = monthlyData.Select(m => new DataPointDto
        {
            Label = $"{m.Year}-{m.Month:D2}",
            Value = m.Count
        }).ToList();

        // Simple trend direction calculation
        if (trendAnalytics.HazardReportingTrend.Count >= 2)
        {
            var firstHalf = trendAnalytics.HazardReportingTrend.Take(trendAnalytics.HazardReportingTrend.Count / 2);
            var secondHalf = trendAnalytics.HazardReportingTrend.Skip(trendAnalytics.HazardReportingTrend.Count / 2);
            
            var firstHalfAvg = firstHalf.Average(d => d.Value);
            var secondHalfAvg = secondHalf.Average(d => d.Value);
            
            if (secondHalfAvg > firstHalfAvg * 1.1)
            {
                trendAnalytics.TrendDirection = "Increasing";
                trendAnalytics.KeyInsights.Add("Hazard reporting has increased significantly in recent months");
            }
            else if (secondHalfAvg < firstHalfAvg * 0.9)
            {
                trendAnalytics.TrendDirection = "Decreasing";
                trendAnalytics.KeyInsights.Add("Hazard reporting has decreased in recent months");
            }
            else
            {
                trendAnalytics.TrendDirection = "Stable";
                trendAnalytics.KeyInsights.Add("Hazard reporting levels remain consistent");
            }
        }

        return trendAnalytics;
    }

    private async Task<LocationAnalytics> BuildLocationAnalytics(IQueryable<Hazard> baseQuery, CancellationToken cancellationToken)
    {
        var locationAnalytics = new LocationAnalytics();

        // Department distribution
        locationAnalytics.DepartmentDistribution = await baseQuery
            .GroupBy(h => h.ReporterDepartment)
            .Select(g => new { Department = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Department, x => x.Count, cancellationToken);

        // Hotspot locations
        locationAnalytics.HotspotLocations = await baseQuery
            .GroupBy(h => new { h.Location, h.ReporterDepartment })
            .Select(g => new HazardLocationDto
            {
                Location = g.Key.Location,
                Department = g.Key.ReporterDepartment,
                HazardCount = g.Count(),
                HighRiskCount = g.Count(h => h.CurrentRiskAssessment != null && 
                                           (h.CurrentRiskAssessment.RiskLevel == RiskLevel.High || 
                                            h.CurrentRiskAssessment.RiskLevel == RiskLevel.Critical))
            })
            .OrderByDescending(l => l.HazardCount)
            .Take(10)
            .ToListAsync(cancellationToken);

        // Most affected area
        if (locationAnalytics.HotspotLocations.Any())
        {
            locationAnalytics.MostAffectedArea = locationAnalytics.HotspotLocations.First().Location;
        }

        locationAnalytics.LocationsWithHazards = await baseQuery
            .Select(h => h.Location)
            .Distinct()
            .CountAsync(cancellationToken);

        return locationAnalytics;
    }

    private async Task<ComplianceMetrics> BuildComplianceMetrics(IQueryable<Hazard> baseQuery, CancellationToken cancellationToken)
    {
        var complianceMetrics = new ComplianceMetrics();

        var totalHazards = await baseQuery.CountAsync(cancellationToken);
        var assessedHazards = await baseQuery.CountAsync(h => h.CurrentRiskAssessment != null, cancellationToken);
        var resolvedWithinSLA = await baseQuery.CountAsync(h => 
            h.ExpectedResolutionDate.HasValue &&
            h.LastModifiedAt.HasValue &&
            h.LastModifiedAt <= h.ExpectedResolutionDate, cancellationToken);

        if (totalHazards > 0)
        {
            var assessmentCompliance = (double)assessedHazards / totalHazards * 100;
            var resolutionCompliance = totalHazards > 0 ? (double)resolvedWithinSLA / totalHazards * 100 : 100;
            
            complianceMetrics.OverallComplianceScore = (assessmentCompliance + resolutionCompliance) / 2;
            complianceMetrics.RegulatoryReportingCompliance = assessmentCompliance;
        }

        // Identify non-compliance areas
        if (complianceMetrics.OverallComplianceScore < 80)
        {
            complianceMetrics.NonComplianceAreas.Add("Risk Assessment Completion");
        }
        
        var overdueCount = await _context.HazardMitigationActions
            .Where(ma => baseQuery.Any(h => h.Id == ma.HazardId) &&
                        ma.TargetDate < DateTime.UtcNow &&
                        ma.Status != MitigationActionStatus.Completed)
            .CountAsync(cancellationToken);
            
        if (overdueCount > 0)
        {
            complianceMetrics.NonComplianceAreas.Add("Overdue Mitigation Actions");
            complianceMetrics.ComplianceViolations = overdueCount;
        }

        return complianceMetrics;
    }

    private async Task<List<RecentActivityDto>> BuildRecentActivities(IQueryable<Hazard> baseQuery, CancellationToken cancellationToken)
    {
        var recentActivities = new List<RecentActivityDto>();

        // Recent hazard reports
        var recentHazards = await baseQuery
            .OrderByDescending(h => h.CreatedAt)
            .Take(5)
            .Select(h => new RecentActivityDto
            {
                ActivityType = "HazardReported",
                Title = $"New hazard reported: {h.Title}",
                Description = $"Hazard reported at {h.Location}",
                Timestamp = h.CreatedAt,
                PerformedBy = h.Reporter.Name,
                Severity = h.Severity.ToString(),
                RelatedEntityId = h.Id
            })
            .ToListAsync(cancellationToken);

        recentActivities.AddRange(recentHazards);

        // Recent risk assessments
        var recentAssessments = await _context.RiskAssessments
            .Where(ra => baseQuery.Any(h => h.Id == ra.HazardId))
            .OrderByDescending(ra => ra.AssessmentDate)
            .Take(5)
            .Select(ra => new RecentActivityDto
            {
                ActivityType = "RiskAssessed",
                Title = $"Risk assessment completed",
                Description = $"Risk level: {ra.RiskLevel}",
                Timestamp = ra.AssessmentDate,
                PerformedBy = ra.Assessor.Name,
                Severity = ra.RiskLevel.ToString(),
                RelatedEntityId = ra.HazardId
            })
            .ToListAsync(cancellationToken);

        recentActivities.AddRange(recentAssessments);

        return recentActivities.OrderByDescending(a => a.Timestamp).Take(10).ToList();
    }

    private async Task<List<HazardAlertDto>> BuildAlerts(IQueryable<Hazard> baseQuery, CancellationToken cancellationToken)
    {
        var alerts = new List<HazardAlertDto>();

        // Critical risk hazards
        var criticalHazards = await baseQuery
            .Where(h => h.CurrentRiskAssessment != null && 
                       h.CurrentRiskAssessment.RiskLevel == RiskLevel.Critical)
            .Select(h => new HazardAlertDto
            {
                AlertType = "HighRisk",
                Title = "Critical Risk Hazard",
                Message = $"Critical risk hazard requires immediate attention: {h.Title}",
                Severity = "Critical",
                CreatedAt = h.CreatedAt,
                HazardId = h.Id,
                HazardTitle = h.Title,
                IsAcknowledged = false
            })
            .ToListAsync(cancellationToken);

        alerts.AddRange(criticalHazards);

        // Overdue actions
        var overdueActions = await _context.HazardMitigationActions
            .Where(ma => baseQuery.Any(h => h.Id == ma.HazardId) &&
                        ma.TargetDate < DateTime.UtcNow &&
                        ma.Status != MitigationActionStatus.Completed)
            .Take(5)
            .Select(ma => new HazardAlertDto
            {
                AlertType = "Overdue",
                Title = "Overdue Mitigation Action",
                Message = $"Action overdue: {ma.ActionDescription}",
                Severity = ma.Priority.ToString(),
                CreatedAt = ma.TargetDate,
                HazardId = ma.HazardId,
                HazardTitle = ma.Hazard.Title,
                IsAcknowledged = false
            })
            .ToListAsync(cancellationToken);

        alerts.AddRange(overdueActions);

        return alerts.OrderByDescending(a => a.CreatedAt).Take(10).ToList();
    }

    private string GenerateCacheKey(GetHazardDashboardQuery request, DateTime dateFrom, DateTime dateTo)
    {
        var keyParts = new List<string>
        {
            "hazard_dashboard",
            dateFrom.ToString("yyyyMMdd"),
            dateTo.ToString("yyyyMMdd"),
            request.Department ?? "all",
            request.PersonalizedView ? _currentUserService.UserId.ToString() : "global"
        };

        return string.Join("_", keyParts);
    }
}