using MediatR;

namespace HarmoniHSE360.Application.Features.Incidents.Queries;

public class GetIncidentAttachmentsQuery : IRequest<List<IncidentAttachmentDto>>
{
    public int IncidentId { get; set; }
}

public class IncidentAttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string FileSizeFormatted { get; set; } = string.Empty;
}