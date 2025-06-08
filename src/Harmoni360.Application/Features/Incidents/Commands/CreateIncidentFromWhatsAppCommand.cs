using Harmoni360.Domain.Entities;
using MediatR;

namespace Harmoni360.Application.Features.Incidents.Commands;

public class CreateIncidentFromWhatsAppCommand : IRequest<int>
{
    public string FromPhoneNumber { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string MessageBody { get; set; } = string.Empty;
    public List<WhatsAppMediaDto> MediaAttachments { get; set; } = new();
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public string? MessageId { get; set; }
    public string? ChatId { get; set; }
}

public class WhatsAppMediaDto
{
    public string MediaId { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty; // image, video, document, audio
    public string? Caption { get; set; }
    public byte[]? MediaData { get; set; }
    public string? FileName { get; set; }
    public string? MimeType { get; set; }
    public long? FileSize { get; set; }
}