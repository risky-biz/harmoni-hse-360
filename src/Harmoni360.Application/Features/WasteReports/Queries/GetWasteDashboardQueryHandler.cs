using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WasteReports.DTOs;
using Harmoni360.Domain.Enums;
using Harmoni360.Domain.Entities.Waste;

namespace Harmoni360.Application.Features.WasteReports.Queries;

public class GetWasteDashboardQueryHandler : IRequestHandler<GetWasteDashboardQuery, WasteDashboardDto>
{
    private readonly IApplicationDbContext _context;

    public GetWasteDashboardQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WasteDashboardDto> Handle(GetWasteDashboardQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var sixMonthsAgo = now.AddMonths(-6);

        var totalReports = await _context.WasteReports.CountAsync(cancellationToken);
        var pendingReports = await _context.WasteReports
            .Where(w => w.DisposalStatus == WasteDisposalStatus.Pending)
            .CountAsync(cancellationToken);
        var completedReports = await _context.WasteReports
            .Where(w => w.DisposalStatus == WasteDisposalStatus.Disposed)
            .CountAsync(cancellationToken);

        var totalProviders = await _context.DisposalProviders
            .CountAsync(cancellationToken);
        var activeProviders = await _context.DisposalProviders
            .Where(p => p.IsActive)
            .CountAsync(cancellationToken);

        var categoryStats = await _context.WasteReports
            .GroupBy(w => w.Category)
            .Select(g => new WasteCategoryStatsDto
            {
                Category = g.Key.ToString(),
                Count = g.Count(),
                Percentage = totalReports > 0 ? (decimal)g.Count() / totalReports * 100 : 0
            })
            .ToListAsync(cancellationToken);

        var monthlyStatsRaw = await _context.WasteReports
            .Where(w => w.GeneratedDate >= sixMonthsAgo)
            .GroupBy(w => new { w.GeneratedDate.Year, w.GeneratedDate.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                ReportCount = g.Count()
            })
            .ToListAsync(cancellationToken);

        var monthlyStats = monthlyStatsRaw
            .Select(s => new MonthlyWasteStatsDto
            {
                Month = $"{s.Year}-{s.Month:D2}",
                ReportCount = s.ReportCount
            })
            .OrderBy(s => s.Month)
            .ToList();

        var recentReports = await _context.WasteReports
            .Include(w => w.Reporter)
            .Include(w => w.Attachments)
            .OrderByDescending(w => w.CreatedAt)
            .Take(5)
            .Select(w => new WasteReportDto
            {
                Id = w.Id,
                Title = w.Title,
                Description = w.Description,
                Category = w.Category.ToString(),
                Status = w.DisposalStatus.ToString(),
                GeneratedDate = w.GeneratedDate,
                Location = w.Location,
                ReporterId = w.ReporterId,
                ReporterName = w.Reporter != null ? w.Reporter.Name : null,
                AttachmentsCount = w.Attachments.Count,
                CreatedAt = w.CreatedAt,
                CreatedBy = w.CreatedBy
            })
            .ToListAsync(cancellationToken);

        var expiringProviders = await _context.DisposalProviders
            .Where(p => p.IsActive && p.LicenseExpiryDate <= now.AddMonths(3))
            .OrderBy(p => p.LicenseExpiryDate)
            .Select(p => new DisposalProviderDto
            {
                Id = p.Id,
                Name = p.Name,
                LicenseNumber = p.LicenseNumber,
                LicenseExpiryDate = p.LicenseExpiryDate,
                Status = p.Status.ToString(),
                IsActive = p.IsActive
            })
            .ToListAsync(cancellationToken);

        return new WasteDashboardDto
        {
            TotalReports = totalReports,
            PendingReports = pendingReports,
            CompletedReports = completedReports,
            TotalDisposalProviders = totalProviders,
            ActiveDisposalProviders = activeProviders,
            CategoryStats = categoryStats,
            MonthlyStats = monthlyStats,
            RecentReports = recentReports,
            ExpiringProviders = expiringProviders
        };
    }
}