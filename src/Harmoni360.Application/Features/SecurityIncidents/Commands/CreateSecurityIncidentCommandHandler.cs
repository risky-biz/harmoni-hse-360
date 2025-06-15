using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.SecurityIncidents.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Entities.Security;
using Harmoni360.Domain.Events;
using Harmoni360.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.SecurityIncidents.Commands;

public class CreateSecurityIncidentCommandHandler : IRequestHandler<CreateSecurityIncidentCommand, SecurityIncidentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMediator _mediator;

    public CreateSecurityIncidentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService,
        IMediator mediator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _mediator = mediator;
    }

    public async Task<SecurityIncidentDto> Handle(CreateSecurityIncidentCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        // Create GeoLocation if coordinates provided
        GeoLocation? geoLocation = null;
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            geoLocation = GeoLocation.Create(request.Latitude.Value, request.Longitude.Value);
        }

        // Create security incident
        var incident = SecurityIncident.Create(
            request.IncidentType,
            request.Category,
            request.Title,
            request.Description,
            request.Severity,
            request.IncidentDateTime,
            request.Location,
            currentUserId,
            _currentUserService.Name);

        // Set geo location if provided
        if (geoLocation != null)
        {
            incident.SetGeoLocation(geoLocation.Latitude, geoLocation.Longitude);
        }

        // Set optional threat assessment fields
        if (request.ThreatActorType.HasValue)
        {
            incident.UpdateThreatAssessment(
                incident.ThreatLevel,
                request.ThreatActorType,
                request.ThreatActorDescription,
                request.IsInternalThreat,
                _currentUserService.Name);
        }

        // Set impact assessment fields
        if (request.AffectedPersonsCount.HasValue || request.EstimatedLoss.HasValue || request.DataBreachSuspected || request.Impact != Domain.Enums.SecurityImpact.None)
        {
            incident.UpdateImpactAssessment(
                request.Impact,
                request.AffectedPersonsCount,
                request.EstimatedLoss,
                request.DataBreachSuspected,
                _currentUserService.Name);
        }

        if (!string.IsNullOrEmpty(request.ContainmentActions))
        {
            incident.RecordContainment(
                request.ContainmentActions, 
                DateTime.UtcNow, 
                _currentUserService.Name);
        }

        // Assign if specified
        if (request.AssignedToId.HasValue)
        {
            incident.AssignTo(request.AssignedToId.Value, _currentUserService.Name);
        }

        if (request.InvestigatorId.HasValue)
        {
            incident.AssignInvestigator(request.InvestigatorId.Value, _currentUserService.Name);
        }


        // Add to context first to get the ID
        _context.SecurityIncidents.Add(incident);
        await _context.SaveChangesAsync(cancellationToken);

        // Handle file attachments
        if (request.Attachments?.Any() == true)
        {
            foreach (var file in request.Attachments)
            {
                var uploadResult = await _fileStorageService.UploadAsync(
                    file.OpenReadStream(),
                    file.FileName,
                    file.ContentType,
                    "security-incidents");
                
                incident.AddAttachment(
                    file.FileName,
                    uploadResult.FilePath,
                    file.Length,
                    Domain.Enums.SecurityAttachmentType.Evidence,
                    _currentUserService.Name);
            }
        }

        // Handle involved persons
        if (request.InvolvedPersonIds?.Any() == true)
        {
            foreach (var personId in request.InvolvedPersonIds)
            {
                incident.AddInvolvedPerson(personId, "Involved", false);
            }
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


    private async Task<SecurityIncidentDto> MapToDto(SecurityIncident incident, CancellationToken cancellationToken)
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
            LastModifiedAt = incident.LastModifiedAt
        };
    }
}