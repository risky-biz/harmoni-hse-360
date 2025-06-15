using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.DisposalProviders.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.DisposalProviders.Queries;

public class GetDisposalProvidersQueryHandler : IRequestHandler<GetDisposalProvidersQuery, List<DisposalProviderDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDisposalProvidersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DisposalProviderDto>> Handle(GetDisposalProvidersQuery request, CancellationToken cancellationToken)
    {
        return await _context.DisposalProviders
            .Where(p => p.IsActive)
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
