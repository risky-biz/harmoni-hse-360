using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.PPE.Queries;

public class GetPPEDashboardQueryHandler : IRequestHandler<GetPPEDashboardQuery, PPEDashboardDto>
{
    private readonly IApplicationDbContext _context;

    public GetPPEDashboardQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PPEDashboardDto> Handle(GetPPEDashboardQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.PPEItems
            .Include(i => i.Category)
            .Include(i => i.AssignedTo)
            .ToListAsync(cancellationToken);

        var totalItems = items.Count;
        var availableItems = items.Count(i => i.Status == PPEStatus.Available);
        var assignedItems = items.Count(i => i.Status == PPEStatus.Assigned);
        var outOfServiceItems = items.Count(i => i.Status == PPEStatus.OutOfService);
        var lostItems = items.Count(i => i.Status == PPEStatus.Lost);
        var retiredItems = items.Count(i => i.Status == PPEStatus.Retired);

        var now = DateTime.UtcNow;
        var expiredItems = items.Count(i => i.ExpiryDate.HasValue && i.ExpiryDate.Value < now);
        var expiringSoonItems = items.Count(i => i.ExpiryDate.HasValue && 
                                                  i.ExpiryDate.Value >= now && 
                                                  i.ExpiryDate.Value <= now.AddDays(30));

        var maintenanceDueItems = items.Count(i => i.MaintenanceInfo != null && 
                                                   i.MaintenanceInfo.NextMaintenanceDate.HasValue && 
                                                   i.MaintenanceInfo.NextMaintenanceDate.Value <= now);

        var inspectionDueItems = items.Count(i => i.Category.RequiresInspection && 
                                                  i.Category.InspectionIntervalDays.HasValue &&
                                                  (!i.Inspections.Any() || 
                                                   i.Inspections.OrderByDescending(insp => insp.InspectionDate)
                                                     .First().InspectionDate.AddDays(i.Category.InspectionIntervalDays.Value) <= now));

        // Category statistics
        var categoryStats = items
            .GroupBy(i => new { i.CategoryId, i.Category.Name })
            .Select(g => new PPECategoryStatsDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                TotalItems = g.Count(),
                AvailableItems = g.Count(i => i.Status == PPEStatus.Available),
                AssignedItems = g.Count(i => i.Status == PPEStatus.Assigned)
            })
            .OrderBy(c => c.CategoryName)
            .ToList();

        // Status statistics
        var statusStats = items
            .GroupBy(i => i.Status)
            .Select(g => new PPEStatusStatsDto
            {
                Status = g.Key.ToString(),
                Count = g.Count(),
                Percentage = totalItems > 0 ? Math.Round((decimal)g.Count() / totalItems * 100, 1) : 0
            })
            .OrderByDescending(s => s.Count)
            .ToList();

        // Condition statistics
        var conditionStats = items
            .GroupBy(i => i.Condition)
            .Select(g => new PPEConditionStatsDto
            {
                Condition = g.Key.ToString(),
                Count = g.Count(),
                Percentage = totalItems > 0 ? Math.Round((decimal)g.Count() / totalItems * 100, 1) : 0
            })
            .OrderByDescending(c => c.Count)
            .ToList();

        // Expiry warnings
        var expiryWarnings = items
            .Where(i => i.ExpiryDate.HasValue && i.ExpiryDate.Value <= now.AddDays(60))
            .Select(i => new PPEExpiryWarningDto
            {
                ItemId = i.Id,
                ItemCode = i.ItemCode,
                ItemName = i.Name,
                ExpiryDate = i.ExpiryDate!.Value,
                DaysUntilExpiry = (int)(i.ExpiryDate.Value - now).TotalDays,
                IsExpired = i.ExpiryDate.Value < now
            })
            .OrderBy(w => w.DaysUntilExpiry)
            .ToList();

        // Maintenance warnings
        var maintenanceWarnings = items
            .Where(i => i.MaintenanceInfo != null && 
                       i.MaintenanceInfo.NextMaintenanceDate.HasValue &&
                       i.MaintenanceInfo.NextMaintenanceDate.Value <= now.AddDays(30))
            .Select(i => new PPEMaintenanceWarningDto
            {
                ItemId = i.Id,
                ItemCode = i.ItemCode,
                ItemName = i.Name,
                DueDate = i.MaintenanceInfo!.NextMaintenanceDate!.Value,
                DaysOverdue = i.MaintenanceInfo.NextMaintenanceDate.Value < now 
                    ? (int)(now - i.MaintenanceInfo.NextMaintenanceDate.Value).TotalDays 
                    : 0,
                IsOverdue = i.MaintenanceInfo.NextMaintenanceDate.Value < now
            })
            .OrderBy(w => w.DueDate)
            .ToList();

        return new PPEDashboardDto
        {
            TotalItems = totalItems,
            AvailableItems = availableItems,
            AssignedItems = assignedItems,
            OutOfServiceItems = outOfServiceItems,
            ExpiredItems = expiredItems,
            ExpiringSoonItems = expiringSoonItems,
            MaintenanceDueItems = maintenanceDueItems,
            InspectionDueItems = inspectionDueItems,
            LostItems = lostItems,
            RetiredItems = retiredItems,
            CategoryStats = categoryStats,
            StatusStats = statusStats,
            ConditionStats = conditionStats,
            ExpiryWarnings = expiryWarnings,
            MaintenanceWarnings = maintenanceWarnings
        };
    }
}