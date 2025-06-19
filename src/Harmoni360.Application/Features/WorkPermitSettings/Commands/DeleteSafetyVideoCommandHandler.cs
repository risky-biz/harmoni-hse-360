using Harmoni360.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.WorkPermitSettings.Commands;

public class DeleteSafetyVideoCommandHandler : IRequestHandler<DeleteSafetyVideoCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteSafetyVideoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteSafetyVideoCommand request, CancellationToken cancellationToken)
    {
        var safetyVideo = await _context.WorkPermitSafetyVideos
            .FirstOrDefaultAsync(sv => sv.Id == request.VideoId, cancellationToken);

        if (safetyVideo == null)
        {
            throw new KeyNotFoundException($"Safety Video with ID {request.VideoId} was not found.");
        }

        _context.WorkPermitSafetyVideos.Remove(safetyVideo);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}