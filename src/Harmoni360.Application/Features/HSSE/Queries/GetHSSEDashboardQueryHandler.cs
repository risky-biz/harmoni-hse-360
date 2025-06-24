using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.HSSE.DTOs;
using Harmoni360.Domain.Enums;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Entities.Waste;

namespace Harmoni360.Application.Features.HSSE.Queries;

public class GetHSSEDashboardQueryHandler : IRequestHandler<GetHSSEDashboardQuery, HSSEDashboardDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetHSSEDashboardQueryHandler> _logger;
    private readonly IHSSECacheService _cacheService;

    public GetHSSEDashboardQueryHandler(
        IApplicationDbContext context,
        ILogger<GetHSSEDashboardQueryHandler> logger,
        IHSSECacheService cacheService)
    {
        _context = context;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<HSSEDashboardDto> Handle(GetHSSEDashboardQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting comprehensive HSSE dashboard data for period {StartDate} to {EndDate}",
            request.StartDate, request.EndDate);

        // Set default date range if not provided
        var endDate = request.EndDate ?? DateTime.UtcNow;
        var startDate = request.StartDate ?? endDate.AddYears(-1);

        // Generate cache key
        var cacheKey = _cacheService.GenerateCacheKey(
            HSSECacheKeys.DashboardPrefix,
            startDate.ToString("yyyy-MM-dd"),
            endDate.ToString("yyyy-MM-dd"),
            request.Department ?? "all",
            request.Location ?? "all",
            request.IncludeTrends,
            request.IncludeComparisons
        );

        // Try to get from cache first
        var cachedDashboard = await _cacheService.GetAsync<HSSEDashboardDto>(cacheKey, cancellationToken);
        if (cachedDashboard != null)
        {
            _logger.LogDebug("Returning cached HSSE dashboard data");
            return cachedDashboard;
        }

        var dashboard = new HSSEDashboardDto();

        try
        {
            // Core HSSE data
            dashboard.HazardStatistics = await GetHazardStatisticsAsync(startDate, endDate, request.Department, cancellationToken);
            dashboard.MonthlyHazards = await GetMonthlyHazardsAsync(startDate, endDate, request.Department, cancellationToken);
            dashboard.HazardClassifications = await GetHazardClassificationsAsync(startDate, endDate, request.Department, cancellationToken);
            dashboard.NonConformanceCriteria = await GetNonConformanceCriteriaAsync(startDate, endDate, request.Department, cancellationToken);
            dashboard.TopUnsafeConditions = await GetTopUnsafeConditionsAsync(startDate, endDate, request.Department, cancellationToken);
            dashboard.ResponsibleActions = await GetResponsibleActionsAsync(startDate, endDate, request.Department, cancellationToken);
            dashboard.HazardCaseStatus = await GetHazardCaseStatusAsync(startDate, endDate, request.Department, cancellationToken);

            // Extended module data integration
            dashboard.PPECompliance = await GetPPEComplianceAsync(startDate, endDate, request.Department, cancellationToken);
            dashboard.TrainingSafety = await GetTrainingSafetyAsync(startDate, endDate, request.Department, cancellationToken);
            dashboard.InspectionSafety = await GetInspectionSafetyAsync(startDate, endDate, request.Department, cancellationToken);
            dashboard.WorkPermitSafety = await GetWorkPermitSafetyAsync(startDate, endDate, request.Department, cancellationToken);
            dashboard.WasteEnvironmental = await GetWasteEnvironmentalAsync(startDate, endDate, request.Department, cancellationToken);
            dashboard.SecurityIncidents = await GetSecurityIncidentsAsync(startDate, endDate, request.Department, cancellationToken);
            dashboard.HealthMonitoring = await GetHealthMonitoringAsync(startDate, endDate, request.Department, cancellationToken);
            dashboard.AuditFindings = await GetAuditFindingsAsync(startDate, endDate, request.Department, cancellationToken);

            // Advanced analytics if requested
            if (request.IncludeTrends)
            {
                dashboard.IncidentFrequencyRates = await GetIncidentFrequencyRatesAsync(startDate, endDate, request.Department, cancellationToken);
                dashboard.SafetyPerformance = await GetSafetyPerformanceAsync(startDate, endDate, request.Department, cancellationToken);
                dashboard.LostTimeInjury = await GetLostTimeInjuryAsync(startDate, endDate, request.Department, cancellationToken);
            }

            // Cache the dashboard data
            await _cacheService.SetAsync(cacheKey, dashboard, cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully retrieved and cached comprehensive HSSE dashboard data");
            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving HSSE dashboard data");
            throw;
        }
    }

    #region Core HSSE Data Methods

    private async Task<HazardStatisticsDto> GetHazardStatisticsAsync(DateTime startDate, DateTime endDate, string? department, CancellationToken cancellationToken)
    {
        var hazards = _context.Hazards.AsQueryable();

        if (!string.IsNullOrEmpty(department))
        {
            hazards = hazards.Where(h => h.Location == department); // Using Location as department field
        }

        hazards = hazards.Where(h => h.CreatedAt >= startDate && h.CreatedAt <= endDate);

        var totalHazards = await hazards.CountAsync(cancellationToken);
        var nearMiss = await hazards.CountAsync(h => h.Category != null && h.Category.Name.ToLower().Contains("near miss"), cancellationToken);
        var accidents = await hazards.CountAsync(h => h.Severity >= HazardSeverity.Major, cancellationToken);
        var openCases = await hazards.CountAsync(h => h.Status != HazardStatus.Closed && h.Status != HazardStatus.Resolved, cancellationToken);
        var closedCases = await hazards.CountAsync(h => h.Status == HazardStatus.Closed || h.Status == HazardStatus.Resolved, cancellationToken);

        return new HazardStatisticsDto
        {
            TotalHazards = totalHazards,
            NearMiss = nearMiss,
            Accidents = accidents,
            OpenCases = openCases,
            ClosedCases = closedCases,
            CompletionRate = totalHazards > 0 ? (decimal)closedCases / totalHazards * 100 : 0
        };
    }

    private async Task<List<MonthlyHazardDto>> GetMonthlyHazardsAsync(DateTime startDate, DateTime endDate, string? department, CancellationToken cancellationToken)
    {
        var hazards = _context.Hazards.AsQueryable();

        if (!string.IsNullOrEmpty(department))
        {
            hazards = hazards.Where(h => h.Location == department);
        }

        var monthlyData = await hazards
            .Where(h => h.CreatedAt >= startDate && h.CreatedAt <= endDate)
            .GroupBy(h => new { h.CreatedAt.Year, h.CreatedAt.Month })
            .Select(g => new MonthlyHazardDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                HazardCount = g.Count(),
                NearnessCount = g.Count(h => h.Category != null && h.Category.Name.ToLower().Contains("near miss")),
                AccidentCount = g.Count(h => h.Severity >= HazardSeverity.Major),
                RiskLevel = g.Count() > 10 ? "High" : g.Count() > 5 ? "Medium" : "Low"
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToListAsync(cancellationToken);

        return monthlyData;
    }

    private async Task<List<HazardClassificationDto>> GetHazardClassificationsAsync(DateTime startDate, DateTime endDate, string? department, CancellationToken cancellationToken)
    {
        var hazards = _context.Hazards.AsQueryable();

        if (!string.IsNullOrEmpty(department))
        {
            hazards = hazards.Where(h => h.Location == department);
        }

        var total = await hazards.CountAsync(h => h.CreatedAt >= startDate && h.CreatedAt <= endDate, cancellationToken);

        var classifications = await hazards
            .Where(h => h.CreatedAt >= startDate && h.CreatedAt <= endDate)
            .GroupBy(h => h.Category != null ? h.Category.Name : "Unknown")
            .Select(g => new HazardClassificationDto
            {
                Type = g.Key,
                Count = g.Count(),
                Percentage = total > 0 ? (decimal)g.Count() / total * 100 : 0,
                Color = GetChartColor(g.Key)
            })
            .OrderByDescending(c => c.Count)
            .ToListAsync(cancellationToken);

        return classifications;
    }

    #endregion

    #region Extended Module Integration

    private async Task<PPEComplianceDto> GetPPEComplianceAsync(DateTime startDate, DateTime endDate, string? department, CancellationToken cancellationToken)
    {
        var ppeItems = _context.PPEItems.AsQueryable();

        if (!string.IsNullOrEmpty(department))
        {
            ppeItems = ppeItems.Where(p => p.AssignedTo != null && p.AssignedTo.Department == department);
        }

        var totalAssignments = await ppeItems.CountAsync(p => p.Status == PPEStatus.Assigned, cancellationToken);
        var compliantAssignments = await ppeItems.CountAsync(p => p.Status == PPEStatus.Assigned && p.Condition != PPECondition.Damaged && p.Condition != PPECondition.Expired, cancellationToken);
        var overdueInspections = await ppeItems.CountAsync(p => p.ExpiryDate < DateTime.UtcNow && p.Status == PPEStatus.Assigned, cancellationToken);

        var categoryCompliance = await ppeItems
            .Where(p => p.Status == PPEStatus.Assigned)
            .GroupBy(p => p.Category != null ? p.Category.Name : "Unknown")
            .Select(g => new PPECategoryComplianceDto
            {
                Category = g.Key,
                TotalAssigned = g.Count(),
                CompliantCount = g.Count(p => p.Condition != PPECondition.Damaged && p.Condition != PPECondition.Expired),
                ComplianceRate = g.Count() > 0 ? (decimal)g.Count(p => p.Condition != PPECondition.Damaged && p.Condition != PPECondition.Expired) / g.Count() * 100 : 0
            })
            .ToListAsync(cancellationToken);

        return new PPEComplianceDto
        {
            TotalPPEAssignments = totalAssignments,
            ComplianceCount = compliantAssignments,
            ComplianceRate = totalAssignments > 0 ? (decimal)compliantAssignments / totalAssignments * 100 : 0,
            OverdueInspections = overdueInspections,
            NonCompliantUsers = totalAssignments - compliantAssignments,
            CategoryCompliance = categoryCompliance
        };
    }

    private async Task<TrainingSafetyDto> GetTrainingSafetyAsync(DateTime startDate, DateTime endDate, string? department, CancellationToken cancellationToken)
    {
        var trainings = _context.Trainings
            .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate);

        if (!string.IsNullOrEmpty(department))
        {
            trainings = trainings.Where(t => t.Participants.Any(p => p.UserDepartment == department));
        }

        // Focus on safety-related trainings - filter by category or title containing safety
        var safetyTrainings = trainings.Where(t => t.Category == TrainingCategory.SafetyTraining || 
                                                  t.Title.ToLower().Contains("safety") ||
                                                  t.Title.ToLower().Contains("hse"));

        var participants = _context.TrainingParticipants
            .Where(tp => safetyTrainings.Any(st => st.Id == tp.TrainingId));

        var totalSafetyTrainings = await participants.CountAsync(cancellationToken);
        var completedTrainings = await participants.CountAsync(tp => tp.Status == ParticipantStatus.Completed, cancellationToken);
        var overdueTrainings = await participants.CountAsync(tp => tp.Status != ParticipantStatus.Completed && tp.Training != null && tp.Training.ScheduledEndDate < DateTime.UtcNow, cancellationToken);

        return new TrainingSafetyDto
        {
            TotalSafetyTrainings = totalSafetyTrainings,
            CompletedTrainings = completedTrainings,
            CompletionRate = totalSafetyTrainings > 0 ? (decimal)completedTrainings / totalSafetyTrainings * 100 : 0,
            OverdueTrainings = overdueTrainings,
            MandatoryTrainings = totalSafetyTrainings, // Assume safety trainings are mandatory
            MandatoryCompleted = completedTrainings,
            MandatoryCompletionRate = totalSafetyTrainings > 0 ? (decimal)completedTrainings / totalSafetyTrainings * 100 : 0
        };
    }

    private async Task<InspectionSafetyDto> GetInspectionSafetyAsync(DateTime startDate, DateTime endDate, string? department, CancellationToken cancellationToken)
    {
        var inspections = _context.Inspections
            .Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate);

        if (!string.IsNullOrEmpty(department))
        {
            inspections = inspections.Where(i => i.Inspector.Department == department);
        }

        var safetyInspections = inspections.Where(i => i.Type == InspectionType.Safety);
        var findings = _context.InspectionFindings.Where(f => safetyInspections.Any(si => si.Id == f.InspectionId));

        var totalInspections = await inspections.CountAsync(cancellationToken);
        var safetyInspectionCount = await safetyInspections.CountAsync(cancellationToken);
        var totalFindings = await findings.CountAsync(cancellationToken);
        var criticalFindings = await findings.CountAsync(f => f.Severity == FindingSeverity.Critical, cancellationToken);
        var highPriorityFindings = await findings.CountAsync(f => f.Severity == FindingSeverity.Major, cancellationToken);

        return new InspectionSafetyDto
        {
            TotalInspections = totalInspections,
            SafetyInspections = safetyInspectionCount,
            TotalFindings = totalFindings,
            CriticalFindings = criticalFindings,
            HighPriorityFindings = highPriorityFindings,
            InspectionComplianceRate = totalInspections > 0 ? (decimal)(totalInspections - criticalFindings) / totalInspections * 100 : 100
        };
    }

    private async Task<WorkPermitSafetyDto> GetWorkPermitSafetyAsync(DateTime startDate, DateTime endDate, string? department, CancellationToken cancellationToken)
    {
        var permits = _context.WorkPermits
            .Where(wp => wp.CreatedAt >= startDate && wp.CreatedAt <= endDate);

        if (!string.IsNullOrEmpty(department))
        {
            permits = permits.Where(wp => wp.RequestedByDepartment == department);
        }

        var activePermits = permits.Where(wp => wp.Status == WorkPermitStatus.InProgress || wp.Status == WorkPermitStatus.Approved);
        var safetyInductions = _context.WorkPermitSafetyVideos
            .Where(sv => permits.Any(p => p.Id == sv.WorkPermitSettingsId));

        var totalActive = await activePermits.CountAsync(cancellationToken);
        var safetyCompliant = await activePermits.CountAsync(wp => wp.RequiresHotWorkPermit || wp.RequiresConfinedSpaceEntry, cancellationToken);
        var overduePermits = await permits.CountAsync(wp => wp.PlannedEndDate < DateTime.UtcNow && (wp.Status == WorkPermitStatus.InProgress || wp.Status == WorkPermitStatus.Approved), cancellationToken);
        var hotWorkPermits = await permits.CountAsync(wp => wp.Type == WorkPermitType.HotWork, cancellationToken);
        var confinedSpacePermits = await permits.CountAsync(wp => wp.Type == WorkPermitType.ConfinedSpace, cancellationToken);

        return new WorkPermitSafetyDto
        {
            TotalActivePermits = totalActive,
            SafetyCompliantPermits = safetyCompliant,
            SafetyComplianceRate = totalActive > 0 ? (decimal)safetyCompliant / totalActive * 100 : 0,
            OverduePermits = overduePermits,
            HotWorkPermits = hotWorkPermits,
            ConfinedSpacePermits = confinedSpacePermits,
            CompletedSafetyInductions = safetyCompliant
        };
    }

    private async Task<WasteEnvironmentalDto> GetWasteEnvironmentalAsync(DateTime startDate, DateTime endDate, string? department, CancellationToken cancellationToken)
    {
        var wasteReports = _context.WasteReports
            .Where(wr => wr.GeneratedDate >= startDate && wr.GeneratedDate <= endDate);

        if (!string.IsNullOrEmpty(department))
        {
            wasteReports = wasteReports.Where(wr => wr.Location.Contains(department));
        }

        var totalReports = await wasteReports.CountAsync(cancellationToken);
        var hazardousWaste = await wasteReports.CountAsync(wr => wr.Category == WasteCategory.Hazardous, cancellationToken);
        var complianceIssues = await wasteReports.CountAsync(wr => wr.DisposalStatus == WasteDisposalStatus.Pending, cancellationToken);

        return new WasteEnvironmentalDto
        {
            TotalWasteReports = totalReports,
            HazardousWasteReports = hazardousWaste,
            ComplianceIssues = complianceIssues,
            WasteComplianceRate = totalReports > 0 ? (decimal)(totalReports - complianceIssues) / totalReports * 100 : 100,
            EnvironmentalImpactScore = totalReports > 0 ? Math.Max(0, 100 - (hazardousWaste * 10)) : 100
        };
    }

    private async Task<SecurityIncidentDto> GetSecurityIncidentsAsync(DateTime startDate, DateTime endDate, string? department, CancellationToken cancellationToken)
    {
        var securityIncidents = _context.SecurityIncidents
            .Where(si => si.IncidentDateTime >= startDate && si.IncidentDateTime <= endDate);

        if (!string.IsNullOrEmpty(department))
        {
            securityIncidents = securityIncidents.Where(si => si.Location.Contains(department));
        }

        var totalIncidents = await securityIncidents.CountAsync(cancellationToken);
        var physicalSecurity = await securityIncidents.CountAsync(si => si.IncidentType == SecurityIncidentType.PhysicalSecurity, cancellationToken);
        var dataSecurity = await securityIncidents.CountAsync(si => si.IncidentType == SecurityIncidentType.InformationSecurity, cancellationToken);
        var resolvedIncidents = await securityIncidents.CountAsync(si => si.Status == SecurityIncidentStatus.Resolved, cancellationToken);
        var criticalIncidents = await securityIncidents.CountAsync(si => si.Severity == SecuritySeverity.Critical, cancellationToken);

        return new SecurityIncidentDto
        {
            TotalSecurityIncidents = totalIncidents,
            PhysicalSecurityIncidents = physicalSecurity,
            DataSecurityIncidents = dataSecurity,
            ResolvedIncidents = resolvedIncidents,
            ResolutionRate = totalIncidents > 0 ? (decimal)resolvedIncidents / totalIncidents * 100 : 0,
            CriticalSecurityIncidents = criticalIncidents,
            SecurityComplianceRate = totalIncidents > 0 ? (decimal)(totalIncidents - criticalIncidents) / totalIncidents * 100 : 100
        };
    }

    private async Task<HealthMonitoringDto> GetHealthMonitoringAsync(DateTime startDate, DateTime endDate, string? department, CancellationToken cancellationToken)
    {
        var healthIncidents = _context.HealthIncidents
            .Where(hi => hi.IncidentDateTime >= startDate && hi.IncidentDateTime <= endDate);

        if (!string.IsNullOrEmpty(department))
        {
            healthIncidents = healthIncidents.Where(hi => hi.HealthRecord.Person.Department == department);
        }

        var totalIncidents = await healthIncidents.CountAsync(cancellationToken);
        var occupationalHealth = await healthIncidents.CountAsync(hi => hi.Type == HealthIncidentType.Injury || hi.Type == HealthIncidentType.Illness, cancellationToken);
        var medicalEmergencies = await healthIncidents.CountAsync(hi => hi.Type == HealthIncidentType.EmergencyResponse, cancellationToken);
        var complianceIssues = await healthIncidents.CountAsync(hi => hi.RequiredHospitalization && !hi.ParentsNotified, cancellationToken);

        return new HealthMonitoringDto
        {
            TotalHealthIncidents = totalIncidents,
            OccupationalHealthCases = occupationalHealth,
            MedicalEmergencies = medicalEmergencies,
            HealthComplianceIssues = complianceIssues,
            HealthComplianceRate = totalIncidents > 0 ? (decimal)(totalIncidents - complianceIssues) / totalIncidents * 100 : 100
        };
    }

    private async Task<AuditFindingsDto> GetAuditFindingsAsync(DateTime startDate, DateTime endDate, string? department, CancellationToken cancellationToken)
    {
        var audits = _context.Audits
            .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate);

        if (!string.IsNullOrEmpty(department))
        {
            audits = audits.Where(a => a.Department != null && a.Department.Name == department);
        }

        var safetyAudits = audits.Where(a => a.Type == AuditType.Safety);
        var findings = _context.AuditFindings.Where(af => audits.Any(a => a.Id == af.AuditId));

        var totalAudits = await audits.CountAsync(cancellationToken);
        var safetyAuditCount = await safetyAudits.CountAsync(cancellationToken);
        var totalFindings = await findings.CountAsync(cancellationToken);
        var majorNonConformities = await findings.CountAsync(f => f.Severity == FindingSeverity.Major, cancellationToken);
        var minorNonConformities = await findings.CountAsync(f => f.Severity == FindingSeverity.Minor, cancellationToken);

        return new AuditFindingsDto
        {
            TotalAudits = totalAudits,
            SafetyAudits = safetyAuditCount,
            TotalFindings = totalFindings,
            MajorNonConformities = majorNonConformities,
            MinorNonConformities = minorNonConformities,
            AuditComplianceScore = totalFindings > 0 ? Math.Max(0, 100 - (majorNonConformities * 10 + minorNonConformities * 5)) : 100
        };
    }

    #endregion

    #region Helper Methods

    private async Task<List<NonConformanceCriteriaDto>> GetNonConformanceCriteriaAsync(DateTime startDate, DateTime endDate, string? department, CancellationToken cancellationToken)
    {
        // Aggregate non-conformances from multiple sources
        var hazardNonConformances = await _context.Hazards
            .Where(h => h.CreatedAt >= startDate && h.CreatedAt <= endDate)
            .Where(h => string.IsNullOrEmpty(department) || h.Location == department)
            .GroupBy(h => h.Location ?? "Unknown")
            .Select(g => new NonConformanceCriteriaDto
            {
                Category = g.Key,
                Count = g.Count(),
                Description = "Hazard-related non-conformances",
                Location = g.Key
            })
            .OrderByDescending(c => c.Count)
            .Take(5)
            .ToListAsync(cancellationToken);

        return hazardNonConformances;
    }

    private async Task<List<UnsafeConditionDto>> GetTopUnsafeConditionsAsync(DateTime startDate, DateTime endDate, string? department, CancellationToken cancellationToken)
    {
        var hazards = _context.Hazards.AsQueryable();

        if (!string.IsNullOrEmpty(department))
        {
            hazards = hazards.Where(h => h.Location == department);
        }

        var total = await hazards.CountAsync(h => h.CreatedAt >= startDate && h.CreatedAt <= endDate, cancellationToken);

        var unsafeConditions = await hazards
            .Where(h => h.CreatedAt >= startDate && h.CreatedAt <= endDate)
            .GroupBy(h => h.Title)
            .Select(g => new UnsafeConditionDto
            {
                Description = g.Key,
                Count = g.Count(),
                Percentage = total > 0 ? (decimal)g.Count() / total * 100 : 0,
                Severity = "Medium" // Default severity mapping
            })
            .OrderByDescending(u => u.Count)
            .Take(10)
            .ToListAsync(cancellationToken);

        // Add ranking
        for (int i = 0; i < unsafeConditions.Count; i++)
        {
            unsafeConditions[i].Rank = i + 1;
        }

        return unsafeConditions;
    }

    private async Task<ResponsibleActionSummaryDto> GetResponsibleActionsAsync(DateTime startDate, DateTime endDate, string? department, CancellationToken cancellationToken)
    {
        // Aggregate actions from multiple modules
        var hazardActions = _context.HazardMitigationActions.AsQueryable();
        
        if (!string.IsNullOrEmpty(department))
        {
            hazardActions = hazardActions.Where(ha => ha.Hazard.Location == department);
        }

        var totalActions = await hazardActions.CountAsync(ha => ha.CreatedAt >= startDate && ha.CreatedAt <= endDate, cancellationToken);
        var openActions = await hazardActions.CountAsync(ha => ha.CreatedAt >= startDate && ha.CreatedAt <= endDate && ha.Status != MitigationActionStatus.Completed, cancellationToken);
        var closedActions = totalActions - openActions;
        var overdueActions = await hazardActions.CountAsync(ha => ha.CreatedAt >= startDate && ha.CreatedAt <= endDate && ha.TargetDate < DateTime.UtcNow && ha.Status != MitigationActionStatus.Completed, cancellationToken);

        return new ResponsibleActionSummaryDto
        {
            TotalActions = totalActions,
            OpenActions = openActions,
            ClosedActions = closedActions,
            OverdueActions = overdueActions,
            CompletionRate = totalActions > 0 ? (decimal)closedActions / totalActions * 100 : 0
        };
    }

    private async Task<HazardCaseStatusDto> GetHazardCaseStatusAsync(DateTime startDate, DateTime endDate, string? department, CancellationToken cancellationToken)
    {
        var hazards = _context.Hazards.AsQueryable();

        if (!string.IsNullOrEmpty(department))
        {
            hazards = hazards.Where(h => h.Location == department);
        }

        var totalCases = await hazards.CountAsync(h => h.CreatedAt >= startDate && h.CreatedAt <= endDate, cancellationToken);
        var openCases = await hazards.CountAsync(h => h.CreatedAt >= startDate && h.CreatedAt <= endDate && h.Status != HazardStatus.Closed, cancellationToken);
        var closedCases = totalCases - openCases;

        return new HazardCaseStatusDto
        {
            TotalCases = totalCases,
            OpenCases = openCases,
            ClosedCases = closedCases,
            OpenPercentage = totalCases > 0 ? (decimal)openCases / totalCases * 100 : 0,
            ClosedPercentage = totalCases > 0 ? (decimal)closedCases / totalCases * 100 : 0,
            StartDate = startDate,
            EndDate = endDate
        };
    }

    private async Task<List<IncidentFrequencyRateDto>> GetIncidentFrequencyRatesAsync(DateTime startDate, DateTime endDate, string? department, CancellationToken cancellationToken)
    {
        var currentYear = DateTime.UtcNow.Year;
        var rates = new List<IncidentFrequencyRateDto>();

        for (int year = currentYear - 5; year <= currentYear; year++)
        {
            var incidents = _context.Incidents
                .Where(i => i.IncidentDate.Year == year);
            
            if (!string.IsNullOrEmpty(department))
            {
                incidents = incidents.Where(i => i.Location == department);
            }

            var totalIncidents = await incidents.CountAsync(cancellationToken);
            var workingHours = 2080 * 100; // Approximate working hours

            rates.Add(new IncidentFrequencyRateDto
            {
                Year = year,
                TotalRecordableIncidentFrequencyRate = workingHours > 0 ? (decimal)totalIncidents / workingHours * 200000 : 0,
                TotalRecordableSeverityRate = workingHours > 0 ? (decimal)totalIncidents * 5 / workingHours * 200000 : 0
            });
        }

        return rates;
    }

    private async Task<List<SafetyPerformanceDto>> GetSafetyPerformanceAsync(DateTime startDate, DateTime endDate, string? department, CancellationToken cancellationToken)
    {
        var currentYear = DateTime.UtcNow.Year;
        var performance = new List<SafetyPerformanceDto>();

        for (int year = currentYear - 5; year <= currentYear; year++)
        {
            var hazards = _context.Hazards
                .Where(h => h.CreatedAt.Year == year);
            
            if (!string.IsNullOrEmpty(department))
            {
                hazards = hazards.Where(h => h.Location == department);
            }

            var totalHazards = await hazards.CountAsync(cancellationToken);
            var nearMiss = await hazards.CountAsync(h => h.Category != null && h.Category.Name.ToLower().Contains("near miss"), cancellationToken);
            var accidents = await hazards.CountAsync(h => h.Severity >= HazardSeverity.Major, cancellationToken);

            var totalIFR = totalHazards * 0.1m; // Mock calculation
            var performanceLevel = totalIFR < 2 ? "Excellent" : totalIFR < 5 ? "Good" : "Average";
            var colorCode = totalIFR < 2 ? "Green" : totalIFR < 5 ? "LightGreen" : "Yellow";

            performance.Add(new SafetyPerformanceDto
            {
                Year = year,
                Hazards = totalHazards,
                NearMiss = nearMiss,
                Accidents = accidents,
                TotalIFR = totalIFR,
                PerformanceLevel = performanceLevel,
                ColorCode = colorCode
            });
        }

        return performance;
    }

    private async Task<LostTimeInjuryDto> GetLostTimeInjuryAsync(DateTime startDate, DateTime endDate, string? department, CancellationToken cancellationToken)
    {
        var lostTimeIncidents = _context.Incidents
            .Where(i => i.IncidentDate >= startDate && i.IncidentDate <= endDate)
            .Where(i => i.InjuryType != InjuryType.None && i.MedicalTreatmentProvided);

        if (!string.IsNullOrEmpty(department))
        {
            lostTimeIncidents = lostTimeIncidents.Where(i => i.Location == department);
        }

        var totalCases = await lostTimeIncidents.CountAsync(cancellationToken);
        var totalHours = 2080 * 100; // Approximate total hours

        return new LostTimeInjuryDto
        {
            Year = DateTime.UtcNow.Year,
            TotalLTICaseRate = totalHours > 0 ? (decimal)totalCases / totalHours * 200000 : 0,
            TotalLTICases = totalCases
        };
    }

    private static string GetChartColor(string category)
    {
        return category.ToLower() switch
        {
            "biological" => "#FF6B6B",
            "chemical" => "#4ECDC4",
            "physical" => "#45B7D1",
            "mechanical" => "#96CEB4",
            "ergonomic" => "#FFEAA7",
            "psychosocial" => "#DDA0DD",
            "environmental" => "#98FB98",
            _ => "#FFA07A"
        };
    }

    #endregion
}