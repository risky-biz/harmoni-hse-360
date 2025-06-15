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

public class GetMyLicensesQueryHandler : IRequestHandler<GetMyLicensesQuery, PagedList<LicenseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMyLicensesQueryHandler> _logger;

    public GetMyLicensesQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetMyLicensesQueryHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedList<LicenseDto>> Handle(GetMyLicensesQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId <= 0)
        {
            return new PagedList<LicenseDto>(new List<LicenseDto>(), 0, request.Page, request.PageSize);
        }

        var query = _context.Licenses
            .Include(l => l.Attachments)
            .Include(l => l.Renewals)
            .Include(l => l.LicenseConditions)
            .Where(l => l.HolderId == currentUserId)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTermLower = request.SearchTerm.ToLower();
            query = query.Where(l =>
                l.Title.ToLower().Contains(searchTermLower) ||
                l.Description.ToLower().Contains(searchTermLower) ||
                l.LicenseNumber.ToLower().Contains(searchTermLower) ||
                l.IssuingAuthority.ToLower().Contains(searchTermLower));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(l => l.Status == request.Status.Value);
        }

        if (request.Type.HasValue)
        {
            query = query.Where(l => l.Type == request.Type.Value);
        }

        var now = DateTime.UtcNow;

        if (request.IsExpiring.HasValue && request.IsExpiring.Value)
        {
            query = query.Where(l => 
                l.ExpiryDate >= now && 
                l.ExpiryDate <= now.AddDays(30));
        }

        if (request.IsExpired.HasValue && request.IsExpired.Value)
        {
            query = query.Where(l => l.ExpiryDate < now);
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
            "Retrieved {Count} licenses for user {UserId} (Page {Page}/{TotalPages})",
            licenseDtos.Count,
            currentUserId,
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
            _ => l => l.CreatedAt
        };

        return sortDirection.ToLower() == "asc" 
            ? query.OrderBy(keySelector) 
            : query.OrderByDescending(keySelector);
    }
}