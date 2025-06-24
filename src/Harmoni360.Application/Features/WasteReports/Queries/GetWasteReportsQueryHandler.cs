using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.WasteReports.DTOs;
using System.Linq.Expressions;
using Harmoni360.Domain.Entities.Waste;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace Harmoni360.Application.Features.WasteReports.Queries;

public class GetWasteReportsQueryHandler : IRequestHandler<GetWasteReportsQuery, PagedList<WasteReportDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetWasteReportsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedList<WasteReportDto>> Handle(GetWasteReportsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.WasteReports
            .Include(w => w.Reporter)
            .Include(w => w.Attachments)
            .AsQueryable();

        // Apply filters
        query = ApplyFilters(query, request);

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortDescending);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<WasteReportDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedList<WasteReportDto>(items, totalCount, request.Page, request.PageSize);
    }

    private static IQueryable<WasteReport> ApplyFilters(IQueryable<WasteReport> query, GetWasteReportsQuery request)
    {
        if (request.Category.HasValue)
        {
            query = query.Where(w => w.Category == request.Category.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(w => w.DisposalStatus == (WasteDisposalStatus)request.Status.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(w => w.GeneratedDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(w => w.GeneratedDate <= request.ToDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Location))
        {
            query = query.Where(w => w.Location.Contains(request.Location));
        }

        if (request.ReporterId.HasValue)
        {
            query = query.Where(w => w.ReporterId == request.ReporterId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.ToLower();
            query = query.Where(w => 
                w.Title.ToLower().Contains(searchLower) || 
                w.Description.ToLower().Contains(searchLower) ||
                w.Location.ToLower().Contains(searchLower));
        }

        return query;
    }

    private static IQueryable<WasteReport> ApplySorting(IQueryable<WasteReport> query, string? sortBy, bool sortDescending)
    {
        Expression<Func<WasteReport, object>> sortExpression = sortBy?.ToLower() switch
        {
            "title" => w => w.Title,
            "category" => w => w.Category,
            "status" => w => w.DisposalStatus,
            "location" => w => w.Location,
            "reporter" => w => w.Reporter != null ? w.Reporter.Name : string.Empty,
            "createdat" => w => w.CreatedAt,
            _ => w => w.GeneratedDate // Default sort by GeneratedDate
        };

        return sortDescending 
            ? query.OrderByDescending(sortExpression) 
            : query.OrderBy(sortExpression);
    }
}
