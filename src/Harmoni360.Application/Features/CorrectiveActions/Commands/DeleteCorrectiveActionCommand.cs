using Harmoni360.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.CorrectiveActions.Commands;

public record DeleteCorrectiveActionCommand(int Id) : IRequest<Unit>;

public class DeleteCorrectiveActionCommandHandler : IRequestHandler<DeleteCorrectiveActionCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IIncidentAuditService _auditService;

    public DeleteCorrectiveActionCommandHandler(IApplicationDbContext context, IIncidentAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<Unit> Handle(DeleteCorrectiveActionCommand request, CancellationToken cancellationToken)
    {
        var correctiveAction = await _context.CorrectiveActions
            .FirstOrDefaultAsync(ca => ca.Id == request.Id, cancellationToken);

        if (correctiveAction == null)
        {
            throw new InvalidOperationException($"Corrective action with ID {request.Id} not found");
        }

        // Log audit trail before deletion
        await _auditService.LogCorrectiveActionRemovedAsync(correctiveAction.IncidentId, correctiveAction.Description);

        _context.CorrectiveActions.Remove(correctiveAction);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}