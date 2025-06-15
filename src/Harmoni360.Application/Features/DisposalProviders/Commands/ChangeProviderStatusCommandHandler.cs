using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.DisposalProviders.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.DisposalProviders.Commands;

public class ChangeProviderStatusCommandHandler : IRequestHandler<ChangeProviderStatusCommand, DisposalProviderDto>
{
    private readonly IApplicationDbContext _context;

    public ChangeProviderStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DisposalProviderDto> Handle(ChangeProviderStatusCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.DisposalProviders
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException($"Disposal provider with ID {request.Id} not found");
        }

        entity.ChangeStatus(request.Status);
        await _context.SaveChangesAsync(cancellationToken);

        return new DisposalProviderDto
        {
            Id = entity.Id,
            Name = entity.Name,
            LicenseNumber = entity.LicenseNumber,
            LicenseExpiryDate = entity.LicenseExpiryDate,
            Status = entity.Status,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            LastModifiedAt = entity.LastModifiedAt,
            LastModifiedBy = entity.LastModifiedBy
        };
    }
}