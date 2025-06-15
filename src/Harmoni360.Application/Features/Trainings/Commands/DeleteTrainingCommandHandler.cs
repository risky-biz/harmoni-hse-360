using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;

namespace Harmoni360.Application.Features.Trainings.Commands;

public class DeleteTrainingCommandHandler : IRequestHandler<DeleteTrainingCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteTrainingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteTrainingCommand request, CancellationToken cancellationToken)
    {
        var training = await _context.Trainings
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (training == null)
        {
            throw new InvalidOperationException($"Training with ID {request.Id} not found.");
        }

        // Check if training can be deleted (not started or completed)
        if (training.Status != Domain.Enums.TrainingStatus.Draft && 
            training.Status != Domain.Enums.TrainingStatus.Scheduled &&
            training.Status != Domain.Enums.TrainingStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot delete training that has started or is completed.");
        }

        _context.Trainings.Remove(training);
        await _context.SaveChangesAsync(cancellationToken);
    }
}