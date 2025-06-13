using Harmoni360.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Application.Features.WorkPermits.Commands
{
    public class DeleteWorkPermitCommandHandler : IRequestHandler<DeleteWorkPermitCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<DeleteWorkPermitCommandHandler> _logger;

        public DeleteWorkPermitCommandHandler(IApplicationDbContext context, ILogger<DeleteWorkPermitCommandHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Handle(DeleteWorkPermitCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting work permit with ID: {Id}", request.Id);

            var workPermit = await _context.WorkPermits
                .FirstOrDefaultAsync(wp => wp.Id == request.Id, cancellationToken);

            if (workPermit == null)
            {
                throw new InvalidOperationException($"Work permit with ID {request.Id} not found");
            }

            if (workPermit.Status != Domain.Enums.WorkPermitStatus.Draft)
            {
                throw new InvalidOperationException($"Only draft work permits can be deleted. Current status: {workPermit.Status}");
            }

            _context.WorkPermits.Remove(workPermit);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Work permit {Id} deleted successfully", request.Id);
        }
    }
}