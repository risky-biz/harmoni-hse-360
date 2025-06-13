using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WorkPermits.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Application.Features.WorkPermits.Commands
{
    public class CancelWorkPermitCommandHandler : IRequestHandler<CancelWorkPermitCommand, WorkPermitDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CancelWorkPermitCommandHandler> _logger;

        public CancelWorkPermitCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<CancelWorkPermitCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<WorkPermitDto> Handle(CancelWorkPermitCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Cancelling work permit {Id}", request.Id);

            var workPermit = await _context.WorkPermits
                .FirstOrDefaultAsync(wp => wp.Id == request.Id, cancellationToken);

            if (workPermit == null)
            {
                throw new InvalidOperationException($"Work permit with ID {request.Id} not found");
            }

            // Get current user details
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);

            var cancelledByName = currentUser?.Name ?? _currentUserService.Email ?? "System";
            var cancelledById = currentUser?.Id ?? _currentUserService.UserId;

            // Cancel the work permit
            workPermit.Cancel(
                cancelledBy: cancelledByName,
                reason: request.CancellationReason
            );

            _context.WorkPermits.Update(workPermit);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Work permit {Id} cancelled successfully", request.Id);

            // Map to DTO
            return new WorkPermitDto
            {
                Id = workPermit.Id,
                PermitNumber = workPermit.PermitNumber,
                Title = workPermit.Title,
                Description = workPermit.Description,
                Type = workPermit.Type.ToString(),
                Status = workPermit.Status.ToString(),
                Priority = workPermit.Priority.ToString(),
                WorkLocation = workPermit.WorkLocation,
                PlannedStartDate = workPermit.PlannedStartDate,
                PlannedEndDate = workPermit.PlannedEndDate,
                RequestedById = workPermit.RequestedById,
                RequestedByName = workPermit.RequestedByName,
                CreatedAt = workPermit.CreatedAt,
                UpdatedAt = workPermit.LastModifiedAt
            };
        }
    }
}