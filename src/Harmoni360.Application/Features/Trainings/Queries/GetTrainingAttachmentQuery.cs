using MediatR;

namespace Harmoni360.Application.Features.Trainings.Queries;

public record GetTrainingAttachmentQuery : IRequest<TrainingAttachmentFileDto?>
{
    public int TrainingId { get; init; }
    public int AttachmentId { get; init; }
}

public class TrainingAttachmentFileDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    public long FileSize { get; set; }
}