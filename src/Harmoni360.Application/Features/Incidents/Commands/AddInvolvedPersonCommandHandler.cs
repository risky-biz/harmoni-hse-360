using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Application.Features.Incidents.Commands;

public class AddInvolvedPersonCommandHandler : IRequestHandler<AddInvolvedPersonCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IIncidentAuditService _auditService;
    private readonly ILogger<AddInvolvedPersonCommandHandler> _logger;

    public AddInvolvedPersonCommandHandler(IApplicationDbContext context, IIncidentAuditService auditService, ILogger<AddInvolvedPersonCommandHandler> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Unit> Handle(AddInvolvedPersonCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing AddInvolvedPersonCommand: IncidentId={IncidentId}, PersonId={PersonId}, ManualName={ManualName}", 
            request.IncidentId, request.PersonId, request.ManualPersonName);

        var incident = await _context.Incidents
            .Include(i => i.InvolvedPersons)
            .FirstOrDefaultAsync(i => i.Id == request.IncidentId, cancellationToken);

        if (incident == null)
        {
            throw new InvalidOperationException($"Incident with ID {request.IncidentId} not found.");
        }

        // Handle manual person entry (when PersonId is 0 or manual data is provided)
        if (!string.IsNullOrWhiteSpace(request.ManualPersonName) || request.PersonId <= 0)
        {
            // Validate that manual person name is provided for manual entries
            if (string.IsNullOrWhiteSpace(request.ManualPersonName))
            {
                throw new InvalidOperationException("Manual person name is required for manual entries.");
            }

            // For manual entries, use null PersonId and pass manual person data
            incident.AddInvolvedPerson(
                personId: null, 
                involvementType: request.InvolvementType, 
                injuryDescription: request.InjuryDescription,
                manualPersonName: request.ManualPersonName.Trim(),
                manualPersonEmail: request.ManualPersonEmail?.Trim()
            );
            
            // Log audit trail
            await _auditService.LogActionAsync(request.IncidentId, "Person Added", $"Involved person added: {request.ManualPersonName.Trim()} ({request.InvolvementType})");
        }
        else
        {
            // For existing users (PersonId > 0)
            if (request.PersonId <= 0)
            {
                throw new InvalidOperationException("Invalid PersonId for existing user entry.");
            }

            // Check if person is already involved (for existing users)
            if (incident.InvolvedPersons.Any(ip => ip.PersonId == request.PersonId))
            {
                throw new InvalidOperationException($"Person with ID {request.PersonId} is already involved in this incident.");
            }

            // Get person name for audit logging
            var person = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.PersonId, cancellationToken);
            if (person == null)
            {
                throw new InvalidOperationException($"User with ID {request.PersonId} not found.");
            }
            
            // Use the domain method to add involved person
            incident.AddInvolvedPerson(
                personId: request.PersonId, 
                involvementType: request.InvolvementType, 
                injuryDescription: request.InjuryDescription
            );
            
            // Log audit trail
            await _auditService.LogActionAsync(request.IncidentId, "Person Added", $"Involved person added: {person.Name} ({request.InvolvementType})");
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}