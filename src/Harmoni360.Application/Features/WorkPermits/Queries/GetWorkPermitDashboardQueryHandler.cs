using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WorkPermits.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Application.Features.WorkPermits.Queries
{
    public class GetWorkPermitDashboardQueryHandler : IRequestHandler<GetWorkPermitDashboardQuery, WorkPermitDashboardDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<GetWorkPermitDashboardQueryHandler> _logger;

        public GetWorkPermitDashboardQueryHandler(
            IApplicationDbContext context,
            ILogger<GetWorkPermitDashboardQueryHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<WorkPermitDashboardDto> Handle(GetWorkPermitDashboardQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Generating work permit dashboard data");

            var now = DateTime.UtcNow;
            var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            // Get all work permits for analysis
            var allPermits = await _context.WorkPermits
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var dashboard = new WorkPermitDashboardDto
            {
                // Basic counts
                TotalPermits = allPermits.Count,
                DraftPermits = allPermits.Count(p => p.Status == WorkPermitStatus.Draft),
                PendingApprovalPermits = allPermits.Count(p => p.Status == WorkPermitStatus.PendingApproval),
                ApprovedPermits = allPermits.Count(p => p.Status == WorkPermitStatus.Approved),
                InProgressPermits = allPermits.Count(p => p.Status == WorkPermitStatus.InProgress),
                CompletedPermits = allPermits.Count(p => p.Status == WorkPermitStatus.Completed),
                RejectedPermits = allPermits.Count(p => p.Status == WorkPermitStatus.Rejected),
                CancelledPermits = allPermits.Count(p => p.Status == WorkPermitStatus.Cancelled),
                ExpiredPermits = allPermits.Count(p => p.Status == WorkPermitStatus.Expired),

                // Risk and urgency counts
                HighRiskPermits = allPermits.Count(p => p.RiskLevel == RiskLevel.High),
                CriticalRiskPermits = allPermits.Count(p => p.RiskLevel == RiskLevel.Critical),
                OverduePermits = allPermits.Count(p => p.Status == WorkPermitStatus.InProgress && p.PlannedEndDate < now),
                PermitsDueToday = allPermits.Count(p => p.Status == WorkPermitStatus.InProgress && p.PlannedEndDate.Date == now.Date),
                PermitsDueThisWeek = allPermits.Count(p => p.Status == WorkPermitStatus.InProgress && p.PlannedEndDate >= startOfWeek && p.PlannedEndDate < endOfWeek)
            };

            // Permits by Type
            dashboard.PermitsByType = Enum.GetValues<WorkPermitType>()
                .Select(type => new WorkPermitTypeStatDto
                {
                    Type = type.ToString(),
                    Count = allPermits.Count(p => p.Type == type),
                    Percentage = allPermits.Any() ? 
                        (int)Math.Round((decimal)allPermits.Count(p => p.Type == type) / allPermits.Count * 100) : 0
                })
                .Where(t => t.Count > 0)
                .OrderByDescending(t => t.Count)
                .ToList();

            // Monthly Trends (last 12 months)
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            dashboard.MonthlyTrends = new List<WorkPermitMonthlyTrendDto>();
            for (int i = 11; i >= 0; i--)
            {
                var periodStart = startOfMonth.AddMonths(-i);
                var periodEnd = periodStart.AddMonths(1);
                var periodPermits = allPermits
                    .Where(p => p.CreatedAt >= periodStart && p.CreatedAt < periodEnd)
                    .ToList();

                dashboard.MonthlyTrends.Add(new WorkPermitMonthlyTrendDto
                {
                    Month = periodStart.ToString("MMM yyyy"),
                    TotalPermits = periodPermits.Count,
                    CompletedPermits = periodPermits.Count(p => p.Status == WorkPermitStatus.Completed),
                    SafelyCompletedPermits = periodPermits.Count(p => p.Status == WorkPermitStatus.Completed && p.IsCompletedSafely)
                });
            }

            // Recent Permits (last 5)
            dashboard.RecentPermits = await _context.WorkPermits
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new WorkPermitDto
                {
                    Id = p.Id,
                    PermitNumber = p.PermitNumber,
                    Title = p.Title,
                    Description = p.Description,
                    Type = p.Type.ToString(),
                    Status = p.Status.ToString(),
                    Priority = p.Priority.ToString(),
                    RiskLevel = p.RiskLevel.ToString(),
                    WorkLocation = p.WorkLocation,
                    PlannedStartDate = p.PlannedStartDate,
                    PlannedEndDate = p.PlannedEndDate,
                    ActualStartDate = p.ActualStartDate,
                    ActualEndDate = p.ActualEndDate,
                    RequestedByName = p.RequestedByName,
                    RequestedByDepartment = p.RequestedByDepartment,
                    WorkSupervisor = p.WorkSupervisor,
                    SafetyOfficer = p.SafetyOfficer,
                    IsCompletedSafely = p.IsCompletedSafely,
                    CreatedAt = p.CreatedAt,
                    CreatedBy = p.CreatedBy
                })
                .ToListAsync(cancellationToken);

            // High Priority Permits
            dashboard.HighPriorityPermits = await _context.WorkPermits
                .Where(p => p.Priority == WorkPermitPriority.High || p.Priority == WorkPermitPriority.Critical)
                .Where(p => p.Status != WorkPermitStatus.Completed && p.Status != WorkPermitStatus.Cancelled)
                .OrderByDescending(p => p.Priority)
                .ThenBy(p => p.PlannedStartDate)
                .Take(5)
                .Select(p => new WorkPermitDto
                {
                    Id = p.Id,
                    PermitNumber = p.PermitNumber,
                    Title = p.Title,
                    Description = p.Description,
                    Type = p.Type.ToString(),
                    Status = p.Status.ToString(),
                    Priority = p.Priority.ToString(),
                    RiskLevel = p.RiskLevel.ToString(),
                    WorkLocation = p.WorkLocation,
                    PlannedStartDate = p.PlannedStartDate,
                    PlannedEndDate = p.PlannedEndDate,
                    ActualStartDate = p.ActualStartDate,
                    ActualEndDate = p.ActualEndDate,
                    RequestedByName = p.RequestedByName,
                    RequestedByDepartment = p.RequestedByDepartment,
                    WorkSupervisor = p.WorkSupervisor,
                    SafetyOfficer = p.SafetyOfficer,
                    IsCompletedSafely = p.IsCompletedSafely,
                    CreatedAt = p.CreatedAt,
                    CreatedBy = p.CreatedBy
                })
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Work permit dashboard data generated successfully");
            return dashboard;
        }

    }
}