using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Health.DTOs;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.Health.Queries;

public class GetHealthDashboardQueryHandler : IRequestHandler<GetHealthDashboardQuery, HealthDashboardDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetHealthDashboardQueryHandler> _logger;
    private readonly ICacheService _cache;

    public GetHealthDashboardQueryHandler(
        IApplicationDbContext context,
        ILogger<GetHealthDashboardQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<HealthDashboardDto> Handle(GetHealthDashboardQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting health dashboard data for date range: {FromDate} to {ToDate}, Department: {Department}",
            request.FromDate?.ToString("yyyy-MM-dd") ?? "All", 
            request.ToDate?.ToString("yyyy-MM-dd") ?? "All",
            request.Department ?? "All");

        // Set default date range if not provided
        var fromDate = request.FromDate ?? DateTime.UtcNow.AddMonths(-12);
        var toDate = request.ToDate ?? DateTime.UtcNow;

        // Try cache first
        var cacheKey = $"health-dashboard-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}-{request.Department ?? "all"}-{request.PersonType?.ToString() ?? "all"}";
        var cachedResult = await _cache.GetAsync<HealthDashboardDto>(cacheKey);
        if (cachedResult != null)
        {
            _logger.LogInformation("Health dashboard data found in cache");
            return cachedResult;
        }

        // Build base queries with filters
        var healthRecordsQuery = _context.HealthRecords
            .Include(hr => hr.Person)
            .Include(hr => hr.MedicalConditions.Where(mc => mc.IsActive))
            .Include(hr => hr.Vaccinations)
            .Include(hr => hr.HealthIncidents)
            .Include(hr => hr.EmergencyContacts.Where(ec => ec.IsActive))
            .AsQueryable();

        if (!request.IncludeInactive)
        {
            healthRecordsQuery = healthRecordsQuery.Where(hr => hr.IsActive);
        }

        if (request.PersonType.HasValue)
        {
            healthRecordsQuery = healthRecordsQuery.Where(hr => hr.PersonType == request.PersonType.Value);
        }

        if (!string.IsNullOrEmpty(request.Department))
        {
            healthRecordsQuery = healthRecordsQuery.Where(hr => 
                hr.Person.Department != null && 
                hr.Person.Department.ToLower().Contains(request.Department.ToLower()));
        }

        // Get all health records for calculations
        var healthRecords = await healthRecordsQuery.ToListAsync(cancellationToken);

        // Calculate basic health record metrics
        var totalHealthRecords = healthRecords.Count;
        var totalStudentRecords = healthRecords.Count(hr => hr.PersonType == PersonType.Student);
        var totalStaffRecords = healthRecords.Count(hr => hr.PersonType == PersonType.Staff);
        var activeHealthRecords = healthRecords.Count(hr => hr.IsActive);

        // Medical conditions analysis
        var allMedicalConditions = healthRecords.SelectMany(hr => hr.MedicalConditions.Where(mc => mc.IsActive)).ToList();
        var totalMedicalConditions = allMedicalConditions.Count;
        var criticalMedicalConditions = allMedicalConditions.Count(mc => mc.RequiresEmergencyAction);
        var lifeThreateningConditions = allMedicalConditions.Count(mc => mc.Severity == ConditionSeverity.LifeThreatening);

        // Medical conditions by category
        var conditionsByCategory = allMedicalConditions
            .GroupBy(mc => mc.Type)
            .Select(g => new ConditionCategoryBreakdown
            {
                Category = g.Key.ToString(),
                Count = g.Count(),
                CriticalCount = g.Count(mc => mc.RequiresEmergencyAction),
                Percentage = totalMedicalConditions > 0 ? (decimal)g.Count() / totalMedicalConditions * 100 : 0
            })
            .OrderByDescending(c => c.Count)
            .ToList();

        // Vaccination analysis
        var allVaccinations = healthRecords.SelectMany(hr => hr.Vaccinations).ToList();
        var requiredVaccinations = allVaccinations.Where(v => v.IsRequired).ToList();
        var compliantVaccinations = requiredVaccinations.Where(v => v.IsCompliant()).Count();
        var vaccinationComplianceRate = requiredVaccinations.Any() ? 
            (decimal)compliantVaccinations / requiredVaccinations.Count * 100 : 100;

        var cutoffDate = DateTime.UtcNow.AddDays(30);
        var expiringVaccinations = allVaccinations.Count(v => v.IsExpiringSoon(30));
        var expiredVaccinations = allVaccinations.Count(v => v.IsExpired());
        var overdueVaccinations = allVaccinations.Count(v => 
            v.Status == VaccinationStatus.Scheduled && v.DateAdministered == null);

        // Vaccination status breakdown
        var vaccinationsByStatus = allVaccinations
            .GroupBy(v => v.Status)
            .Select(g => new VaccinationStatusBreakdown
            {
                Status = g.Key.ToString(),
                Count = g.Count(),
                Percentage = allVaccinations.Any() ? (decimal)g.Count() / allVaccinations.Count * 100 : 0
            })
            .OrderByDescending(v => v.Count)
            .ToList();

        // Health incidents analysis
        var healthIncidentsQuery = healthRecords
            .SelectMany(hr => hr.HealthIncidents)
            .Where(hi => hi.IncidentDateTime >= fromDate && hi.IncidentDateTime <= toDate);

        var allHealthIncidents = healthIncidentsQuery.ToList();
        var totalHealthIncidents = allHealthIncidents.Count;
        var criticalHealthIncidents = allHealthIncidents.Count(hi => hi.IsCritical());
        var unresolvedHealthIncidents = allHealthIncidents.Count(hi => !hi.IsResolved);
        var recentHealthIncidents = allHealthIncidents.Count(hi => hi.IncidentDateTime >= DateTime.UtcNow.AddDays(-30));

        // Health incident trends (last 12 months by month)
        var healthIncidentTrends = Enumerable.Range(0, 12)
            .Select(i =>
            {
                var date = DateTime.UtcNow.AddMonths(-i).Date;
                var monthStart = new DateTime(date.Year, date.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                
                var monthIncidents = allHealthIncidents
                    .Where(hi => hi.IncidentDateTime >= monthStart && hi.IncidentDateTime <= monthEnd)
                    .ToList();

                return new HealthIncidentTrendDto
                {
                    Date = monthStart,
                    Count = monthIncidents.Count,
                    CriticalCount = monthIncidents.Count(hi => hi.IsCritical())
                };
            })
            .OrderBy(t => t.Date)
            .ToList();

        // Emergency contacts analysis
        var allEmergencyContacts = healthRecords.SelectMany(hr => hr.EmergencyContacts.Where(ec => ec.IsActive)).ToList();
        var totalEmergencyContacts = allEmergencyContacts.Count;
        var primaryContactsMissing = healthRecords.Count(hr => 
            !hr.EmergencyContacts.Any(ec => ec.IsActive && ec.IsPrimaryContact));
        
        var recordsWithContacts = healthRecords.Count(hr => hr.EmergencyContacts.Any(ec => ec.IsActive));
        var emergencyContactCompleteness = totalHealthRecords > 0 ? 
            (decimal)recordsWithContacts / totalHealthRecords * 100 : 0;

        // Recent activity
        var recentHealthRecords = healthRecords
            .Where(hr => hr.CreatedAt >= DateTime.UtcNow.AddDays(-30))
            .OrderByDescending(hr => hr.CreatedAt)
            .Take(5)
            .Select(hr => new HealthRecordDto
            {
                Id = hr.Id,
                PersonId = hr.PersonId,
                PersonType = hr.PersonType.ToString(),
                PersonName = hr.Person.Name,
                PersonDepartment = hr.Person.Department,
                CreatedAt = hr.CreatedAt,
                CreatedBy = hr.CreatedBy
            })
            .ToList();

        var recentHealthIncidentsList = allHealthIncidents
            .Where(hi => hi.CreatedAt >= DateTime.UtcNow.AddDays(-30))
            .OrderByDescending(hi => hi.CreatedAt)
            .Take(5)
            .Select(hi => new HealthIncidentDto
            {
                Id = hi.Id,
                HealthRecordId = hi.HealthRecordId,
                Type = hi.Type.ToString(),
                Severity = hi.Severity.ToString(),
                IncidentDateTime = hi.IncidentDateTime,
                CreatedAt = hi.CreatedAt,
                CreatedBy = hi.CreatedBy
            })
            .ToList();

        var expiringVaccinationsList = allVaccinations
            .Where(v => v.IsExpiringSoon(30))
            .OrderBy(v => v.ExpiryDate)
            .Take(10)
            .Select(v => new VaccinationRecordDto
            {
                Id = v.Id,
                HealthRecordId = v.HealthRecordId,
                VaccineName = v.VaccineName,
                ExpiryDate = v.ExpiryDate,
                DaysUntilExpiry = v.DaysUntilExpiry(),
                IsExpiringSoon = true
            })
            .ToList();

        // Build dashboard DTO
        var dashboard = new HealthDashboardDto
        {
            TotalHealthRecords = totalHealthRecords,
            TotalStudentRecords = totalStudentRecords,
            TotalStaffRecords = totalStaffRecords,
            ActiveHealthRecords = activeHealthRecords,

            TotalMedicalConditions = totalMedicalConditions,
            CriticalMedicalConditions = criticalMedicalConditions,
            LifeThreateningConditions = lifeThreateningConditions,
            ConditionsByCategory = conditionsByCategory,

            VaccinationComplianceRate = vaccinationComplianceRate,
            ExpiringVaccinations = expiringVaccinations,
            ExpiredVaccinations = expiredVaccinations,
            OverdueVaccinations = overdueVaccinations,
            VaccinationsByStatus = vaccinationsByStatus,

            TotalHealthIncidents = totalHealthIncidents,
            CriticalHealthIncidents = criticalHealthIncidents,
            UnresolvedHealthIncidents = unresolvedHealthIncidents,
            RecentHealthIncidents = recentHealthIncidents,
            HealthIncidentTrends = healthIncidentTrends,

            TotalEmergencyContacts = totalEmergencyContacts,
            EmergencyContactCompleteness = emergencyContactCompleteness,
            PrimaryContactsMissing = primaryContactsMissing,

            RecentHealthRecords = recentHealthRecords,
            RecentHealthIncidentDetails = recentHealthIncidentsList,
            DashboardExpiringVaccinations = expiringVaccinationsList,

            FromDate = fromDate,
            ToDate = toDate
        };

        // Cache for 15 minutes
        await _cache.SetAsync(cacheKey, dashboard, TimeSpan.FromMinutes(15), new[] { "health", "health-dashboard" });

        _logger.LogInformation("Health dashboard data calculated successfully. Total records: {TotalRecords}, Critical conditions: {CriticalConditions}, Compliance rate: {ComplianceRate}%",
            totalHealthRecords, criticalMedicalConditions, vaccinationComplianceRate);

        return dashboard;
    }
}