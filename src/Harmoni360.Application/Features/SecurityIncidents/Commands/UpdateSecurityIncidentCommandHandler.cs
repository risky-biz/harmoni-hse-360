using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.SecurityIncidents.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.SecurityIncidents.Commands;

public class UpdateSecurityIncidentCommandHandler : IRequestHandler<UpdateSecurityIncidentCommand, SecurityIncidentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;

    public UpdateSecurityIncidentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMediator mediator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    public async Task<SecurityIncidentDto> Handle(UpdateSecurityIncidentCommand request, CancellationToken cancellationToken)
    {
        var incident = await _context.SecurityIncidents
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (incident == null)
        {
            throw new KeyNotFoundException($"Security incident with ID {request.Id} not found");
        }

        // TODO: Implement proper update methods in domain entity
        // For now, only update what's available through existing business methods

        // Update geolocation if provided
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            incident.SetGeoLocation(request.Latitude.Value, request.Longitude.Value);
        }

        // Update threat assessment if provided
        if (request.ThreatActorType.HasValue)
        {
            incident.UpdateThreatAssessment(
                incident.ThreatLevel,
                request.ThreatActorType,
                request.ThreatActorDescription,
                request.IsInternalThreat,
                _currentUserService.Name);
        }

        // Update impact assessment
        incident.UpdateImpactAssessment(
            request.Impact,
            request.AffectedPersonsCount,
            request.EstimatedLoss,
            request.DataBreachOccurred,
            _currentUserService.Name);

        // Update containment if provided
        if (request.ContainmentDateTime.HasValue && !string.IsNullOrEmpty(request.ContainmentActions))
        {
            incident.RecordContainment(request.ContainmentActions, request.ContainmentDateTime.Value, _currentUserService.Name);
        }

        // Resolve incident if provided
        if (request.ResolutionDateTime.HasValue && !string.IsNullOrEmpty(request.RootCause))
        {
            incident.ResolveIncident(request.RootCause, request.ResolutionDateTime.Value, _currentUserService.Name);
        }

        // Update assignments
        if (request.AssignedToId.HasValue)
        {
            incident.AssignTo(request.AssignedToId.Value, _currentUserService.Name);
        }

        if (request.InvestigatorId.HasValue)
        {
            incident.AssignInvestigator(request.InvestigatorId.Value, _currentUserService.Name);
        }

        // Save changes
        await _context.SaveChangesAsync(cancellationToken);

        // Publish domain events
        foreach (var domainEvent in incident.DomainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        // Map to DTO
        return await MapToDto(incident, cancellationToken);
    }

    private async Task<SecurityIncidentDto> MapToDto(Domain.Entities.Security.SecurityIncident incident, CancellationToken cancellationToken)
    {
        var reporter = await _context.Users
            .Where(u => u.Id == incident.ReporterId)
            .Select(u => new { u.Name, u.Email })
            .FirstOrDefaultAsync(cancellationToken);

        User? assignedTo = null;
        if (incident.AssignedToId.HasValue)
        {
            assignedTo = await _context.Users.FindAsync(incident.AssignedToId.Value);
        }

        User? investigator = null;
        if (incident.InvestigatorId.HasValue)
        {
            investigator = await _context.Users.FindAsync(incident.InvestigatorId.Value);
        }

        return new SecurityIncidentDto
        {
            Id = incident.Id,
            IncidentNumber = incident.IncidentNumber,
            Title = incident.Title,
            Description = incident.Description,
            IncidentType = incident.IncidentType,
            Category = incident.Category,
            Severity = incident.Severity,
            Status = incident.Status,
            ThreatLevel = incident.ThreatLevel,
            IncidentDateTime = incident.IncidentDateTime,
            DetectionDateTime = incident.DetectionDateTime,
            Location = incident.Location,
            Latitude = incident.GeoLocation?.Latitude,
            Longitude = incident.GeoLocation?.Longitude,
            ThreatActorType = incident.ThreatActorType,
            ThreatActorDescription = incident.ThreatActorDescription,
            IsInternalThreat = incident.IsInternalThreat,
            Impact = incident.Impact,
            AffectedPersonsCount = incident.AffectedPersonsCount,
            EstimatedLoss = incident.EstimatedLoss,
            DataBreachOccurred = incident.DataBreachOccurred,
            ContainmentDateTime = incident.ContainmentDateTime,
            ResolutionDateTime = incident.ResolutionDateTime,
            ContainmentActions = incident.ContainmentActions,
            RootCause = incident.RootCause,
            ReporterName = reporter?.Name,
            ReporterEmail = reporter?.Email,
            AssignedToName = assignedTo?.Name,
            InvestigatorName = investigator?.Name,
            CreatedAt = incident.CreatedAt,
            LastModifiedAt = incident.LastModifiedAt,
            CreatedBy = incident.CreatedBy,
            LastModifiedBy = incident.LastModifiedBy
        };
    }
}