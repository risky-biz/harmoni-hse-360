using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.DisposalProviders.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.DisposalProviders.Queries;

public class GetExpiringProvidersQueryHandler : IRequestHandler<GetExpiringProvidersQuery, List<DisposalProviderDto>>
{
    private readonly IApplicationDbContext _context;

    public GetExpiringProvidersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DisposalProviderDto>> Handle(GetExpiringProvidersQuery request, CancellationToken cancellationToken)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(request.DaysAhead);

        return await _context.DisposalProviders
            .Where(p => p.IsActive && p.LicenseExpiryDate <= cutoffDate)
            .OrderBy(p => p.LicenseExpiryDate)
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