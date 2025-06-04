using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Application.Features.Incidents.DTOs;

namespace HarmoniHSE360.Application.Features.Incidents.Commands;

public class UpdateIncidentCommandHandler : IRequestHandler<UpdateIncidentCommand, IncidentDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateIncidentCommandHandler> _logger;
    private readonly IIncidentAuditService _auditService;
    private readonly ICacheService _cache;

    public UpdateIncidentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<UpdateIncidentCommandHandler> logger,
        IIncidentAuditService auditService,
        ICacheService cache)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
        _auditService = auditService;
        _cache = cache;
    }

    public async Task<IncidentDto?> Handle(UpdateIncidentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating incident {IncidentId} by user {UserEmail}", 
            request.Id, _currentUserService.Email);

        var incident = await _context.Incidents
            .Include(i => i.Attachments)
            .Include(i => i.InvolvedPersons)
            .Include(i => i.CorrectiveActions)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (incident == null)
        {
            _logger.LogWarning("Incident {IncidentId} not found", request.Id);
            return null;
        }

        // Track changes for audit trail
        var oldTitle = incident.Title;
        var oldDescription = incident.Description;
        var oldStatus = incident.Status.ToString();
        var oldSeverity = incident.Severity.ToString();
        var oldLocation = incident.Location;

        // Update basic details
        incident.UpdateDetails(request.Title, request.Description, _currentUserService.Email!);
        
        // Log title change
        if (oldTitle != request.Title)
        {
            await _auditService.LogFieldChangeAsync(incident.Id, "Title", oldTitle, request.Title);
        }

        // Log description change
        if (oldDescription != request.Description)
        {
            await _auditService.LogFieldChangeAsync(incident.Id, "Description", 
                oldDescription.Length > 100 ? oldDescription.Substring(0, 100) + "..." : oldDescription,
                request.Description.Length > 100 ? request.Description.Substring(0, 100) + "..." : request.Description);
        }

        // Update and log severity change
        if (incident.Severity != request.Severity)
        {
            incident.UpdateSeverity(request.Severity);
            await _auditService.LogSeverityChangeAsync(incident.Id, oldSeverity, request.Severity.ToString());
        }

        // Update status if changed
        if (incident.Status != request.Status)
        {
            incident.UpdateStatus(request.Status);
            await _auditService.LogStatusChangeAsync(incident.Id, oldStatus, request.Status.ToString());
        }

        // Update and log location change
        if (oldLocation != request.Location)
        {
            incident.UpdateLocation(request.Location);
            await _auditService.LogFieldChangeAsync(incident.Id, "Location", oldLocation, request.Location);
        }

        // Update location
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            incident.SetGeoLocation(request.Latitude.Value, request.Longitude.Value);
        }

        await _context.SaveChangesAsync(cancellationToken);
        
        // Invalidate incident caches to ensure fresh data
        await InvalidateIncidentCaches(incident.ReporterId?.ToString() ?? "unknown");

        _logger.LogInformation("Incident {IncidentId} updated successfully", incident.Id);

        // Return updated DTO
        return new IncidentDto
        {
            Id = incident.Id,
            Title = incident.Title,
            Description = incident.Description,
            Severity = incident.Severity.ToString(),
            Status = incident.Status.ToString(),
            IncidentDate = incident.IncidentDate,
            Location = incident.Location,
            Latitude = incident.GeoLocation?.Latitude,
            Longitude = incident.GeoLocation?.Longitude,
            ReporterName = incident.ReporterName,
            ReporterEmail = incident.ReporterEmail,
            ReporterDepartment = incident.ReporterDepartment,
            InjuryType = incident.InjuryType?.ToString(),
            MedicalTreatmentProvided = incident.MedicalTreatmentProvided,
            EmergencyServicesContacted = incident.EmergencyServicesContacted,
            WitnessNames = incident.WitnessNames,
            ImmediateActionsTaken = incident.ImmediateActionsTaken,
            AttachmentsCount = incident.Attachments.Count,
            InvolvedPersonsCount = incident.InvolvedPersons.Count,
            CorrectiveActionsCount = incident.CorrectiveActions.Count,
            CreatedAt = incident.CreatedAt,
            CreatedBy = incident.CreatedBy,
            LastModifiedAt = incident.LastModifiedAt,
            LastModifiedBy = incident.LastModifiedBy
        };
    }
    
    private async Task InvalidateIncidentCaches(string reporterId)
    {
        await _cache.RemoveByTagAsync("incidents");
        _logger.LogInformation("Cache invalidated for incident update, reporter: {ReporterId}", reporterId);
    }
}