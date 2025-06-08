using Harmoni360.Domain.Entities;
using MediatR;

namespace Harmoni360.Application.Features.Incidents.Commands;

public class CreateQuickIncidentCommand : IRequest<int>
{
    public string? UserId { get; set; } // For anonymous reporting
    public string ReporterName { get; set; } = string.Empty;
    public string ReporterEmail { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; } = IncidentSeverity.Minor;
    public DateTime IncidentDate { get; set; } = DateTime.UtcNow;
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? QrCodeId { get; set; } // For location-based QR codes
    public string ReportingChannel { get; set; } = "Web"; // Web, QR, WhatsApp, Email, Anonymous
    public bool IsAnonymous { get; set; } = false;
    public string? DeviceInfo { get; set; }
    public List<string> PhotoBase64 { get; set; } = new(); // For mobile quick photos
}