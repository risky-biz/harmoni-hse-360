using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.DisposalProviders.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.DisposalProviders.Queries;

public class SearchDisposalProvidersQueryHandler : IRequestHandler<SearchDisposalProvidersQuery, List<DisposalProviderDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchDisposalProvidersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DisposalProviderDto>> Handle(SearchDisposalProvidersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.DisposalProviders.AsQueryable();

        // Filter by active status unless including inactive
        if (request.IncludeInactive != true)
        {
            query = query.Where(p => p.IsActive);
        }

        // Filter by specific status
        if (request.Status.HasValue)
        {
            query = query.Where(p => p.Status == request.Status.Value);
        }

        // Filter by search term (name or license number)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(searchLower) || 
                p.LicenseNumber.ToLower().Contains(searchLower));
        }

        // Filter by expiring licenses
        if (request.OnlyExpiring == true)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(request.ExpiringDays ?? 30);
            query = query.Where(p => p.LicenseExpiryDate <= cutoffDate);
        }

        return await query
            .OrderBy(p => p.Name)
            .Select(p => new DisposalProviderDto
            {
                Id = p.Id,
                Name = p.Name,
                LicenseNumber = p.LicenseNumber,
                LicenseExpiryDate = p.LicenseExpiryDate,
                Status = p.Status,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                CreatedBy = p.CreatedBy,
                LastModifiedAt = p.LastModifiedAt,
                LastModifiedBy = p.LastModifiedBy
            })
            .ToListAsync(cancellationToken);
    }
}