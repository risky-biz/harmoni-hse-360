using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarmoniHSE360.Application.Features.Incidents.Commands;

public class AddInvolvedPersonCommandHandler : IRequestHandler<AddInvolvedPersonCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IIncidentAuditService _auditService;

    public AddInvolvedPersonCommandHandler(IApplicationDbContext context, IIncidentAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<Unit> Handle(AddInvolvedPersonCommand request, CancellationToken cancellationToken)
    {
        var incident = await _context.Incidents
            .Include(i => i.InvolvedPersons)
            .FirstOrDefaultAsync(i => i.Id == request.IncidentId, cancellationToken);

        if (incident == null)
        {
            throw new InvalidOperationException($"Incident with ID {request.IncidentId} not found.");
        }

        // Check if person is already involved
        if (incident.InvolvedPersons.Any(ip => ip.PersonId == request.PersonId))
        {
            throw new InvalidOperationException($"Person with ID {request.PersonId} is already involved in this incident.");
        }

        // Get person name for audit logging
        var person = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.PersonId, cancellationToken);
        var personName = person?.Name ?? $"Person ID {request.PersonId}";

        // Use the domain method to add involved person
        incident.AddInvolvedPerson(request.PersonId, request.InvolvementType, request.InjuryDescription);
        
        await _context.SaveChangesAsync(cancellationToken);

        // Log audit trail
        await _auditService.LogActionAsync(request.IncidentId, $"Involved person added: {personName} ({request.InvolvementType})");
        
        // Save audit trail entry
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}