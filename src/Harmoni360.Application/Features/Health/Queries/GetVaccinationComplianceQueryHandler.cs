using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Health.DTOs;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.Health.Queries;

public class GetVaccinationComplianceQueryHandler : IRequestHandler<GetVaccinationComplianceQuery, VaccinationComplianceDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetVaccinationComplianceQueryHandler> _logger;
    private readonly ICacheService _cache;

    public GetVaccinationComplianceQueryHandler(
        IApplicationDbContext context,
        ILogger<GetVaccinationComplianceQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<VaccinationComplianceDto> Handle(GetVaccinationComplianceQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting vaccination compliance data for date range: {FromDate} to {ToDate}, Department: {Department}, PersonType: {PersonType}, Vaccine: {VaccineName}",
            request.FromDate?.ToString("yyyy-MM-dd") ?? "All",
            request.ToDate?.ToString("yyyy-MM-dd") ?? "All",
            request.Department ?? "All",
            request.PersonType?.ToString() ?? "All",
            request.VaccineName ?? "All");

        // Set default date range if not provided
        var fromDate = request.FromDate ?? DateTime.UtcNow.AddYears(-1);
        var toDate = request.ToDate ?? DateTime.UtcNow;

        // Try cache first
        var cacheKey = $"vaccination-compliance-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}-{request.Department ?? "all"}-{request.PersonType?.ToString() ?? "all"}-{request.VaccineName ?? "all"}-{request.IncludeInactive}-{request.IncludeExemptions}";
        var cachedResult = await _cache.GetAsync<VaccinationComplianceDto>(cacheKey);
        if (cachedResult != null)
        {
            _logger.LogInformation("Vaccination compliance data found in cache");
            return cachedResult;
        }

        // Build base query for health records
        var healthRecordsQuery = _context.HealthRecords
            .Include(hr => hr.Person)
            .Include(hr => hr.Vaccinations.Where(v => 
                (!request.FromDate.HasValue || v.DateAdministered >= fromDate) &&
                (!request.ToDate.HasValue || v.DateAdministered <= toDate) &&
                (string.IsNullOrEmpty(request.VaccineName) || v.VaccineName.ToLower().Contains(request.VaccineName.ToLower()))))
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

        var healthRecords = await healthRecordsQuery.ToListAsync(cancellationToken);

        // Get all vaccinations from filtered health records
        var allVaccinations = healthRecords
            .SelectMany(hr => hr.Vaccinations.Where(v =>
                (!request.FromDate.HasValue || v.DateAdministered >= fromDate) &&
                (!request.ToDate.HasValue || v.DateAdministered <= toDate) &&
                (string.IsNullOrEmpty(request.VaccineName) || v.VaccineName.ToLower().Contains(request.VaccineName.ToLower()))))
            .ToList();

        // Calculate overall compliance metrics
        var totalPeople = healthRecords.Count;
        var totalVaccinations = allVaccinations.Count;
        var requiredVaccinations = allVaccinations.Where(v => v.IsRequired).ToList();
        var compliantVaccinations = requiredVaccinations.Where(v => v.IsCompliant()).ToList();
        var exemptVaccinations = allVaccinations.Where(v => !string.IsNullOrEmpty(v.ExemptionReason)).ToList();

        var overallComplianceRate = requiredVaccinations.Any() ? 
            (decimal)compliantVaccinations.Count / requiredVaccinations.Count * 100 : 100;

        var exemptionRate = allVaccinations.Any() ? 
            (decimal)exemptVaccinations.Count / allVaccinations.Count * 100 : 0;

        // Compliance by vaccine type
        var complianceByVaccine = allVaccinations
            .Where(v => v.IsRequired)
            .GroupBy(v => v.VaccineName)
            .Select(g =>
            {
                var compliant = g.Count(v => v.IsCompliant());
                var total = g.Count();
                var expired = g.Count(v => v.IsExpired());
                var expiringSoon = g.Count(v => v.IsExpiringSoon(30));
                var overdue = g.Count(v => v.Status == VaccinationStatus.Scheduled && v.DateAdministered == null);

                return new VaccinationTypeCompliance
                {
                    VaccineName = g.Key,
                    TotalRequired = total,
                    TotalCompliant = compliant,
                    ComplianceRate = total > 0 ? (decimal)compliant / total * 100 : 0,
                    TotalExpired = expired,
                    TotalExpiring = expiringSoon,
                    TotalExempt = g.Count(v => !string.IsNullOrEmpty(v.ExemptionReason)),
                    IsMandatory = true // Assuming required vaccinations are mandatory
                };
            })
            .OrderByDescending(v => v.TotalRequired)
            .ToList();

        // Compliance by person type
        var studentCompliance = healthRecords.Where(hr => hr.PersonType == PersonType.Student).ToList();
        var staffCompliance = healthRecords.Where(hr => hr.PersonType == PersonType.Staff).ToList();
        
        var studentVaccinations = studentCompliance.SelectMany(hr => hr.Vaccinations.Where(v => v.IsRequired)).ToList();
        var staffVaccinations = staffCompliance.SelectMany(hr => hr.Vaccinations.Where(v => v.IsRequired)).ToList();
        
        var studentComplianceBreakdown = new VaccinationComplianceBreakdown
        {
            PersonType = "Student",
            TotalRecords = studentCompliance.Count,
            CompliantRecords = studentVaccinations.Count(v => v.IsCompliant()),
            NonCompliantRecords = studentVaccinations.Count(v => !v.IsCompliant()),
            ComplianceRate = studentVaccinations.Any() ? (decimal)studentVaccinations.Count(v => v.IsCompliant()) / studentVaccinations.Count * 100 : 100,
            ExpiringVaccinations = studentVaccinations.Count(v => v.IsExpiringSoon(30)),
            ExpiredVaccinations = studentVaccinations.Count(v => v.IsExpired())
        };
        
        var staffComplianceBreakdown = new VaccinationComplianceBreakdown
        {
            PersonType = "Staff",
            TotalRecords = staffCompliance.Count,
            CompliantRecords = staffVaccinations.Count(v => v.IsCompliant()),
            NonCompliantRecords = staffVaccinations.Count(v => !v.IsCompliant()),
            ComplianceRate = staffVaccinations.Any() ? (decimal)staffVaccinations.Count(v => v.IsCompliant()) / staffVaccinations.Count * 100 : 100,
            ExpiringVaccinations = staffVaccinations.Count(v => v.IsExpiringSoon(30)),
            ExpiredVaccinations = staffVaccinations.Count(v => v.IsExpired())
        };

        // Compliance by department
        var complianceByDepartment = healthRecords
            .Where(hr => !string.IsNullOrEmpty(hr.Person.Department))
            .GroupBy(hr => new { hr.Person.Department, hr.PersonType })
            .Select(g =>
            {
                var vaccinations = g.SelectMany(hr => hr.Vaccinations.Where(v => v.IsRequired)).ToList();
                var compliant = vaccinations.Count(v => v.IsCompliant());
                var total = vaccinations.Count;

                return new DepartmentVaccinationCompliance
                {
                    Department = g.Key.Department!,
                    PersonType = g.Key.PersonType.ToString(),
                    TotalRecords = g.Count(),
                    CompliantRecords = compliant,
                    ComplianceRate = total > 0 ? (decimal)compliant / total * 100 : 100,
                    AtRiskRecords = vaccinations.Count(v => v.IsExpired() || v.IsExpiringSoon(30))
                };
            })
            .OrderByDescending(d => d.TotalRecords)
            .ToList();

        // Non-compliant people requiring attention
        var nonCompliantRecords = healthRecords
            .Where(hr => hr.Vaccinations.Any(v => v.IsRequired && !v.IsCompliant()))
            .Select(hr =>
            {
                var nonCompliantVaccinations = hr.Vaccinations
                    .Where(v => v.IsRequired && !v.IsCompliant())
                    .ToList();
                    
                var expiredVaccinations = nonCompliantVaccinations.Where(v => v.IsExpired()).ToList();
                var overdueVaccinations = nonCompliantVaccinations.Where(v => v.Status == VaccinationStatus.Scheduled && v.DateAdministered == null).ToList();
                var oldestOverdue = overdueVaccinations.Any() ? 
                    overdueVaccinations.Min(v => (DateTime.UtcNow - (v.ExpiryDate ?? DateTime.UtcNow)).TotalDays) : 0;

                return new NonCompliantRecordDto
                {
                    HealthRecordId = hr.Id,
                    PersonName = hr.Person.Name,
                    PersonType = hr.PersonType.ToString(),
                    Department = hr.Person.Department,
                    MissingVaccinations = overdueVaccinations.Select(v => v.VaccineName).ToList(),
                    ExpiredVaccinations = expiredVaccinations.Select(v => v.VaccineName).ToList(),
                    DaysOverdue = (int)oldestOverdue,
                    HasExemption = hr.Vaccinations.Any(v => !string.IsNullOrEmpty(v.ExemptionReason))
                };
            })
            .OrderByDescending(p => p.DaysOverdue)
            .Take(20)
            .ToList();
            
        // Expiring vaccinations
        var expiringVaccinationsList = allVaccinations
            .Where(v => v.IsExpiringSoon(30))
            .Select(v =>
            {
                var healthRecord = healthRecords.First(hr => hr.Id == v.HealthRecordId);
                return new ExpiringVaccinationDto
                {
                    VaccinationId = v.Id,
                    HealthRecordId = v.HealthRecordId,
                    PersonName = healthRecord.Person.Name,
                    PersonType = healthRecord.PersonType.ToString(),
                    VaccineName = v.VaccineName,
                    ExpiryDate = v.ExpiryDate ?? DateTime.UtcNow,
                    DaysUntilExpiry = v.DaysUntilExpiry(),
                    IsExpired = v.IsExpired(),
                    UrgencyLevel = v.IsExpired() ? "Critical" : 
                                  v.DaysUntilExpiry() <= 7 ? "High" :
                                  v.DaysUntilExpiry() <= 14 ? "Medium" : "Low"
                };
            })
            .OrderBy(v => v.DaysUntilExpiry)
            .ToList();

        var result = new VaccinationComplianceDto
        {
            TotalRecords = totalPeople,
            CompliantRecords = healthRecords.Count(hr => hr.Vaccinations.Where(v => v.IsRequired).All(v => v.IsCompliant())),
            NonCompliantRecords = healthRecords.Count(hr => hr.Vaccinations.Any(v => v.IsRequired && !v.IsCompliant())),
            ComplianceRate = overallComplianceRate,
            ExpiringVaccinations = allVaccinations.Count(v => v.IsExpiringSoon(30)),
            ExpiredVaccinations = allVaccinations.Count(v => v.IsExpired()),
            ExemptRecords = exemptVaccinations.Count,
            
            StudentCompliance = studentComplianceBreakdown,
            StaffCompliance = staffComplianceBreakdown,
            VaccinationsByType = complianceByVaccine,
            ComplianceByDepartment = complianceByDepartment,
            RecentNonCompliantRecords = nonCompliantRecords,
            CurrentExpiringVaccinations = expiringVaccinationsList,
            
            FromDate = fromDate,
            ToDate = toDate
        };

        // Cache for 1 hour
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(1), new[] { "health", "vaccination-compliance" });

        _logger.LogInformation("Vaccination compliance analysis completed. Overall compliance rate: {ComplianceRate}%, Total people: {TotalPeople}",
            overallComplianceRate, totalPeople);

        return result;
    }
}