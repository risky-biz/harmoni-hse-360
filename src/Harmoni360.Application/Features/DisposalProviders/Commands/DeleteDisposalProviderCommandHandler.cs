using Harmoni360.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.DisposalProviders.Commands;

public class DeleteDisposalProviderCommandHandler : IRequestHandler<DeleteDisposalProviderCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteDisposalProviderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteDisposalProviderCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.DisposalProviders
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException($"Disposal provider with ID {request.Id} not found");
        }

        // Check if provider is being used in any disposal records
        var isInUse = await _context.WasteDisposalRecords
            .AnyAsync(r => r.DisposalProviderId == request.Id, cancellationToken);

        if (isInUse)
        {
            throw new InvalidOperationException("Cannot delete disposal provider as it is being used in waste disposal records");
        }

        _context.DisposalProviders.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}