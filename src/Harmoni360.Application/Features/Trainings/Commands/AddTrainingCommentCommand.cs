using MediatR;
using Harmoni360.Application.Features.Trainings.DTOs;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Trainings.Commands;

public record AddTrainingCommentCommand : IRequest<TrainingCommentDto>
{
    public int TrainingId { get; init; }
    public string Content { get; init; } = string.Empty;
    public TrainingCommentType CommentType { get; init; } = TrainingCommentType.General;
    public int? ParentCommentId { get; init; }
    public bool IsPublic { get; init; } = true;
    public bool IsInstructorOnly { get; init; } = false;
    public bool IsPrivateNote { get; init; } = false;
    public bool IsImportant { get; init; } = false;
    public bool RequiresResponse { get; init; } = false;
}