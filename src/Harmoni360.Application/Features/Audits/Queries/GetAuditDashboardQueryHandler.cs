using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Audits.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Audits.Queries;

public class GetAuditDashboardQueryHandler : IRequestHandler<GetAuditDashboardQuery, AuditDashboardDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAuditDashboardQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<AuditDashboardDto> Handle(GetAuditDashboardQuery request, CancellationToken cancellationToken)
    {
        // Determine date range
        var dateFrom = request.StartDate ?? DateTime.UtcNow.AddMonths(-12);
        var dateTo = request.EndDate ?? DateTime.UtcNow;
        
        // Ensure dates are in UTC for PostgreSQL compatibility
        if (dateFrom.Kind == DateTimeKind.Unspecified)
            dateFrom = DateTime.SpecifyKind(dateFrom, DateTimeKind.Utc);
        else if (dateFrom.Kind == DateTimeKind.Local)
            dateFrom = dateFrom.ToUniversalTime();
            
        if (dateTo.Kind == DateTimeKind.Unspecified)
            dateTo = DateTime.SpecifyKind(dateTo, DateTimeKind.Utc);
        else if (dateTo.Kind == DateTimeKind.Local)
            dateTo = dateTo.ToUniversalTime();

        // Base query for audits within date range
        var auditQuery = _context.Audits
            .Where(a => a.CreatedAt >= dateFrom && a.CreatedAt <= dateTo);

        // Filter by department if specified
        if (request.DepartmentId.HasValue)
        {
            auditQuery = auditQuery.Where(a => a.DepartmentId == request.DepartmentId.Value);
        }

        // Get basic counts
        var totalAudits = await auditQuery.CountAsync(cancellationToken);
        var completedAudits = await auditQuery.CountAsync(a => a.Status == AuditStatus.Completed, cancellationToken);
        var pendingAudits = await auditQuery.CountAsync(a => a.Status == AuditStatus.InProgress, cancellationToken);
        var overdueAudits = await auditQuery.CountAsync(a => a.Status == AuditStatus.Scheduled && a.ScheduledDate < DateTime.UtcNow, cancellationToken);

        // Get audits by type
        var auditsByType = await auditQuery
            .GroupBy(a => a.Type)
            .Select(g => new AuditTypeStatDto
            {
                Type = g.Key.ToString(),
                Count = g.Count(),
                Percentage = totalAudits > 0 ? (int)((decimal)g.Count() / totalAudits * 100) : 0,
                AverageScore = g.Where(a => a.ScorePercentage.HasValue).Average(a => a.ScorePercentage) ?? 0
            })
            .ToListAsync(cancellationToken);

        // Get recent audits
        var recentAudits = await auditQuery
            .Include(a => a.Auditor)
            .Include(a => a.Department)
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .Select(a => new AuditSummaryDto
            {
                Id = a.Id,
                AuditNumber = a.AuditNumber,
                Title = a.Title,
                Type = a.Type.ToString(),
                Status = a.Status.ToString(),
                Priority = a.Priority.ToString(),
                ScheduledDate = a.ScheduledDate,
                AuditorName = a.Auditor.Name,
                DepartmentName = a.Department != null ? a.Department.Name : "Unknown",
                RiskLevel = a.RiskLevel.ToString(),
                ScorePercentage = a.ScorePercentage,
                FindingsCount = a.Findings.Count,
                CompletionPercentage = a.CompletionPercentage,
                IsOverdue = a.IsOverdue,
                CanEdit = a.CanEdit,
                CanStart = a.CanStart,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // Get high priority audits
        var highPriorityAudits = await auditQuery
            .Include(a => a.Auditor)
            .Include(a => a.Department)
            .Where(a => a.Priority == AuditPriority.High || a.Priority == AuditPriority.Critical)
            .OrderBy(a => a.ScheduledDate)
            .Take(10)
            .Select(a => new AuditSummaryDto
            {
                Id = a.Id,
                AuditNumber = a.AuditNumber,
                Title = a.Title,
                Type = a.Type.ToString(),
                Status = a.Status.ToString(),
                Priority = a.Priority.ToString(),
                ScheduledDate = a.ScheduledDate,
                AuditorName = a.Auditor.Name,
                DepartmentName = a.Department != null ? a.Department.Name : "Unknown",
                RiskLevel = a.RiskLevel.ToString(),
                ScorePercentage = a.ScorePercentage,
                FindingsCount = a.Findings.Count,
                CompletionPercentage = a.CompletionPercentage,
                IsOverdue = a.IsOverdue,
                CanEdit = a.CanEdit,
                CanStart = a.CanStart,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // Calculate rates and averages
        var completionRate = totalAudits > 0 ? (decimal)completedAudits / totalAudits * 100 : 0;
        var averageScore = await auditQuery
            .Where(a => a.ScorePercentage.HasValue)
            .AverageAsync(a => a.ScorePercentage!.Value, cancellationToken);

        // Get findings counts
        var totalFindings = await _context.AuditFindings
            .Where(f => auditQuery.Any(a => a.Id == f.AuditId))
            .CountAsync(cancellationToken);
        var openFindings = await _context.AuditFindings
            .Where(f => auditQuery.Any(a => a.Id == f.AuditId) && f.Status != FindingStatus.Closed)
            .CountAsync(cancellationToken);
        var criticalFindings = await _context.AuditFindings
            .Where(f => auditQuery.Any(a => a.Id == f.AuditId) && f.Severity == FindingSeverity.Critical)
            .CountAsync(cancellationToken);

        // Get audits due today and this week
        var today = DateTime.UtcNow.Date;
        var endOfWeek = today.AddDays(7);
        var auditsDueToday = await auditQuery
            .CountAsync(a => a.Status == AuditStatus.Scheduled && a.ScheduledDate.Date == today, cancellationToken);
        var auditsDueThisWeek = await auditQuery
            .CountAsync(a => a.Status == AuditStatus.Scheduled && a.ScheduledDate.Date >= today && a.ScheduledDate.Date <= endOfWeek, cancellationToken);

        return new AuditDashboardDto
        {
            TotalAudits = totalAudits,
            DraftAudits = await auditQuery.CountAsync(a => a.Status == AuditStatus.Draft, cancellationToken),
            ScheduledAudits = await auditQuery.CountAsync(a => a.Status == AuditStatus.Scheduled, cancellationToken),
            InProgressAudits = await auditQuery.CountAsync(a => a.Status == AuditStatus.InProgress, cancellationToken),
            CompletedAudits = completedAudits,
            OverdueAudits = overdueAudits,
            CancelledAudits = await auditQuery.CountAsync(a => a.Status == AuditStatus.Cancelled, cancellationToken),
            HighRiskAudits = await auditQuery.CountAsync(a => a.RiskLevel == RiskLevel.High, cancellationToken),
            CriticalRiskAudits = await auditQuery.CountAsync(a => a.RiskLevel == RiskLevel.Critical, cancellationToken),
            AuditsDueToday = auditsDueToday,
            AuditsDueThisWeek = auditsDueThisWeek,
            AverageScore = averageScore,
            CompletionRate = completionRate,
            TotalFindings = totalFindings,
            OpenFindings = openFindings,
            CriticalFindings = criticalFindings,
            AuditsByType = auditsByType,
            RecentAudits = recentAudits,
            HighPriorityAudits = highPriorityAudits
        };
    }
}