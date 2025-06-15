using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.DisposalProviders.DTOs;
using Harmoni360.Domain.Entities.Waste;
using MediatR;

namespace Harmoni360.Application.Features.DisposalProviders.Commands;

public class CreateDisposalProviderCommandHandler : IRequestHandler<CreateDisposalProviderCommand, DisposalProviderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateDisposalProviderCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<DisposalProviderDto> Handle(CreateDisposalProviderCommand request, CancellationToken cancellationToken)
    {
        var entity = DisposalProvider.Create(request.Name, request.LicenseNumber, request.LicenseExpiryDate, _currentUser.Email);
        _context.DisposalProviders.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new DisposalProviderDto
        {
            Id = entity.Id,
            Name = entity.Name,
            LicenseNumber = entity.LicenseNumber,
            LicenseExpiryDate = entity.LicenseExpiryDate
        };
    }
}
