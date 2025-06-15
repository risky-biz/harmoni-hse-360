using MediatR;

namespace Harmoni360.Application.Features.WasteReports.Commands;

public record DeleteWasteCommentCommand(int CommentId) : IRequest;