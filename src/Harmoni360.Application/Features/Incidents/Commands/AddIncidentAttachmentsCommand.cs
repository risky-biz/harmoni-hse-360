using MediatR;
using Microsoft.AspNetCore.Http;

namespace Harmoni360.Application.Features.Incidents.Commands;

public class AddIncidentAttachmentsCommand : IRequest<AddIncidentAttachmentsResult>
{
    public int IncidentId { get; set; }
    public IFormFileCollection Files { get; set; } = null!;
    public string UploadedBy { get; set; } = string.Empty;
}

public class AddIncidentAttachmentsResult
{
    public List<AttachmentInfo> Attachments { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

public class AttachmentInfo
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}