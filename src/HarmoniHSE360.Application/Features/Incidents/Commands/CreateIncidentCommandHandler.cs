using MediatR;
using Microsoft.Extensions.Logging;
using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Application.Features.Incidents.DTOs;
using HarmoniHSE360.Domain.Entities;
using HarmoniHSE360.Domain.Interfaces;

namespace HarmoniHSE360.Application.Features.Incidents.Commands;

public class CreateIncidentCommandHandler : IRequestHandler<CreateIncidentCommand, IncidentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateIncidentCommandHandler> _logger;
    private readonly IIncidentAuditService _auditService;
    private readonly ICacheService _cache;

    public CreateIncidentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<CreateIncidentCommandHandler> logger,
        IIncidentAuditService auditService,
        ICacheService cache)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
        _auditService = auditService;
        _cache = cache;
    }

    public async Task<IncidentDto> Handle(CreateIncidentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating incident: {Title} for user {UserEmail}",
            request.Title, _currentUserService.Email);

        // Get current user details
        var user = await _context.Users.FindAsync(_currentUserService.UserId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException("Current user not found");
        }

        var incident = Incident.Create(
            request.Title,
            request.Description,
            request.Severity,
            request.IncidentDate,
            request.Location,
            user.Name,
            user.Email,
            user.Department,
            request.Latitude.HasValue && request.Longitude.HasValue 
                ? Domain.ValueObjects.GeoLocation.Create(request.Latitude.Value, request.Longitude.Value) 
                : null,
            user.Id
        );

        // Add witness information if provided
        if (!string.IsNullOrWhiteSpace(request.WitnessNames))
        {
            incident.AddWitnessInformation(request.WitnessNames);
        }

        // Record immediate actions if provided
        if (!string.IsNullOrWhiteSpace(request.ImmediateActionsTaken))
        {
            incident.RecordImmediateActions(request.ImmediateActionsTaken);
        }

        _context.Incidents.Add(incident);
        
        // We need to save first to get the incident ID
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Incident created successfully with ID: {IncidentId}", incident.Id);

        // Log audit trail for incident creation (this needs the incident ID)
        await _auditService.LogIncidentCreatedAsync(incident.Id);
        await _context.SaveChangesAsync(cancellationToken);

        // Invalidate incident list caches to ensure fresh data
        await InvalidateIncidentCaches(user.Id.ToString());
        
        _logger.LogInformation("Invalidated incident caches after creating incident {IncidentId}", incident.Id);
        
        // Return DTO
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
            WitnessNames = incident.WitnessNames,
            ImmediateActionsTaken = incident.ImmediateActionsTaken,
            CreatedAt = incident.CreatedAt,
            CreatedBy = incident.CreatedBy,
            AttachmentsCount = 0,
            InvolvedPersonsCount = 0,
            CorrectiveActionsCount = 0
        };
    }
    
    private async Task InvalidateIncidentCaches(string userId)
    {
        await _cache.RemoveByTagAsync("incidents");
        _logger.LogInformation("Cache invalidated for incident creation, user: {UserId}", userId);
    }
}