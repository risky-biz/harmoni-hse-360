using MediatR;

namespace Harmoni360.Application.Features.WorkPermits.Queries;

public class GetWorkPermitAttachmentQuery : IRequest<WorkPermitAttachmentFileResult>
{
    public int WorkPermitId { get; set; }
    public int AttachmentId { get; set; }
}

public class WorkPermitAttachmentFileResult
{
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}