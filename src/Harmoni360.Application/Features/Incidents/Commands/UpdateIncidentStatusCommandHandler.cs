using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.Incidents.Commands;

public class UpdateIncidentStatusCommandHandler : IRequestHandler<UpdateIncidentStatusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IIncidentAuditService _auditService;

    public UpdateIncidentStatusCommandHandler(IApplicationDbContext context, IIncidentAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task Handle(UpdateIncidentStatusCommand request, CancellationToken cancellationToken)
    {
        var incident = await _context.Incidents
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (incident == null)
        {
            throw new ArgumentException($"Incident with ID {request.Id} not found");
        }

        // Parse status enum from string
        if (!Enum.TryParse<IncidentStatus>(request.Status, out var newStatus))
        {
            throw new ArgumentException($"Invalid status: {request.Status}");
        }

        // Store old status for audit logging
        var oldStatus = incident.Status;

        // Update the status
        incident.UpdateStatus(newStatus);

        // Log the status change for audit trail BEFORE saving
        var description = $"Status changed from {oldStatus} to {newStatus}";
        if (!string.IsNullOrWhiteSpace(request.Comment))
        {
            description += $". Comment: {request.Comment}";
        }
        await _auditService.LogFieldChangeAsync(incident.Id, "Status", oldStatus.ToString(), newStatus.ToString(), description);

        // Save changes - this will save both the incident status update and the audit log
        await _context.SaveChangesAsync(cancellationToken);
    }
}