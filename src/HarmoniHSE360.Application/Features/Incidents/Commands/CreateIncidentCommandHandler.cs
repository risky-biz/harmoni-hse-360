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
    private readonly ILogger<CreateIncidentCommandHandler> _logger;

    public CreateIncidentCommandHandler(
        IApplicationDbContext context,
        ILogger<CreateIncidentCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IncidentDto> Handle(CreateIncidentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating incident: {Title} for reporter {ReporterId}",
            request.Title, request.ReporterId);

        var incident = Incident.Create(
            request.Title,
            request.Description,
            request.Severity,
            request.IncidentDate,
            request.Location,
            request.ReporterId,
            "System" // TODO: Get actual username from current user service
        );

        // Set GPS coordinates if provided
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            incident.SetGeoLocation(request.Latitude.Value, request.Longitude.Value);
        }

        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Incident created successfully with ID: {IncidentId}", incident.Id);

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
            Reporter = new UserDto
            {
                Id = incident.ReporterId,
                Name = "Current User", // TODO: Get actual user name from context
                Email = "",
                Department = "",
                Position = ""
            },
            CreatedAt = incident.CreatedAt,
            AttachmentCount = 0,
            InvolvedPersonCount = 0,
            CorrectiveActionCount = 0
        };
    }
}