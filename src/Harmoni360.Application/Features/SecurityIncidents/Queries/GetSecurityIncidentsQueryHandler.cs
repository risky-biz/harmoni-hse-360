using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.SecurityIncidents.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.SecurityIncidents.Queries;

public class GetSecurityIncidentsQueryHandler : IRequestHandler<GetSecurityIncidentsQuery, PagedList<SecurityIncidentListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetSecurityIncidentsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PagedList<SecurityIncidentListDto>> Handle(GetSecurityIncidentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.SecurityIncidents.AsQueryable();

        // Apply filters
        if (request.Type.HasValue)
        {
            query = query.Where(i => i.IncidentType == request.Type.Value);
        }

        if (request.Category.HasValue)
        {
            query = query.Where(i => i.Category == request.Category.Value);
        }

        if (request.MinSeverity.HasValue)
        {
            query = query.Where(i => i.Severity >= request.MinSeverity.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(i => i.Status == request.Status.Value);
        }

        if (request.MinThreatLevel.HasValue)
        {
            query = query.Where(i => i.ThreatLevel >= request.MinThreatLevel.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(i => i.IncidentDateTime >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(i => i.IncidentDateTime <= request.EndDate.Value);
        }

        if (!string.IsNullOrEmpty(request.Location))
        {
            query = query.Where(i => i.Location.Contains(request.Location));
        }

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(i => 
                i.Title.ToLower().Contains(searchTerm) ||
                i.Description.ToLower().Contains(searchTerm) ||
                i.IncidentNumber.ToLower().Contains(searchTerm));
        }

        if (request.IsDataBreach.HasValue)
        {
            query = query.Where(i => i.DataBreachOccurred == request.IsDataBreach.Value);
        }

        if (request.IsInternalThreat.HasValue)
        {
            query = query.Where(i => i.IsInternalThreat == request.IsInternalThreat.Value);
        }

        if (request.ReporterId.HasValue)
        {
            query = query.Where(i => i.ReporterId == request.ReporterId.Value);
        }

        if (request.AssignedToId.HasValue)
        {
            query = query.Where(i => i.AssignedToId == request.AssignedToId.Value);
        }

        if (request.InvestigatorId.HasValue)
        {
            query = query.Where(i => i.InvestigatorId == request.InvestigatorId.Value);
        }

        if (request.OnlyMyIncidents)
        {
            var currentUserId = _currentUserService.UserId;
            query = query.Where(i => 
                i.ReporterId == currentUserId || 
                i.AssignedToId == currentUserId || 
                i.InvestigatorId == currentUserId);
        }

        if (request.OnlyOpenIncidents)
        {
            query = query.Where(i => i.Status != SecurityIncidentStatus.Closed);
        }

        if (request.OnlyOverdueIncidents)
        {
            var cutoffDates = new Dictionary<SecuritySeverity, DateTime>
            {
                { SecuritySeverity.Critical, DateTime.UtcNow.AddDays(-1) },
                { SecuritySeverity.High, DateTime.UtcNow.AddDays(-3) },
                { SecuritySeverity.Medium, DateTime.UtcNow.AddDays(-7) },
                { SecuritySeverity.Low, DateTime.UtcNow.AddDays(-14) }
            };

            query = query.Where(i => 
                i.Status != SecurityIncidentStatus.Closed &&
                ((i.Severity == SecuritySeverity.Critical && i.CreatedAt < cutoffDates[SecuritySeverity.Critical]) ||
                 (i.Severity == SecuritySeverity.High && i.CreatedAt < cutoffDates[SecuritySeverity.High]) ||
                 (i.Severity == SecuritySeverity.Medium && i.CreatedAt < cutoffDates[SecuritySeverity.Medium]) ||
                 (i.Severity == SecuritySeverity.Low && i.CreatedAt < cutoffDates[SecuritySeverity.Low])));
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortDescending);

        // Select with joins for names
        var projectedQuery = query
            .Include(i => i.Reporter)
            .Include(i => i.AssignedTo)
            .Select(i => new SecurityIncidentListDto
            {
                Id = i.Id,
                IncidentNumber = i.IncidentNumber,
                Title = i.Title,
                IncidentType = i.IncidentType,
                Category = i.Category,
                Severity = i.Severity,
                Status = i.Status,
                ThreatLevel = i.ThreatLevel,
                IncidentDateTime = i.IncidentDateTime,
                Location = i.Location,
                ReporterName = i.Reporter.Name,
                AssignedToName = i.AssignedTo != null ? i.AssignedTo.Name : null,
                CreatedAt = i.CreatedAt
            });

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await projectedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<SecurityIncidentListDto>(items, totalCount, request.Page, request.PageSize);
    }

    private static IQueryable<Domain.Entities.Security.SecurityIncident> ApplySorting(
        IQueryable<Domain.Entities.Security.SecurityIncident> query, 
        string? sortBy, 
        bool sortDescending)
    {
        return sortBy?.ToLower() switch
        {
            "title" => sortDescending ? query.OrderByDescending(i => i.Title) : query.OrderBy(i => i.Title),
            "severity" => sortDescending ? query.OrderByDescending(i => i.Severity) : query.OrderBy(i => i.Severity),
            "status" => sortDescending ? query.OrderByDescending(i => i.Status) : query.OrderBy(i => i.Status),
            "threatlevel" => sortDescending ? query.OrderByDescending(i => i.ThreatLevel) : query.OrderBy(i => i.ThreatLevel),
            "incidentdate" => sortDescending ? query.OrderByDescending(i => i.IncidentDateTime) : query.OrderBy(i => i.IncidentDateTime),
            "location" => sortDescending ? query.OrderByDescending(i => i.Location) : query.OrderBy(i => i.Location),
            "type" => sortDescending ? query.OrderByDescending(i => i.IncidentType) : query.OrderBy(i => i.IncidentType),
            "category" => sortDescending ? query.OrderByDescending(i => i.Category) : query.OrderBy(i => i.Category),
            "createdat" => sortDescending ? query.OrderByDescending(i => i.CreatedAt) : query.OrderBy(i => i.CreatedAt),
            _ => query.OrderByDescending(i => i.CreatedAt) // Default sort by creation date descending
        };
    }
}