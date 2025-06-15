using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities.Waste;

namespace Harmoni360.Application.Features.WasteReports.Commands;

public class DeleteWasteCommentCommandHandler : IRequestHandler<DeleteWasteCommentCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteWasteCommentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteWasteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _context.WasteComments
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);

        if (comment == null)
        {
            throw new InvalidOperationException($"Comment with ID {request.CommentId} not found");
        }

        _context.WasteComments.Remove(comment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}