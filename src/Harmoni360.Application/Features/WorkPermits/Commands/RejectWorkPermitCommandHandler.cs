using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WorkPermits.DTOs;
using Harmoni360.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Application.Features.WorkPermits.Commands
{
    public class RejectWorkPermitCommandHandler : IRequestHandler<RejectWorkPermitCommand, WorkPermitDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<RejectWorkPermitCommandHandler> _logger;

        public RejectWorkPermitCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<RejectWorkPermitCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<WorkPermitDto> Handle(RejectWorkPermitCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Rejecting work permit {Id}", request.Id);

            var workPermit = await _context.WorkPermits
                .Include(wp => wp.Approvals)
                .FirstOrDefaultAsync(wp => wp.Id == request.Id, cancellationToken);

            if (workPermit == null)
            {
                throw new InvalidOperationException($"Work permit with ID {request.Id} not found");
            }

            // Get current user details
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);

            if (currentUser == null)
            {
                throw new InvalidOperationException("Current user not found");
            }

            // Create rejection approval record
            var approval = WorkPermitApproval.Create(
                workPermitId: workPermit.Id,
                approvedById: currentUser.Id,
                approvedByName: currentUser.Name, // Using Name property instead of FullName
                approvalLevel: "Supervisor",
                isApproved: false,
                comments: request.RejectionReason
            );

            // Reject the work permit
            workPermit.Reject(
                rejectedById: currentUser.Id,
                rejectedByName: currentUser.Name,
                reason: request.RejectionReason
            );

            _context.WorkPermitApprovals.Add(approval);
            _context.WorkPermits.Update(workPermit);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Work permit {Id} rejected successfully", request.Id);

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