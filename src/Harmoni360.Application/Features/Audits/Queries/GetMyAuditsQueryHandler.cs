using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.Audits.DTOs;

namespace Harmoni360.Application.Features.Audits.Queries;

public class GetMyAuditsQueryHandler : IRequestHandler<GetMyAuditsQuery, PagedList<AuditSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyAuditsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PagedList<AuditSummaryDto>> Handle(GetMyAuditsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Audits
            .Include(a => a.Auditor)
            .Include(a => a.Department)
            .Include(a => a.Findings)
            .Where(a => a.AuditorId == _currentUserService.UserId)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(a => a.Title.Contains(request.Search) || 
                                   a.Description.Contains(request.Search) ||
                                   a.AuditNumber.Contains(request.Search));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(a => a.Status == request.Status.Value);
        }

        if (request.Type.HasValue)
        {
            query = query.Where(a => a.Type == request.Type.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(a => a.ScheduledDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(a => a.ScheduledDate <= request.EndDate.Value);
        }

        // Apply sorting
        query = request.SortBy.ToLower() switch
        {
            "title" => request.SortDescending ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title),
            "status" => request.SortDescending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
            "type" => request.SortDescending ? query.OrderByDescending(a => a.Type) : query.OrderBy(a => a.Type),
            "priority" => request.SortDescending ? query.OrderByDescending(a => a.Priority) : query.OrderBy(a => a.Priority),
            "createdat" => request.SortDescending ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt),
            _ => request.SortDescending ? query.OrderByDescending(a => a.ScheduledDate) : query.OrderBy(a => a.ScheduledDate)
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and select
        var audits = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
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
                DepartmentName = a.Department != null ? a.Department.Name : null,
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

        return new PagedList<AuditSummaryDto>(audits, totalCount, request.Page, request.PageSize);
    }
}