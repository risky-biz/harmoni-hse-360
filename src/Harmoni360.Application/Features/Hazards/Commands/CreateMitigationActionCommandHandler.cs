using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Hazards.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Events;

namespace Harmoni360.Application.Features.Hazards.Commands;

public class CreateMitigationActionCommandHandler : IRequestHandler<CreateMitigationActionCommand, HazardMitigationActionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateMitigationActionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<HazardMitigationActionDto> Handle(CreateMitigationActionCommand request, CancellationToken cancellationToken)
    {
        // Verify hazard exists
        var hazard = await _context.Hazards
            .Include(h => h.Reporter)
            .Include(h => h.CurrentRiskAssessment)
            .FirstOrDefaultAsync(h => h.Id == request.HazardId, cancellationToken);

        if (hazard == null)
        {
            throw new ArgumentException($"Hazard with ID {request.HazardId} not found.");
        }

        // Verify assigned user exists
        var assignedUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.AssignedToId, cancellationToken);

        if (assignedUser == null)
        {
            throw new ArgumentException($"User with ID {request.AssignedToId} not found.");
        }

        // Create the mitigation action
        var mitigationAction = HazardMitigationAction.Create(
            hazardId: request.HazardId,
            actionDescription: request.ActionDescription,
            type: request.Type,
            targetDate: request.TargetDate,
            assignedToId: request.AssignedToId,
            priority: request.Priority,
            estimatedCost: request.EstimatedCost,
            requiresVerification: request.RequiresVerification
        );

        // Notes can be added during updates if needed - for now store in creation details

        // Add to context
        _context.HazardMitigationActions.Add(mitigationAction);

        // Update hazard status if this is the first mitigation action
        var existingActionsCount = await _context.HazardMitigationActions
            .CountAsync(ma => ma.HazardId == request.HazardId, cancellationToken);

        if (existingActionsCount == 0 && hazard.Status == HazardStatus.UnderAssessment)
        {
            hazard.UpdateStatus(HazardStatus.ActionRequired, _currentUserService.Name);
        }
        else if (hazard.Status == HazardStatus.ActionRequired)
        {
            hazard.UpdateStatus(HazardStatus.Mitigating, _currentUserService.Name);
        }

        // Save changes
        await _context.SaveChangesAsync(cancellationToken);

        // Note: Audit logging would be handled by domain events in a full implementation

        // Add domain events - events are already added by the entity's Create method
        // Additional event for hazard context
        hazard.AddMitigationAction(mitigationAction);

        // Note: Notification sending would be handled by domain event handlers in a full implementation

        // Create automatic follow-up reminder based on priority
        var reminderDate = CalculateReminderDate(request.TargetDate, request.Priority);
        // Reminder scheduling would be handled by a background service in a real implementation

        // Map to DTO
        var mitigationActionDto = new HazardMitigationActionDto
        {
            Id = mitigationAction.Id,
            HazardId = mitigationAction.HazardId,
            ActionDescription = mitigationAction.ActionDescription,
            Type = mitigationAction.Type.ToString(),
            Status = mitigationAction.Status.ToString(),
            Priority = mitigationAction.Priority.ToString(),
            TargetDate = mitigationAction.TargetDate,
            CompletedDate = mitigationAction.CompletedDate,
            AssignedToName = assignedUser.Name,
            CompletionNotes = mitigationAction.CompletionNotes,
            EstimatedCost = mitigationAction.EstimatedCost,
            ActualCost = mitigationAction.ActualCost,
            RequiresVerification = mitigationAction.RequiresVerification,
            VerifiedByName = mitigationAction.VerifiedBy?.Name,
            VerifiedAt = mitigationAction.VerifiedAt,
            VerificationNotes = mitigationAction.VerificationNotes,
            AssignedTo = new UserDto
            {
                Id = assignedUser.Id,
                Name = assignedUser.Name,
                Email = assignedUser.Email,
                Department = assignedUser.Department,
                Position = assignedUser.Position,
                EmployeeId = assignedUser.EmployeeId
            }
        };

        return mitigationActionDto;
    }

    private static DateTime? CalculateReminderDate(DateTime targetDate, MitigationPriority priority)
    {
        var daysBeforeTarget = priority switch
        {
            MitigationPriority.Critical => 1,  // 1 day before
            MitigationPriority.High => 3,     // 3 days before
            MitigationPriority.Medium => 7,   // 1 week before
            MitigationPriority.Low => 14,     // 2 weeks before
            _ => 7
        };

        var reminderDate = targetDate.AddDays(-daysBeforeTarget);
        
        // Only set reminder if it's in the future
        return reminderDate > DateTime.UtcNow ? reminderDate : null;
    }
}