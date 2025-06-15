using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.Licenses.DTOs;
using Harmoni360.Domain.Entities;
using System.Linq.Expressions;

namespace Harmoni360.Application.Features.Licenses.Queries;

public class GetLicensesQueryHandler : IRequestHandler<GetLicensesQuery, PagedList<LicenseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetLicensesQueryHandler> _logger;

    public GetLicensesQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetLicensesQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedList<LicenseDto>> Handle(GetLicensesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Licenses
            .Include(l => l.Attachments)
            .Include(l => l.Renewals)
            .Include(l => l.LicenseConditions)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTermLower = request.SearchTerm.ToLower();
            query = query.Where(l =>
                l.Title.ToLower().Contains(searchTermLower) ||
                l.Description.ToLower().Contains(searchTermLower) ||
                l.LicenseNumber.ToLower().Contains(searchTermLower) ||
                l.IssuingAuthority.ToLower().Contains(searchTermLower) ||
                l.HolderName.ToLower().Contains(searchTermLower));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(l => l.Status == request.Status.Value);
        }

        if (request.Type.HasValue)
        {
            query = query.Where(l => l.Type == request.Type.Value);
        }

        if (request.Priority.HasValue)
        {
            query = query.Where(l => l.Priority == request.Priority.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.IssuingAuthority))
        {
            query = query.Where(l => l.IssuingAuthority.Contains(request.IssuingAuthority));
        }

        if (!string.IsNullOrWhiteSpace(request.Department))
        {
            query = query.Where(l => l.Department.Contains(request.Department));
        }

        // Date filters
        if (request.ExpiryDateFrom.HasValue)
        {
            query = query.Where(l => l.ExpiryDate >= request.ExpiryDateFrom.Value);
        }

        if (request.ExpiryDateTo.HasValue)
        {
            query = query.Where(l => l.ExpiryDate <= request.ExpiryDateTo.Value);
        }

        // Expiring and expired filters
        var now = DateTime.UtcNow;
        
        if (request.IsExpiring.HasValue && request.IsExpiring.Value)
        {
            query = query.Where(l => l.ExpiryDate >= now && l.ExpiryDate <= now.AddDays(30));
        }

        if (request.IsExpired.HasValue && request.IsExpired.Value)
        {
            query = query.Where(l => l.ExpiryDate < now);
        }

        if (request.RenewalDue.HasValue && request.RenewalDue.Value)
        {
            query = query.Where(l => l.RenewalRequired && 
                l.NextRenewalDate.HasValue && 
                l.NextRenewalDate.Value <= now.AddDays(l.RenewalPeriodDays));
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
            "Retrieved {Count} licenses (Page {Page}/{TotalPages})",
            licenseDtos.Count,
            request.Page,
            (int)Math.Ceiling(totalCount / (double)request.PageSize));

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
            "status" => l => l.Status,
            "priority" => l => l.Priority,
            "issueddate" => l => l.IssuedDate,
            "expirydate" => l => l.ExpiryDate,
            "issuingauthority" => l => l.IssuingAuthority,
            "holdername" => l => l.HolderName,
            "department" => l => l.Department,
            _ => l => l.CreatedAt
        };

        return sortDirection.ToLower() == "asc" 
            ? query.OrderBy(keySelector) 
            : query.OrderByDescending(keySelector);
    }
}