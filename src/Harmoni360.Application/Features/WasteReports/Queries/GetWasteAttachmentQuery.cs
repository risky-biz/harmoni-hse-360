using MediatR;

namespace Harmoni360.Application.Features.WasteReports.Queries;

public record GetWasteAttachmentQuery(int AttachmentId) : IRequest<WasteAttachmentDownloadDto>;

public class WasteAttachmentDownloadDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] FileData { get; set; } = Array.Empty<byte>();
}