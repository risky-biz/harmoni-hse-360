using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarmoniHSE360.Application.Features.CorrectiveActions.Commands;

public record UpdateCorrectiveActionCommand : IRequest<Unit>
{
    public int Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public string AssignedToDepartment { get; init; } = string.Empty;
    public int? AssignedToId { get; init; }
    public DateTime DueDate { get; init; }
    public ActionPriority Priority { get; init; }
    public ActionStatus Status { get; init; }
    public string? CompletionNotes { get; init; }
}

public class UpdateCorrectiveActionCommandHandler : IRequestHandler<UpdateCorrectiveActionCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IIncidentAuditService _auditService;

    public UpdateCorrectiveActionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IIncidentAuditService auditService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Unit> Handle(UpdateCorrectiveActionCommand request, CancellationToken cancellationToken)
    {
        var correctiveAction = await _context.CorrectiveActions
            .FirstOrDefaultAsync(ca => ca.Id == request.Id, cancellationToken);

        if (correctiveAction == null)
        {
            throw new InvalidOperationException($"Corrective action with ID {request.Id} not found");
        }

        var currentUser = _currentUserService.Email ?? "System";

        // Handle status changes first
        if (request.Status == ActionStatus.Completed && correctiveAction.Status != ActionStatus.Completed)
        {
            correctiveAction.Complete(request.CompletionNotes ?? string.Empty, currentUser);
        }
        else if (request.Status == ActionStatus.InProgress && correctiveAction.Status == ActionStatus.Pending)
        {
            correctiveAction.StartProgress(currentUser);
        }

        // Update other properties
        correctiveAction.Update(
            request.Description,
            request.AssignedToDepartment,
            request.DueDate,
            request.Priority,
            currentUser,
            request.AssignedToId);

        // Log audit trail
        await _auditService.LogCorrectiveActionUpdatedAsync(correctiveAction.IncidentId, request.Description);
        
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}