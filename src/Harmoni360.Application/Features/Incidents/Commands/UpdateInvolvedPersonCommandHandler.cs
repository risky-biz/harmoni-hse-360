using Harmoni360.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Application.Features.Incidents.Commands;

public class UpdateInvolvedPersonCommandHandler : IRequestHandler<UpdateInvolvedPersonCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IIncidentAuditService _auditService;
    private readonly ILogger<UpdateInvolvedPersonCommandHandler> _logger;

    public UpdateInvolvedPersonCommandHandler(IApplicationDbContext context, IIncidentAuditService auditService, ILogger<UpdateInvolvedPersonCommandHandler> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateInvolvedPersonCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing UpdateInvolvedPersonCommand: IncidentId={IncidentId}, PersonId={PersonId}, InvolvementType={InvolvementType}", 
            request.IncidentId, request.PersonId, request.InvolvementType);

        // For manual entries, PersonId might be null, so we need to look up by the IncidentInvolvedPerson record ID
        // First try to find by PersonId (for existing users)
        var involvedPerson = await _context.IncidentInvolvedPersons
            .Include(ip => ip.Person)
            .FirstOrDefaultAsync(ip => ip.IncidentId == request.IncidentId && ip.PersonId == request.PersonId, cancellationToken);

        _logger.LogInformation("Found involved person by PersonId: {Found}", involvedPerson != null);

        // If not found by PersonId, try to find by the IncidentInvolvedPerson ID (for manual entries)
        if (involvedPerson == null)
        {
            involvedPerson = await _context.IncidentInvolvedPersons
                .Include(ip => ip.Person)
                .FirstOrDefaultAsync(ip => ip.IncidentId == request.IncidentId && ip.Id == request.PersonId, cancellationToken);
            
            _logger.LogInformation("Found involved person by record ID: {Found}", involvedPerson != null);
        }

        if (involvedPerson == null)
        {
            _logger.LogWarning("Person with ID {PersonId} is not involved in incident {IncidentId}", request.PersonId, request.IncidentId);
            throw new InvalidOperationException($"Person with ID {request.PersonId} is not involved in incident {request.IncidentId}.");
        }

        // Get the person name for audit logging
        var personName = involvedPerson.Person?.Name ?? involvedPerson.ManualPersonName ?? $"Person ID {request.PersonId}";
        var oldInvolvementType = involvedPerson.InvolvementType.ToString();

        try
        {
            involvedPerson.UpdateDetails(request.InvolvementType, request.InjuryDescription);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to update involved person: {ex.Message}", ex);
        }

        // Log audit trail
        await _auditService.LogActionAsync(request.IncidentId, "Person Updated", $"Involved person updated: {personName} (changed from {oldInvolvementType} to {request.InvolvementType})");

        // Save audit trail entry
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}