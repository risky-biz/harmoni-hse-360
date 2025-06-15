using MediatR;

namespace Harmoni360.Application.Features.Trainings.Commands;

public record DeleteTrainingAttachmentCommand : IRequest
{
    public int TrainingId { get; init; }
    public int AttachmentId { get; init; }
}