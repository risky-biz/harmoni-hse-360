using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WorkPermits.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Application.Features.WorkPermits.Commands
{
    public class CompleteWorkCommandHandler : IRequestHandler<CompleteWorkCommand, WorkPermitDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CompleteWorkCommandHandler> _logger;

        public CompleteWorkCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, ILogger<CompleteWorkCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<WorkPermitDto> Handle(CompleteWorkCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Completing work for permit {Id}", request.Id);

            var workPermit = await _context.WorkPermits
                .FirstOrDefaultAsync(wp => wp.Id == request.Id, cancellationToken);

            if (workPermit == null)
            {
                throw new InvalidOperationException($"Work permit with ID {request.Id} not found");
            }

            // Get current user details
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);
                
            var completedBy = currentUser?.Name ?? _currentUserService.Email ?? "System";

            // Complete the work
            workPermit.CompleteWork(
                completedBy: completedBy,
                completionNotes: request.CompletionNotes,
                isCompletedSafely: request.IsCompletedSafely,
                lessonsLearned: request.LessonsLearned
            );

            _context.WorkPermits.Update(workPermit);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Work completed for permit {Id}", request.Id);

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
                ActualStartDate = workPermit.ActualStartDate,
                ActualEndDate = workPermit.ActualEndDate,
                RequestedById = workPermit.RequestedById,
                RequestedByName = workPermit.RequestedByName,
                CompletionNotes = workPermit.CompletionNotes,
                IsCompletedSafely = workPermit.IsCompletedSafely,
                LessonsLearned = workPermit.LessonsLearned,
                CreatedAt = workPermit.CreatedAt,
                UpdatedAt = workPermit.LastModifiedAt
            };
        }
    }
}