using Harmoni360.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.Inspections.Commands;

public class StartInspectionCommandHandler : IRequestHandler<StartInspectionCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public StartInspectionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(StartInspectionCommand request, CancellationToken cancellationToken)
    {
        var inspection = await _context.Inspections
            .FirstOrDefaultAsync(i => i.Id == request.InspectionId, cancellationToken);

        if (inspection == null)
            throw new ArgumentException($"Inspection with ID {request.InspectionId} not found");

        inspection.StartInspection();

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}