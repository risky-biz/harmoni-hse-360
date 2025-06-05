using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Domain.Entities;
using HarmoniHSE360.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HarmoniHSE360.Application.Features.Incidents.Commands;

public class CreateQuickIncidentCommandHandler : IRequestHandler<CreateQuickIncidentCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateQuickIncidentCommandHandler> _logger;
    private readonly IFileStorageService _fileStorageService;

    public CreateQuickIncidentCommandHandler(
        IApplicationDbContext context,
        ILogger<CreateQuickIncidentCommandHandler> logger,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _logger = logger;
        _fileStorageService = fileStorageService;
    }

    public async Task<int> Handle(CreateQuickIncidentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating quick incident report via {Channel} by {Reporter}",
            request.ReportingChannel, request.IsAnonymous ? "Anonymous" : request.ReporterEmail);

        try
        {
            // Resolve location from QR code if provided
            var locationInfo = await ResolveLocationInfo(request.QrCodeId, request.Location, cancellationToken);

            // Create incident with appropriate reporter info
            var reporterName = request.IsAnonymous ? "Anonymous Reporter" : request.ReporterName;
            var reporterEmail = request.IsAnonymous ? GenerateAnonymousEmail() : request.ReporterEmail;
            var reporterDepartment = request.IsAnonymous ? "Anonymous" : await GetUserDepartment(request.UserId, cancellationToken);

            var incident = Incident.Create(
                title: request.Title,
                description: EnhanceDescription(request),
                severity: request.Severity,
                incidentDate: request.IncidentDate,
                location: locationInfo.DisplayName,
                reporterName: reporterName,
                reporterEmail: reporterEmail,
                reporterDepartment: reporterDepartment,
                geoLocation: CreateGeoLocation(request.Latitude, request.Longitude, locationInfo),
                reporterId: request.IsAnonymous ? null : ParseUserId(request.UserId)
            );

            _context.Incidents.Add(incident);
            await _context.SaveChangesAsync(cancellationToken);

            // Process quick photos if provided
            if (request.PhotoBase64.Any())
            {
                await ProcessQuickPhotos(incident, request.PhotoBase64, reporterEmail, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Successfully created quick incident {IncidentId} via {Channel}",
                incident.Id, request.ReportingChannel);

            return incident.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create quick incident via {Channel}", request.ReportingChannel);
            throw;
        }
    }

    private Task<LocationInfo> ResolveLocationInfo(string? qrCodeId, string fallbackLocation, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(qrCodeId))
        {
            // For now, simulate QR code location lookup
            // In production, this would query a QrCodeLocation table
            var locationName = ExtractLocationFromQrId(qrCodeId);

            return Task.FromResult(new LocationInfo
            {
                DisplayName = locationName,
                Latitude = null, // Would be populated from QR database
                Longitude = null
            });
        }

        return Task.FromResult(new LocationInfo
        {
            DisplayName = fallbackLocation ?? "Location not specified",
            Latitude = null,
            Longitude = null
        });
    }

    private string ExtractLocationFromQrId(string qrCodeId)
    {
        // Convert QR ID like "loc_building_a_floor_2" to readable location
        var parts = qrCodeId.Replace("loc_", "").Split('_');
        return string.Join(" ", parts.Select(part =>
            char.ToUpper(part[0]) + part.Substring(1)));
    }

    private string EnhanceDescription(CreateQuickIncidentCommand request)
    {
        var description = request.Description;

        // Add metadata about the report
        description += $"\n\n--- Report Metadata ---";
        description += $"\nReporting Channel: {request.ReportingChannel}";

        if (!string.IsNullOrEmpty(request.QrCodeId))
            description += $"\nQR Code ID: {request.QrCodeId}";

        if (!string.IsNullOrEmpty(request.DeviceInfo))
            description += $"\nDevice: {request.DeviceInfo}";

        if (request.IsAnonymous)
            description += $"\nAnonymous Report: Yes";

        description += $"\nReported At: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

        return description;
    }

    private async Task<string> GetUserDepartment(string? userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id))
            return "Unknown";

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return user?.Department ?? "Unknown";
    }

    private int? ParseUserId(string? userId)
    {
        return int.TryParse(userId, out var id) ? id : null;
    }

    private string GenerateAnonymousEmail()
    {
        return $"anonymous_{Guid.NewGuid():N}@harmoniHSE360.anonymous";
    }

    private GeoLocation? CreateGeoLocation(double? latitude, double? longitude, LocationInfo locationInfo)
    {
        var lat = latitude ?? locationInfo.Latitude;
        var lng = longitude ?? locationInfo.Longitude;

        if (lat.HasValue && lng.HasValue)
        {
            return GeoLocation.Create(lat.Value, lng.Value);
        }

        return null;
    }

    private async Task ProcessQuickPhotos(Incident incident, List<string> photoBase64List, string uploaderEmail, CancellationToken cancellationToken)
    {
        for (int i = 0; i < photoBase64List.Count && i < 10; i++) // Limit to 10 photos
        {
            try
            {
                var photoData = Convert.FromBase64String(photoBase64List[i]);
                var fileName = $"quick_photo_{incident.Id}_{i + 1}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.jpg";

                using var stream = new MemoryStream(photoData);
                var uploadResult = await _fileStorageService.UploadAsync(
                    stream,
                    fileName,
                    "image/jpeg",
                    "incident-photos");

                incident.AddAttachment(
                    fileName,
                    uploadResult.FilePath,
                    photoData.Length,
                    uploaderEmail);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process quick photo {Index} for incident {IncidentId}",
                    i + 1, incident.Id);
            }
        }
    }

    private class LocationInfo
    {
        public string DisplayName { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? BuildingId { get; set; }
        public string? FloorLevel { get; set; }
    }
}

