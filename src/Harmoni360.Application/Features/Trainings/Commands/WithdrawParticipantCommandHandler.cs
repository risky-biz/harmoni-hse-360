using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;

namespace Harmoni360.Application.Features.Trainings.Commands;

public class WithdrawParticipantCommandHandler : IRequestHandler<WithdrawParticipantCommand>
{
    private readonly IApplicationDbContext _context;

    public WithdrawParticipantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(WithdrawParticipantCommand request, CancellationToken cancellationToken)
    {
        var participant = await _context.TrainingParticipants
            .FirstOrDefaultAsync(p => p.Id == request.ParticipantId && 
                                    p.TrainingId == request.TrainingId, cancellationToken);

        if (participant == null)
        {
            throw new InvalidOperationException($"Training participant with ID {request.ParticipantId} not found.");
        }

        participant.MarkAsWithdrawn();

        await _context.SaveChangesAsync(cancellationToken);
    }
}