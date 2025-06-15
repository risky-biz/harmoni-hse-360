using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.Licenses.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using System.Linq.Expressions;

namespace Harmoni360.Application.Features.Licenses.Queries;

public class GetExpiringLicensesQueryHandler : IRequestHandler<GetExpiringLicensesQuery, PagedList<LicenseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetExpiringLicensesQueryHandler> _logger;

    public GetExpiringLicensesQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetExpiringLicensesQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedList<LicenseDto>> Handle(GetExpiringLicensesQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var futureDate = now.AddDays(request.DaysAhead);

        var query = _context.Licenses
            .Include(l => l.Attachments)
            .Include(l => l.Renewals)
            .Include(l => l.LicenseConditions)
            .Where(l => l.Status == LicenseStatus.Active || l.Status == LicenseStatus.Approved)
            .AsQueryable();

        // Filter by expiry date
        if (request.IncludeOverdue)
        {
            // Include overdue and expiring licenses
            query = query.Where(l => l.ExpiryDate <= futureDate);
        }
        else
        {
            // Only include future expiring licenses
            query = query.Where(l => l.ExpiryDate >= now && l.ExpiryDate <= futureDate);
        }

        // Filter by department if specified
        if (!string.IsNullOrWhiteSpace(request.Department))
        {
            query = query.Where(l => l.Department.Contains(request.Department));
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortDirection);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var licenses = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var licenseDtos = _mapper.Map<List<LicenseDto>>(licenses);

        _logger.LogInformation(
            "Retrieved {Count} expiring licenses (Page {Page}/{TotalPages}, {DaysAhead} days ahead)",
            licenseDtos.Count,
            request.Page,
            (int)Math.Ceiling(totalCount / (double)request.PageSize),
            request.DaysAhead);

        return new PagedList<LicenseDto>(
            licenseDtos,
            totalCount,
            request.Page,
            request.PageSize);
    }

    private IQueryable<License> ApplySorting(IQueryable<License> query, string sortBy, string sortDirection)
    {
        Expression<Func<License, object>> keySelector = sortBy.ToLower() switch
        {
            "title" => l => l.Title,
            "licensenumber" => l => l.LicenseNumber,
            "type" => l => l.Type,
            "priority" => l => l.Priority,
            "issuingauthority" => l => l.IssuingAuthority,
            "holdername" => l => l.HolderName,
            "department" => l => l.Department,
            "expirydate" => l => l.ExpiryDate,
            _ => l => l.ExpiryDate
        };

        return sortDirection.ToLower() == "asc" 
            ? query.OrderBy(keySelector) 
            : query.OrderByDescending(keySelector);
    }
}