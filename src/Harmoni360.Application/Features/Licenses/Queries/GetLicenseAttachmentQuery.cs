using MediatR;

namespace Harmoni360.Application.Features.Licenses.Queries;

public record GetLicenseAttachmentQuery : IRequest<LicenseAttachmentFileResult?>
{
    public int LicenseId { get; init; }
    public int AttachmentId { get; init; }
}

public class LicenseAttachmentFileResult
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}