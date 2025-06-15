using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.DisposalProviders.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.DisposalProviders.Queries;

public class GetDisposalProviderByIdQueryHandler : IRequestHandler<GetDisposalProviderByIdQuery, DisposalProviderDto?>
{
    private readonly IApplicationDbContext _context;

    public GetDisposalProviderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DisposalProviderDto?> Handle(GetDisposalProviderByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.DisposalProviders
            .Where(p => p.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}