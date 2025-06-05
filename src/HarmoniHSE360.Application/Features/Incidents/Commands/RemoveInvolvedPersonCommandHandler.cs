using HarmoniHSE360.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarmoniHSE360.Application.Features.Incidents.Commands;

public class RemoveInvolvedPersonCommandHandler : IRequestHandler<RemoveInvolvedPersonCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IIncidentAuditService _auditService;

    public RemoveInvolvedPersonCommandHandler(IApplicationDbContext context, IIncidentAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<Unit> Handle(RemoveInvolvedPersonCommand request, CancellationToken cancellationToken)
    {
        var involvedPerson = await _context.IncidentInvolvedPersons
            .Include(ip => ip.Person)
            .FirstOrDefaultAsync(ip => ip.IncidentId == request.IncidentId && ip.PersonId == request.PersonId, cancellationToken);

        if (involvedPerson == null)
        {
            throw new InvalidOperationException($"Person with ID {request.PersonId} is not involved in incident {request.IncidentId}.");
        }

        var personName = involvedPerson.Person?.Name ?? $"Person ID {request.PersonId}";

        _context.IncidentInvolvedPersons.Remove(involvedPerson);
        await _context.SaveChangesAsync(cancellationToken);

        // Log audit trail
        await _auditService.LogActionAsync(request.IncidentId, $"Involved person removed: {personName}");

        // Save audit trail entry
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}