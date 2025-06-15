using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Licenses.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using System.Linq;

namespace Harmoni360.Application.Features.Licenses.Queries;

public class GetLicenseDashboardQueryHandler : IRequestHandler<GetLicenseDashboardQuery, LicenseDashboardDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetLicenseDashboardQueryHandler> _logger;

    public GetLicenseDashboardQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetLicenseDashboardQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LicenseDashboardDto> Handle(GetLicenseDashboardQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Licenses.AsQueryable();

        // Apply filters
        if (request.DepartmentId.HasValue)
        {
            query = query.Where(l => l.Department == request.DepartmentId.Value.ToString());
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(l => l.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(l => l.CreatedAt <= request.ToDate.Value);
        }

        // Get all licenses for the query
        var licenses = await query.ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;

        var dashboard = new LicenseDashboardDto
        {
            // Status Summary
            TotalLicenses = licenses.Count,
            ActiveLicenses = licenses.Count(l => l.Status == LicenseStatus.Active),
            DraftLicenses = licenses.Count(l => l.Status == LicenseStatus.Draft),
            PendingSubmissionLicenses = licenses.Count(l => l.Status == LicenseStatus.PendingSubmission),
            SubmittedLicenses = licenses.Count(l => l.Status == LicenseStatus.Submitted),
            UnderReviewLicenses = licenses.Count(l => l.Status == LicenseStatus.UnderReview),
            ApprovedLicenses = licenses.Count(l => l.Status == LicenseStatus.Approved),
            RejectedLicenses = licenses.Count(l => l.Status == LicenseStatus.Rejected),
            ExpiredLicenses = licenses.Count(l => l.Status == LicenseStatus.Expired || (l.ExpiryDate < now && l.Status == LicenseStatus.Active)),
            SuspendedLicenses = licenses.Count(l => l.Status == LicenseStatus.Suspended),
            RevokedLicenses = licenses.Count(l => l.Status == LicenseStatus.Revoked),
            PendingRenewalLicenses = licenses.Count(l => l.Status == LicenseStatus.PendingRenewal),

            // Risk Analytics
            HighRiskLicenses = licenses.Count(l => l.RiskLevel == RiskLevel.High || l.RiskLevel == RiskLevel.Critical),
            CriticalLicenses = licenses.Count(l => l.IsCriticalLicense),
            ComplianceRate = CalculateComplianceRate(licenses),

            // Alerts
            ExpiringThisWeek = licenses.Count(l => 
                l.Status == LicenseStatus.Active && 
                l.ExpiryDate >= now && 
                l.ExpiryDate <= now.AddDays(7)),
            ExpiringThisMonth = licenses.Count(l => 
                l.Status == LicenseStatus.Active && 
                l.ExpiryDate >= now && 
                l.ExpiryDate <= now.AddDays(30)),
            OverdueLicenses = licenses.Count(l => 
                l.ExpiryDate < now && 
                (l.Status == LicenseStatus.Active || l.Status == LicenseStatus.Approved)),
            LicensesDueForRenewal = licenses.Count(l => 
                l.RenewalRequired && 
                l.NextRenewalDate.HasValue && 
                l.NextRenewalDate.Value <= now.AddDays(l.RenewalPeriodDays))
        };

        // Recent Licenses (last 10 created)
        dashboard.RecentLicenses = await query
            .OrderByDescending(l => l.CreatedAt)
            .Take(10)
            .Select(l => _mapper.Map<LicenseDto>(l))
            .ToListAsync(cancellationToken);

        // Expiring Licenses (next 30 days)
        dashboard.ExpiringLicenses = await query
            .Where(l => l.Status == LicenseStatus.Active && 
                       l.ExpiryDate >= now && 
                       l.ExpiryDate <= now.AddDays(30))
            .OrderBy(l => l.ExpiryDate)
            .Take(10)
            .Select(l => _mapper.Map<LicenseDto>(l))
            .ToListAsync(cancellationToken);

        // High Priority Licenses
        dashboard.HighPriorityLicenses = await query
            .Where(l => l.Priority == LicensePriority.High || l.Priority == LicensePriority.Critical)
            .OrderByDescending(l => l.Priority)
            .ThenBy(l => l.ExpiryDate)
            .Take(10)
            .Select(l => _mapper.Map<LicenseDto>(l))
            .ToListAsync(cancellationToken);

        // Calculate trends
        dashboard.MonthlyTrends = await CalculateMonthlyTrends(query, cancellationToken);
        dashboard.LicensesByType = await CalculateLicensesByType(query, cancellationToken);
        dashboard.LicensesByAuthority = await CalculateLicensesByAuthority(query, cancellationToken);

        return dashboard;
    }

    private double CalculateComplianceRate(List<License> licenses)
    {
        if (!licenses.Any()) return 100.0;

        var activeLicenses = licenses.Where(l => 
            l.Status == LicenseStatus.Active || 
            l.Status == LicenseStatus.Approved || 
            l.Status == LicenseStatus.PendingRenewal).ToList();

        if (!activeLicenses.Any()) return 100.0;

        var compliantLicenses = activeLicenses.Count(l => 
            l.ExpiryDate >= DateTime.UtcNow &&
            (!l.RequiresInsurance || l.RequiredInsuranceAmount.HasValue));

        return Math.Round((double)compliantLicenses / activeLicenses.Count * 100, 2);
    }

    private async Task<List<LicenseMonthlyTrendDto>> CalculateMonthlyTrends(
        IQueryable<License> query, 
        CancellationToken cancellationToken)
    {
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var trends = new List<LicenseMonthlyTrendDto>();

        for (int i = 5; i >= 0; i--)
        {
            var monthStart = DateTime.UtcNow.AddMonths(-i).Date;
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthData = await query
                .Where(l => l.CreatedAt <= monthEnd)
                .ToListAsync(cancellationToken);

            trends.Add(new LicenseMonthlyTrendDto
            {
                Month = monthStart.ToString("MMMM"),
                Year = monthStart.Year,
                Created = monthData.Count(l => l.CreatedAt >= monthStart && l.CreatedAt <= monthEnd),
                Expired = monthData.Count(l => l.ExpiryDate >= monthStart && l.ExpiryDate <= monthEnd),
                Renewed = monthData.Count(l => l.Renewals.Any(r => r.ApprovedDate >= monthStart && r.ApprovedDate <= monthEnd)),
                Active = monthData.Count(l => l.Status == LicenseStatus.Active && l.CreatedAt <= monthEnd)
            });
        }

        return trends;
    }

    private async Task<List<LicenseTypeStatDto>> CalculateLicensesByType(
        IQueryable<License> query, 
        CancellationToken cancellationToken)
    {
        var licensesByType = await query
            .GroupBy(l => l.Type)
            .Select(g => new
            {
                Type = g.Key,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var total = licensesByType.Sum(x => x.Count);

        return licensesByType.Select(x => new LicenseTypeStatDto
        {
            Type = x.Type.ToString(),
            TypeDisplay = x.Type.ToString().Replace("_", " "),
            Count = x.Count,
            Percentage = total > 0 ? Math.Round((double)x.Count / total * 100, 2) : 0
        }).OrderByDescending(x => x.Count).ToList();
    }

    private async Task<List<LicenseAuthorityStatDto>> CalculateLicensesByAuthority(
        IQueryable<License> query, 
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        
        var licensesByAuthority = await query
            .GroupBy(l => l.IssuingAuthority)
            .Select(g => new LicenseAuthorityStatDto
            {
                Authority = g.Key,
                Count = g.Count(),
                Active = g.Count(l => l.Status == LicenseStatus.Active),
                Expired = g.Count(l => l.ExpiryDate < now),
                ComplianceRate = g.Count() > 0 ? 
                    Math.Round((double)g.Count(l => l.Status == LicenseStatus.Active && l.ExpiryDate >= now) / g.Count() * 100, 2) : 
                    0
            })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync(cancellationToken);

        return licensesByAuthority;
    }
}