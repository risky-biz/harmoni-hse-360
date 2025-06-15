using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WorkPermits.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Application.Features.WorkPermits.Commands
{
    public class StartWorkCommandHandler : IRequestHandler<StartWorkCommand, WorkPermitDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<StartWorkCommandHandler> _logger;

        public StartWorkCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, ILogger<StartWorkCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<WorkPermitDto> Handle(StartWorkCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting work for permit {Id}", request.Id);

            var workPermit = await _context.WorkPermits
                .FirstOrDefaultAsync(wp => wp.Id == request.Id, cancellationToken);

            if (workPermit == null)
            {
                throw new InvalidOperationException($"Work permit with ID {request.Id} not found");
            }

            // Start the work
            workPermit.StartWork(_currentUserService.Name);

            _context.WorkPermits.Update(workPermit);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Work started for permit {Id}", request.Id);

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
                RequestedById = workPermit.RequestedById,
                RequestedByName = workPermit.RequestedByName,
                CreatedAt = workPermit.CreatedAt,
                UpdatedAt = workPermit.LastModifiedAt
            };
        }
    }
}