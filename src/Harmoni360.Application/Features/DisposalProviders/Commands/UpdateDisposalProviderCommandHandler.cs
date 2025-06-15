using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.DisposalProviders.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.DisposalProviders.Commands;

public class UpdateDisposalProviderCommandHandler : IRequestHandler<UpdateDisposalProviderCommand, DisposalProviderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateDisposalProviderCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<DisposalProviderDto> Handle(UpdateDisposalProviderCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.DisposalProviders
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException($"Disposal provider with ID {request.Id} not found");
        }

        entity.Update(request.Name, request.LicenseNumber, request.LicenseExpiryDate);
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