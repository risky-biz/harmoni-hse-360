using MediatR;

namespace Harmoni360.Application.Features.Inspections.Queries;

public class GetInspectionAttachmentQuery : IRequest<InspectionAttachmentFileResult>
{
    public int InspectionId { get; set; }
    public int AttachmentId { get; set; }
}

public class InspectionAttachmentFileResult
{
    public byte[] FileContent { get; set; } = null!;
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}