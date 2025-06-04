using HarmoniHSE360.Domain.Entities;
using MediatR;

namespace HarmoniHSE360.Application.Features.Incidents.Commands;

public class CreateIncidentFromEmailCommand : IRequest<int>
{
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public List<EmailAttachmentDto> Attachments { get; set; } = new();
    public DateTime ReceivedAt { get; set; }
    public string? Location { get; set; }
    public IncidentSeverity? DetectedSeverity { get; set; }
}

public class EmailAttachmentDto
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
}