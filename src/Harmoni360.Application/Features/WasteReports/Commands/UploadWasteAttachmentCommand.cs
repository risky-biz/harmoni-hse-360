using MediatR;
using Microsoft.AspNetCore.Http;

namespace Harmoni360.Application.Features.WasteReports.Commands;

public record UploadWasteAttachmentCommand(
    int WasteReportId,
    IFormFile File) : IRequest<WasteAttachmentDto>;

public class WasteAttachmentDto
{
    public int Id { get; set; }
    public int WasteReportId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}